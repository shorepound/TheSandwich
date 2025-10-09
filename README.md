# TheSandwich

Developer README — quick start

This repository contains two main parts:

- `BackOfTheHouse` — ASP.NET Core backend (net10.0)
- `FrontOfTheHouse` — Angular frontend (dev server + proxy to backend)

Quick setup (local dev)

1. Backend

   - Required: .NET 10 SDK (preview) and Docker if you plan to run the SQL container.
   - If you want to use the Docker SQL database, set the environment variable `DOCKER_DB_CONNECTION` before running the backend. Example:

```
export DOCKER_DB_CONNECTION='Server=127.0.0.1,1433;Database=sandwich_app;User Id=sa;Password=MyStrongPass123;TrustServerCertificate=True;'
dotnet run --project BackOfTheHouse.csproj
```

   - If `DOCKER_DB_CONNECTION` is not set, the backend will use a local SQLite fallback (`Data/sandwich.db`). Note: options endpoints (breads/cheeses/etc.) require the Docker DB — when not configured the API will return 503 for those endpoints and the frontend will show a friendly message.

2. Frontend

   - Move to `FrontOfTheHouse` and run the dev server (uses the proxy to the backend):

```
cd FrontOfTheHouse
npm install
ng serve --proxy-config proxy.conf.json --host 0.0.0.0
```

   - The dev proxy forwards `/api` to the backend at `http://127.0.0.1:5251` so the frontend can call the API without CORS issues during development.

What I changed recently

- Moved some component styles into the global `src/styles.css` for consistency.
- Added UI polish to the builder form: disabled submit until all selections are made, per-list retry buttons, and toast messages.
- Implemented server-side validation for `/api/builder`; it returns `400` with field-level errors (JSON: `{ errors: { breadId: 'Bread not found', ... } }`). The frontend maps those to inline messages.
- Made `OptionsController` resilient to missing Docker DB by returning `503` if the Docker DB is not configured.
- Removed `bin/` and `obj/` build artifacts from the repository and added them to `.gitignore`.
- Added `backofthehouse.pid` to `.gitignore` and removed it from the repo. Consider adding `front-dev.pid` or other runtime files if you use them.

Git notes

- Keep the `FrontOfTheHouse` submodule updated by committing and pushing within that submodule when you change frontend code; the top-level repo records the submodule pointer.

Ignored files (high level)

- `bin/`, `obj/` (build outputs)
- `*.log`, `*.pid` (runtime files) — the repo now ignores `backofthehouse.pid`.
- `node_modules/`, `FrontOfTheHouse/dist/`

If you'd like, I can add a short CONTRIBUTING.md with common dev commands and code review guidelines.
