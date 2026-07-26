# Discord authentication flow

This flow is used by the Portable desktop client. Portable never receives the
Discord client secret.

## Flow

1. Desktop calls `POST /api/v1/auth/device-sessions`.
2. The server returns a high-entropy `device_code` and a separate browser URL.
3. Desktop keeps `device_code` private and opens the URL in the system browser.
4. The server generates an OAuth `state` nonce and redirects the browser to
   Discord using the Authorization Code Grant and only the `identify` scope.
5. Discord redirects to `/api/v1/auth/discord/callback`. The server validates
   `state`, exchanges the code using the server-only client secret, reads
   `/users/@me`, and links `(provider=discord, subject=Discord user id)` to an
   internal MoveBook user.
6. Desktop polls `POST /api/v1/auth/device-sessions/token` with `device_code`.
7. A pending login returns HTTP 202. A completed login returns a short MoveBook
   access JWT and a rotatable MoveBook refresh token exactly once.

The Discord OAuth2 URL Generator is not part of this sign-in flow. The server
creates a fresh authorization URL for every login so it can bind a one-time
`state` value to the device session. The `bot` and `applications.commands`
scopes are intentionally not requested; they belong to a future, separate bot
installation flow if MoveBook later needs server commands.

## Security properties

- Device, browser, OAuth state, and refresh secrets are generated independently.
- Only SHA-256 hashes of opaque device and refresh secrets are stored.
- The browser URL cannot retrieve MoveBook tokens; only the desktop-held
  `device_code` can poll them.
- OAuth state is single-use and bound to one short-lived login session.
- The temporary Discord access token is revoked after reading the profile;
  Discord tokens are never stored by MoveBook.
- Real Discord credentials and the MoveBook JWT secret belong only in `.env`
  or production secret storage and are excluded from Git.
- A completed device session can issue tokens only once. Refresh rotation
  revokes the previous MoveBook refresh token.
- Portable encrypts the rotatable MoveBook refresh token with Windows DPAPI for
  the current Windows user. Closing the book keeps the session; explicit logout
  revokes it and removes the local encrypted copy.

The current achievement model treats P1 as the authenticated account. Portable
sends normalized MAME events to the server after sign-in; the server evaluates
and stores the resulting progress as `unverified-local`.
