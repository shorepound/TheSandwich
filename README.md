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

Frontend (from submodule):

```bash
cd FrontOfTheHouse
npm install
npm start
```

If `FrontOfTheHouse` is missing (submodule not initialized), run:

```bash
git submodule update --init --recursive
```

## Deployment notes (short)

- A multi-stage `Dockerfile` and `docker-compose.yml` were prepared to run the app behind `nginx` and use `certbot` for TLS.
- To avoid long remote Angular builds on small servers, the repo contains a local prebuild workflow (`deploy/prebuild.sh`) that packages the `FrontOfTheHouse` `dist/` output into a tarball which can be copied to the host and started with `docker compose --no-build`.
- If you want those files re-added to `main`, or committed to a `deploy/` branch, I can add them and document the exact steps to provision an EC2 instance and attach an Elastic IP.

## API endpoints (selected)

- `GET /api/sandwiches`
- `GET /api/options/meats`
- `POST /api/builder`

For the full surface, see the `Controllers/` folder.

## Contributing

1. Fork the repo
2. Create a branch (`git checkout -b feature/your-feature`)
3. Make changes and run local builds/tests
4. Open a Pull Request

---

If you'd like a different tone (more marketing-friendly README, or a developer guide split into `docs/`), tell me which style and I'll create a branch with the new README and any supporting docs.
```
  - Colorized output and progress indicators
  - Prerequisite checking (Node.js, .NET SDK)
  - Automatic dependency installation
  - Robust service stopping and cleanup
  - Health checks and detailed status reporting
  - Better error handling and troubleshooting hints

- **`dev-stop-all.sh`** - Clean shutdown script that:
  - Gracefully stops services by port and PID
  - Cleans up all related processes
  - Provides clear status feedback
  - Handles both graceful and force-kill scenarios

- **`dev-status.sh`** - Comprehensive status checker that shows:
  - Service running status and health
  - Environment information
  - PID file status
  - Log file locations and sizes
  - Quick command reference

## 🧰 Developer tasks (local)

This section documents local developer tooling and recommended workflows for working on the project.

### dev-clean-build.sh (new)

Purpose: stop running dev services, clear common caches and build artifacts, reinstall frontend dependencies, and perform fresh builds for backend and frontend.

Usage:

```bash
# Normal clean + build (stops services)
./dev-clean-build.sh

# Full clean (removes FrontOfTheHouse/node_modules and reinstalls)
./dev-clean-build.sh --full
```

Notes:
- The script will call `./dev-stop-all.sh` to ensure services are not running while cleaning.
- `--full` removes `node_modules` and requires network access to reinstall packages; expect it to be slower.
- The script runs `dotnet nuget locals all --clear` and `dotnet build`; on repositories with multiple `.csproj` files it will prefer the project file used by the start script, but if you see a message like:

   "MSBUILD : error MSB1011: Specify which project or solution file to use because this folder contains more than one project or solution file."

   then re-run the build against an explicit project file, for example:

```bash
dotnet build BackOfTheHouse.csproj
```

### Recommended developer workflow

1. When switching branches or pulling big changes, use the clean job to ensure your local caches and artifacts are fresh:

```bash
./dev-clean-build.sh
```

2. Start services and view the app:

```bash
./dev-start-all.sh
# Frontend: http://localhost:4200
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
