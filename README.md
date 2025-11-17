# TheSandwich

A full-stack sandwich ordering application demonstrating modern web development practices.

## 🥪 Overview

This repository contains two main parts:

- **`BackOfTheHouse`** — ASP.NET Core backend (.NET 10) with Entity Framework
- **`FrontOfTheHouse`** — Angular frontend with server-side rendering support

## 🚀 Features

- **Dual Database Support**: SQL Server (production) with SQLite fallback (development)
- **Sandwich Builder**: Interactive sandwich creation with ingredient selection
- **RESTful API**: Clean API endpoints for sandwiches and options
- **Modern Frontend**: Angular with TypeScript and SSR capabilities
- **Development Tools**: Automated scripts for running both services

## 📋 Prerequisites

- **.NET 10 SDK** (preview) - [Download here](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Node.js** (v18+) and **npm** - [Download here](https://nodejs.org/)
- **Docker** (optional) - For SQL Server database

## ⚡ Quick Start

### 🔥 One-Command Setup (Recommended)

```bash
# Start both backend and frontend services
./dev-start-all.sh
```

This will:

**Access the application:**

### Quick start (no Docker DB)

The project defaults to a lightweight SQLite fallback so anyone can clone and run locally without Docker or SQL Server.

1. Install prerequisites: .NET SDK and Node.js/npm
2. From the repo root run:

```bash
./dev-start-all.sh
```

This will start the backend (Kestrel) and frontend (Angular dev server). By default the backend will not attempt to use a Docker SQL server and will use a local SQLite DB (seeded with sample data).

Tip: run the lightweight setup helper once after cloning to install frontend dependencies and ensure the `Data/` folder exists:

```bash
./scripts/setup-local.sh
```

### Run with Docker SQL Server (opt-in)

If you want to use the Docker SQL Server for development, opt in explicitly. Create a `.env` file containing a `DOCKER_DB_CONNECTION` (or set the env var), then run the script with the `USE_DOCKER_DB` environment variable or `--use-docker-db` flag:

```bash
# example .env
# DOCKER_DB_CONNECTION="Server=127.0.0.1,1433;Database=sandwich_app;User Id=sa;Password=MyStrongPass123;TrustServerCertificate=True;"

# start using SQLite (default)
./dev-start-all.sh

# start and prefer Docker DB (requires a running SQL container or .env)
USE_DOCKER_DB=1 ./dev-start-all.sh
# or
./dev-start-all.sh --use-docker-db
```

This makes it explicit and easy for newcomers to run the project locally without needing Docker.

Files & helpers:
- `.env.example` — example env file showing `DOCKER_DB_CONNECTION` and `USE_DOCKER_DB` opt-in usage
- `scripts/setup-local.sh` — installs frontend dependencies and ensures `Data/` exists for SQLite
 - `docker-compose.dev.yml` — optional compose file to bring up Azure SQL Edge for demos (`docker compose -f docker-compose.dev.yml up -d`)
### 🛠️ Manual Setup

#### Backend Setup

1. **With SQLite (Simple - No Docker required)**
   ```bash
   dotnet run --project BackOfTheHouse.csproj
   ```
   - Uses local SQLite database (`Data/sandwich.db`)
   - Automatically seeds sample data
   - All features available

2. **With SQL Server (Advanced)**
   ```bash
   export DOCKER_DB_CONNECTION='Server=127.0.0.1,1433;Database=sandwich_app;User Id=sa;Password=MyStrongPass123;TrustServerCertificate=True;'
   dotnet run --project BackOfTheHouse.csproj
   ```

#### Frontend Setup

```bash
cd FrontOfTheHouse
npm install
npm start  # Uses proxy configuration automatically
```

## 🏗️ Architecture

### Database Strategy
- **Production**: SQL Server with full relational schema
- **Development**: SQLite with simplified unified schema
- **Automatic Fallback**: Seamlessly switches based on configuration

### Project Structure
```
├── Controllers/              # API controllers
├── Data/                     # Entity Framework contexts and models
│   ├── Scaffolded/          # SQL Server entities (scaffolded)
│   └── Migrations/          # Database migrations
├── Extensions/              # Service configuration extensions
├── Services/                # Business logic and repository interfaces
├── FrontOfTheHouse/         # Angular frontend (submodule)
└── Properties/              # Launch settings
```

## 🛠️ Development Tools

### Available Scripts

```bash
# 🚀 Start both services (recommended)
./dev-start-all.sh

# 🛑 Stop all development services
./dev-stop-all.sh

# 📊 Check service status and health
./dev-status.sh

# Start with specific SQL Server connection
./dev-start-all.sh "Server=localhost,1433;Database=sandwich_app;..."

# Start only frontend (if backend is already running)
./FrontOfTheHouse/dev-serve.sh
```

### Script Features

- **`dev-start-all.sh`** - Enhanced startup script with:
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
