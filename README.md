# Shopping Basket API

ASP.NET Core 10 API for managing shopping baskets. Uses PostgreSQL, JWT auth with refresh token rotation, FluentValidation, and Serilog.

## Running locally

Requires Docker.

```bash
docker compose up --build
```

Migrations apply automatically on startup. The API URL depends on your Docker runtime:

- **OrbStack**: `https://api-demo.api-demo.orb.local`
- **Docker Desktop / other**: `http://localhost:8080`

Open `<your-url>/swagger` for the Swagger UI.

## Endpoints

### Auth — `/api/v1/auth`

```
POST /signup                    register
POST /login                     login, returns access + refresh token
POST /logout                    revoke refresh token
POST /refresh                   rotate token pair
POST /password-reset/request    send reset token
POST /password-reset/confirm    apply new password
```

### Baskets — `/api/v1/baskets` (JWT required)

```
POST   /                            create basket
GET    /                            search with pagination
GET    /{id}                        get by id
PUT    /{id}                        update name
DELETE /{id}                        soft delete

POST   /{basketId}/items            add item
PUT    /{basketId}/items/{itemId}   update quantity
DELETE /{basketId}/items/{itemId}   remove item
```

## Project structure

Organised as vertical slices. Each feature group contains controller, each feature folder contains its own handler, request/response types, and validator.

```
Features/
  Auth/
    Signup, Login, Logout, RefreshToken, PasswordReset
  Baskets/
    Create, GetById, Search, Update, Delete, AddItem, UpdateItem, RemoveItem
Infrastructure/
  Persistence/   EF Core context, entity configs, migrations
  Services/      JWT token service, BCrypt password hasher
Common/
  Middleware/    Global exception handling, correlation ID
  Models/        ApiErrorResponse, PagedResponse
Domain/
  Entities/      User, Basket, BasketItem, RefreshToken, PasswordResetToken
```

## Auth flow

Signup/login returns an access token (15 min) and a refresh token (7 days). Use the access token as `Bearer` in the `Authorization` header. On expiry, call `/refresh`, old token is revoked, new pair issued. Login revokes all existing refresh tokens for the user.

## Error responses

All errors follow the same shape:

```json
{
  "code": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred.",
  "errors": {
    "Name": ["'Name' must not be empty."]
  }
}
```

Codes: `VALIDATION_ERROR` (400), `UNAUTHORIZED` (401), `NOT_FOUND` (404), `CONFLICT` (409), `INTERNAL_ERROR` (500).

## Postman

A collection and environment are included in the `postman/` folder.

**Import:** In Postman, click Import and load both `api-demo.postman_collection.json` and `api-demo.postman_environment.json`.

**Set the URL:** Select the `api-demo-local` environment and update `baseUrl` to match your running container URL (see above).

**Run:** Right-click the `api-demo API` collection -> Run collection -> confirm environment is `api-demo-local` -> Run. Requests must run in order, later ones depend on variables (`basketId`, `itemId`, etc.) set by earlier ones. Variables like `accessToken`, `refreshToken`, and `basketId` are captured automatically by the collection scripts. Signup generates a unique email each run so it's safe to repeat.

## Notable decisions

- **Soft deletes** on baskets via `IsDeleted` + EF global query filter
- **Refresh token rotation**: old token revoked on every refresh, login revokes all existing tokens for the user
- **Password reset token stored as hash**: raw token only returned once (in response for demo purposes), hash stored in DB to prevent exposure
- **Static mappers** instead of AutoMapper: explicit, no hidden config or runtime reflection
- **Domain validation in entities**: `Basket` enforces business rules (e.g. duplicate item numbers) directly, not just at the API boundary
- **Correlation IDs**: `X-Correlation-Id` header propagated through logs and response, auto-generated if not provided
- **BCrypt work factor 12** for password hashing
- **Auto-migrations in development**: runs `MigrateAsync()` on startup so the database is always in sync locally 
- **Secrets management via vaults**: connection strings, JWT secrets, and other sensitive variables must be stored and retrieved from a secure secret store (e.g. HashiCorp Vault). **The current approach** (appsettings) is for demo purposes only.
