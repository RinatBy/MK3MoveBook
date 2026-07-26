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

Stop the MoveBook service before copying the SQLite database, then start it
again immediately. Keep the previous release directory and change only the
`current` symlink when rolling back code. Never modify or restart unrelated VDS
services during either operation.
