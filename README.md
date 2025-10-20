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
- Stop any existing services on ports 4200 and 5251
- Start the backend with SQLite fallback (if no SQL Server configured)
- Start the frontend with proxy configuration
- Display startup logs

**Access the application:**
- Frontend: http://localhost:4200
- Backend API: http://localhost:5251
- Swagger UI: http://localhost:5251/swagger (development only)

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
# Start both services (recommended)
./dev-start-all.sh

# Start with specific SQL Server connection
./dev-start-all.sh "Server=localhost,1433;Database=sandwich_app;..."

# Start only frontend (if backend is already running)
./FrontOfTheHouse/dev-serve.sh
```

### Environment Configuration

Create a `.env` file in the root directory:
```env
# Optional: SQL Server connection (fallback to SQLite if not set)
DOCKER_DB_CONNECTION="Server=127.0.0.1,1433;Database=sandwich_app;User Id=sa;Password=MyStrongPass123;TrustServerCertificate=True;"
```

### Useful Commands

```bash
# Build the application
dotnet build BackOfTheHouse.csproj

# Run database migrations (if using SQL Server)
dotnet ef database update --context DockerSandwichContext

# Check running services
lsof -i :4200 -i :5251

# View logs
tail -f backof.log
tail -f FrontOfTheHouse/front-dev.log
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
   # Kill processes on development ports
   lsof -ti:4200 -ti:5251 | xargs kill
   ```

2. **Database Schema Mismatch**
   ```bash
   # Reset SQLite database
   rm Data/sandwich.db*
   dotnet run --project BackOfTheHouse.csproj
   ```

3. **Frontend Build Errors**
   ```bash
   cd FrontOfTheHouse
   rm -rf node_modules package-lock.json
   npm install
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

This project is for demonstration purposes.
