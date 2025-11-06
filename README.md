# TheSandwich

A full-stack sandwich ordering application demonstrating modern web development practices.

## 🥪 Overview

```markdown
# TheSandwich

Build and showcase modern full‑stack patterns with a delightful sandwich ordering demo — great for prototypes, demos, and hands‑on workshops. Run it locally in minutes and iterate fast.

## 🧭 Table of contents

- Overview
- Recent work
- Quick local start
- Scripts & tooling
- Local development (backend / frontend)
- Deployment notes
- API endpoints
- Contributing

## Overview

This repository contains the backend API and the frontend UI for a sandwich builder and ordering demo:

- `BackOfTheHouse` — ASP.NET Core backend (targets .NET 10) with EF Core
- `FrontOfTheHouse` — Angular frontend (kept as a git submodule)

The project is optimized for local development: quick startup scripts, an SQLite development database, and helpful troubleshooting helpers.


## Recent highlights

What's new and noteworthy:

- Polished UX: replaced blocking alerts with unobtrusive toasts and streamlined the Builder UI for faster, more pleasant interactions.
- Seed additions: introduced the "Tempeh" option so it's visible via `GET /api/options/meats`.
- Container-ready: prepared a multi-stage `Dockerfile`, `docker-compose.yml`, and `infra/` nginx + certbot templates to run the app as containers.
- Faster deploys: added `deploy/prebuild.sh` and `deploy/deploy.sh` to package a locally-built frontend and deploy it to a host without doing a slow remote Angular build.

Note: some deployment artifacts were created during development. If they are not present on `main` and you want them restored, I can reintroduce them on a separate branch so `main` remains clean.

## Quick local start

The repository includes convenience scripts to boot the app for local development.

From the repo root:

```bash
# start backend + frontend in development mode
./dev-start-all.sh
```

Then browse:

- Frontend: http://localhost:4200
- Backend API: http://localhost:5251 (Swagger available in development)

## Scripts & tooling

- `./dev-start-all.sh` — Start frontend and backend for local dev
- `./dev-stop-all.sh` — Stop development services cleanly
- `./dev-status.sh` — Show service status, PIDs and quick log pointers
- `./dev-clean-build.sh` — Clean caches and perform fresh builds (use `--full` to reinstall frontend deps)

These scripts are intended to hide platform-specific setup and speed up iteration.

## Local development

Backend (simple, uses SQLite by default):

```bash
dotnet run --project BackOfTheHouse.csproj
```
```markdown
# TheSandwich

A full-stack sandwich ordering demo combining an ASP.NET Core backend with an Angular frontend. Designed for rapid local development, prototyping, and demos.

## Table of contents

- Overview
- Recent highlights
- Quick start
- Scripts & tooling
- Local development
- Deployment notes
- API endpoints
- Contributing

## Overview

Folders of interest:

- `BackOfTheHouse` — ASP.NET Core backend (targets .NET 10) with EF Core
- `FrontOfTheHouse` — Angular frontend (kept as a git submodule)

This project is set up for quick iteration with an SQLite development database and helper scripts to start, stop, and inspect both services.

## Recent highlights

- UX: non-blocking toasts replaced alert dialogs and the Builder UI was simplified for faster flows.
- Seed data: "Tempeh" was added to the meats/options seed so it appears in API responses.
- Containerization: a multi-stage `Dockerfile`, `docker-compose.yml`, and `infra/nginx` templates were prepared for container deployments.
- Prebuild deploy flow: `deploy/prebuild.sh` and `deploy/deploy.sh` support packaging a locally-built frontend and deploying it to a host without running a remote Angular build.

## Quick start

Start backend and frontend for development:

```bash
# from repository root
./dev-start-all.sh
```

Open:

- Frontend: http://localhost:4200
- Backend API: http://localhost:5251 (Swagger available in development)

## Scripts & tooling

- `./dev-start-all.sh` — start both services for development
- `./dev-stop-all.sh` — stop development services
- `./dev-status.sh` — show service status, PIDs and quick log pointers
- `./dev-clean-build.sh` — clean caches and perform fresh builds (`--full` reinstalls frontend deps)

## Local development

Backend (SQLite fallback):

```bash
dotnet run --project BackOfTheHouse.csproj
```

Frontend:

```bash
cd FrontOfTheHouse
npm install
npm start
```

If the `FrontOfTheHouse` submodule is not initialized:

```bash
git submodule update --init --recursive
```

## Deployment notes

- The multi-stage `Dockerfile` builds the Angular app and publishes the backend with the frontend assets copied into `wwwroot`.
- `docker-compose.yml` is prepared to run the backend behind `nginx` with `certbot` webroot integration.
- To avoid slow remote builds on small servers, use the prebuild flow: build the frontend locally with `deploy/prebuild.sh`, copy the resulting tarball to the host, and run `docker compose --no-build` on the server.

## API endpoints (selected)

- `GET /api/sandwiches`
- `GET /api/sandwiches/{id}`
- `POST /api/builder`
- `GET /api/options/meats`

Refer to the `Controllers/` folder for the full API surface.

## Contributing

1. Fork the repository
2. Create a branch: `git checkout -b feature/your-feature`
3. Implement and test locally
4. Open a Pull Request

## License

See the repository LICENSE or include licensing terms as appropriate.

``` 
# Backend:  http://localhost:5251
```

3. Use `./dev-status.sh` to quickly check health, logs, and PIDs.

4. If the frontend behaves oddly, try the full clean:

```bash
./dev-clean-build.sh --full
```

### Troubleshooting tips (developer-focused)

- If `dotnet` commands fail due to multiple projects in the root, build or run the intended project explicitly: `dotnet run --project BackOfTheHouse.csproj`.
- If the frontend dev server reports stale artifacts, remove `FrontOfTheHouse/dist` and restart or run the full clean.
- If you need deterministic frontend installs, switch the `dev-clean-build.sh` `npm install` line to `npm ci` and commit a lockfile.


### Environment Configuration

Create a `.env` file in the root directory:
```env
# Optional: SQL Server connection (fallback to SQLite if not set)
DOCKER_DB_CONNECTION="Server=127.0.0.1,1433;Database=sandwich_app;User Id=sa;Password=MyStrongPass123;TrustServerCertificate=True;"
```

### Useful Commands

```bash
# 📊 Quick status check
./dev-status.sh

# 🛑 Stop all services cleanly
./dev-stop-all.sh

# 🔨 Build the application
dotnet build BackOfTheHouse.csproj

# 🗄️ Run database migrations (if using SQL Server)
dotnet ef database update --context DockerSandwichContext

# 👀 View real-time logs
tail -f backofthehouse.log      # Backend logs
tail -f FrontOfTheHouse/front-dev.log  # Frontend logs

# 🔍 Check what's running on development ports
lsof -i :4200 -i :5251

# 🧹 Clean restart (stops and starts fresh)
./dev-stop-all.sh && ./dev-start-all.sh
```

## 📡 API Endpoints

### Sandwiches
- `GET /api/sandwiches` - List all sandwiches
- `GET /api/sandwiches/{id}` - Get sandwich by ID
- `PUT /api/sandwiches/{id}` - Update sandwich
- `DELETE /api/sandwiches/{id}` - Delete sandwich
- `POST /api/sandwiches/backfill-prices` - Set null prices to 0.00

### Sandwich Builder
- `POST /api/builder` - Create custom sandwich

### Options (Ingredients)
- `GET /api/options/breads` - Available bread types
- `GET /api/options/cheeses` - Available cheeses
- `GET /api/options/dressings` - Available dressings
- `GET /api/options/meats` - Available meats
- `GET /api/options/toppings` - Available toppings

## 🔧 Troubleshooting

### Common Issues

1. **Port Already in Use**
   ```bash
   # Use the stop script for clean shutdown
   ./dev-stop-all.sh
   
   # Or manually kill processes on development ports
   lsof -ti:4200 -ti:5251 | xargs kill
   ```

2. **Services Won't Start**
   ```bash
   # Check detailed status
   ./dev-status.sh
   
   # Verify prerequisites are installed
   dotnet --version && node --version && npm --version
   ```

3. **Database Schema Mismatch**
   ```bash
   # Reset SQLite database
   rm Data/sandwich.db*
   ./dev-start-all.sh
   ```

4. **Frontend Build Errors**
   ```bash
   cd FrontOfTheHouse
   rm -rf node_modules package-lock.json
   npm install
   ```

5. **Stale Processes**
   ```bash
   # Clean shutdown and restart
   ./dev-stop-all.sh
   ./dev-start-all.sh
   ```

## 🐛 Git Notes

- **Frontend Submodule**: The `FrontOfTheHouse` directory is a Git submodule
- **Submodule Updates**: Commit and push within the submodule directory for frontend changes
- **Main Repository**: Records the submodule pointer to track frontend versions

## 📁 Ignored Files

Development and build artifacts are automatically ignored:
- `bin/`, `obj/` - Build outputs
- `*.log`, `*.pid` - Runtime and process files
- `node_modules/`, `FrontOfTheHouse/dist/` - Dependencies and build artifacts
- `Data/sandwich.db*` - Local SQLite database files

## 🚀 Deployment

The application is configured for deployment with:
- **CI/CD Pipeline**: GitHub Actions workflow (`.github/workflows/ci.yml`)
- **Production Build**: Angular SSR build copied to `wwwroot`
- **Environment Variables**: Configure `DOCKER_DB_CONNECTION` for production database

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

Inspired by https://github.com/dansinker/tacofancy
