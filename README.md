# TheSandwich

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

   Developer helper

   If you prefer a small helper that stops any process on :4200 and starts the frontend dev server while capturing logs, run this from the repo root:

   ```
   ./FrontOfTheHouse/dev-serve.sh
   ```

   Start everything (backend + frontend)

   There is a small automation script at the repository root that restarts the backend and frontend, waits for both to bind, and tails logs:

   ```
   ./dev-start-all.sh
   ```

   You can optionally provide a Docker connection string (sets `DOCKER_DB_CONNECTION`) as the first argument or place it in a `.env` file as `DOCKER_DB_CONNECTION=...`:

   ```
   ./dev-start-all.sh "Server=127.0.0.1,1433;Database=sandwich_app;User Id=sa;Password=MyStrongPass123;TrustServerCertificate=True;"
   ```


Git notes

- Keep the `FrontOfTheHouse` submodule updated by committing and pushing within that submodule when you change frontend code; the top-level repo records the submodule pointer.

Ignored files (high level)

- `bin/`, `obj/` (build outputs)
- `*.log`, `*.pid` (runtime files) — the repo now ignores `backofthehouse.pid`.
- `node_modules/`, `FrontOfTheHouse/dist/`
