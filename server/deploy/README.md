# Production deployment

The production API is isolated from other VDS applications:

- system user: `movebook`;
- source: `/opt/movebook-server/releases/<git revision>`;
- current release symlink: `/opt/movebook-server/current`;
- shared virtual environment: `/opt/movebook-server/venv`;
- secrets: `/etc/movebook-server.env`, mode `0640`, owner `root:movebook`;
- SQLite data: `/var/lib/movebook-server/movebook.db`, mode `0700` directory;
- private upstream: `127.0.0.1:8020`;
- public endpoint: `https://api.movebook.ru` through Nginx.

Production Portable builds use `https://api.movebook.ru` by default. Local
development can override it with
`MOVEBOOK_API_BASE_URL=http://127.0.0.1:8010`. The desktop host passes the
normalized API address to the web UI so cached progress is scoped to one
server and cannot cross between local and production user IDs.

The service runs one Uvicorn worker. SQLite uses foreign keys, WAL mode and a
five-second busy timeout. Existing services and the PostgreSQL instance on the
VDS are not reused.

## Required external configuration

1. Add DNS A record `api.movebook.ru -> 176.124.218.23`.
2. Keep the local Discord redirect and add this production redirect:
   `https://api.movebook.ru/api/v1/auth/discord/callback`.
3. Never copy a development JWT secret to production. Generate a new secret on
   the VDS and keep it only in `/etc/movebook-server.env`.

## TLS bootstrap

Install the bootstrap Nginx configuration first. It exposes only the ACME
challenge directory and returns 404 for every other request. After DNS resolves,
obtain the certificate with Certbot's webroot mode, replace the bootstrap file
with `nginx-api.movebook.ru.conf`, run `nginx -t`, and reload Nginx.

The final access log deliberately records `$uri` instead of `$request_uri` so
one-time browser codes, Discord authorization codes and OAuth state values are
not written to disk. Uvicorn access logging is also disabled.

## Health checks

Before enabling Nginx:

```bash
curl --fail --silent --show-error http://127.0.0.1:8020/api/v1/health
```

After TLS is enabled:

```bash
curl --fail --silent --show-error https://api.movebook.ru/api/v1/health
```

`GET /api/v1/achievements/me` must return `401` without a bearer token. This is
the expected public smoke-test result.

## Backup and rollback

Install the backup utility and timer without restarting the API:

```bash
install -d -o root -g root -m 0755 /usr/local/libexec/movebook-server
install -o root -g root -m 0755 \
  deploy/backup_sqlite.py \
  /usr/local/libexec/movebook-server/backup_sqlite.py
install -d -o movebook -g movebook -m 0700 \
  /var/backups/movebook-server
install -o root -g root -m 0644 \
  deploy/movebook-server-backup.service \
  /etc/systemd/system/movebook-server-backup.service
install -o root -g root -m 0644 \
  deploy/movebook-server-backup.timer \
  /etc/systemd/system/movebook-server-backup.timer
systemctl daemon-reload
systemctl enable --now movebook-server-backup.timer
systemctl start movebook-server-backup.service
```

The timer runs daily between 03:20 and 03:40 server time and keeps 30 days of
snapshots in `/var/backups/movebook-server`. The utility uses SQLite's online
backup API, verifies the new copy with `PRAGMA integrity_check`, publishes it
atomically with mode `0600`, and only then removes expired managed snapshots.
The API does not need to be stopped.

Check the timer and the newest snapshot:

```bash
systemctl list-timers --all movebook-server-backup.timer --no-pager
systemctl show movebook-server-backup.service \
  -p Result -p ExecMainStatus -p ActiveState
latest="$(find /var/backups/movebook-server -maxdepth 1 -type f \
  -name 'movebook-*.sqlite3' -printf '%T@ %p\n' | sort -nr | head -n1 | cut -d' ' -f2-)"
sqlite3 -readonly "$latest" 'PRAGMA integrity_check;'
```

The oneshot service returns to `ActiveState=inactive` after each run. A
successful completed backup reports `Result=success` and `ExecMainStatus=0`.

These snapshots protect against application errors and accidental database
changes. Because they remain on the same VDS, a later off-host copy is still
required to protect against complete server or disk loss.

For a code rollback, keep the previous release directory and change only the
`current` symlink. Never modify or restart unrelated VDS services during backup
or rollback operations.
