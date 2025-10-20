# Contributing to TheSandwich

Welcome! This guide will help you contribute to TheSandwich effectively. Whether you're fixing bugs, adding features, or improving documentation, we appreciate your help! 🥪

## 🚀 Quick Start for Contributors

The fastest way to get started is using our automation script:

```bash
# Clone the repository
git clone https://github.com/shorepound/TheSandwich.git
cd TheSandwich

# Start development environment (one command!)
./dev-start-all.sh
```

Visit http://localhost:4200 to see the application running.

## 📋 Prerequisites
### Required
- **.NET 10 SDK** (preview) - [Download here](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Node.js** (v18+) and **npm** - [Download here](https://nodejs.org/)

### Optional
- **Docker** - For SQL Server database (SQLite fallback available)
- **Git** - For version control (if not already installed)

## 🏗️ Development Environment Setup

### Option 1: Automated Setup (Recommended)

```bash
# Start both services with automatic configuration
./dev-start-all.sh

# Or with specific SQL Server connection
./dev-start-all.sh "Server=localhost,1433;Database=sandwich_app;User=sa;Password=YourPassword;TrustServerCertificate=True;"
```

### Option 2: Manual Setup

#### Backend (.NET API)

**SQLite (Simple - No Database Setup Required):**
```bash
# Automatically uses SQLite fallback
dotnet run --project BackOfTheHouse.csproj
```

**SQL Server (Advanced):**
```bash
# Set environment variable for SQL Server
export DOCKER_DB_CONNECTION="Server=127.0.0.1,1433;Database=sandwich_app;User Id=sa;Password=MyStrongPass123;TrustServerCertificate=True;"
dotnet run --project BackOfTheHouse.csproj
```

#### Frontend (Angular)

```bash
cd FrontOfTheHouse
npm install
npm start  # Automatically uses proxy configuration
```

## 🛠️ Development Workflows

### Making Changes

1. **Create a feature branch:**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes** using your preferred editor

3. **Test your changes:**
   ```bash
   # Backend tests
   dotnet test
   
   # Frontend tests (if available)
   cd FrontOfTheHouse
   npm test
   ```

4. **Verify the application works:**
   ```bash
   ./dev-start-all.sh
   # Visit http://localhost:4200 to test
   ```

### Database Development

**Creating Migrations:**
```bash
# Add a new migration
dotnet ef migrations add YourMigrationName --project BackOfTheHouse.csproj --context SandwichContext

# For SQL Server context (if using Docker database)
dotnet ef migrations add YourMigrationName --project BackOfTheHouse.csproj --context DockerSandwichContext
```

**Applying Migrations:**
```bash
# Apply to SQLite
dotnet ef database update --project BackOfTheHouse.csproj --context SandwichContext

# Apply to SQL Server
dotnet ef database update --project BackOfTheHouse.csproj --context DockerSandwichContext
```

**Reset Local Database:**
```bash
# For SQLite (simple)
rm Data/sandwich.db*
dotnet run --project BackOfTheHouse.csproj  # Auto-recreates with seed data
```

### Frontend Development

**Key Files to Know:**
- `FrontOfTheHouse/src/app/` - Angular application
- `FrontOfTheHouse/proxy.conf.json` - API proxy configuration
- `FrontOfTheHouse/src/app/services/` - Services for API communication

**Common Commands:**
```bash
cd FrontOfTheHouse

# Development server with hot reload
npm start

# Build for production
npm run build

# Run tests (if available)
npm test
```

## 🔧 Git Workflow & Submodules

### Working with Frontend Submodule

The `FrontOfTheHouse` directory is a Git submodule. When making frontend changes:

```bash
# 1. Make changes in the submodule
cd FrontOfTheHouse
# ... make your changes ...

# 2. Commit in the submodule
git add .
git commit -m "feat: add new feature"
git push origin main

# 3. Update the main repository to reference the new submodule commit
cd ..  # Back to main repository
git add FrontOfTheHouse
git commit -m "chore: update FrontOfTheHouse submodule"
git push origin main
```

### Standard Git Workflow

```bash
# 1. Create feature branch
git checkout -b feature/amazing-feature

# 2. Make commits with conventional commit messages
git commit -m "feat: add sandwich recommendation engine"
git commit -m "fix: resolve bread selection bug"
git commit -m "docs: update API documentation"

# 3. Push and create PR
git push origin feature/amazing-feature
# Then create Pull Request on GitHub
```

## 🧪 Testing Guidelines

### Backend Testing
```bash
# Run all tests
dotnet test

# Run tests with coverage (if configured)
dotnet test --collect:"XPlat Code Coverage"

# Test specific project
dotnet test BackOfTheHouse.Tests
```

### Manual Testing Checklist
- [ ] Application starts with `./dev-start-all.sh`
- [ ] Frontend loads at http://localhost:4200
- [ ] API endpoints respond at http://localhost:5251
- [ ] Sandwich builder creates sandwiches successfully
- [ ] Sandwich list displays correctly
- [ ] Both SQLite and SQL Server modes work (if applicable)

## 🐛 Troubleshooting

### Common Issues & Solutions

**Port Conflicts:**
```bash
# Find and kill processes using development ports
lsof -ti:4200 -ti:5251 | xargs kill

# Or kill specific port
lsof -ti:4200 | xargs kill  # Frontend
lsof -ti:5251 | xargs kill  # Backend
```

**Database Issues:**
```bash
# Reset SQLite database
rm Data/sandwich.db*

# Database migration conflicts
dotnet ef database drop --project BackOfTheHouse.csproj --context SandwichContext
dotnet ef database update --project BackOfTheHouse.csproj --context SandwichContext
```

**Frontend Build Errors:**
```bash
cd FrontOfTheHouse
rm -rf node_modules package-lock.json
npm install
npm start
```

**API Connection Issues:**
- Ensure backend is running on `http://127.0.0.1:5251`
- Check `FrontOfTheHouse/proxy.conf.json` configuration
- Verify CORS settings in backend if calling API directly

**Build Artifacts in Git:**
```bash
# Remove accidentally committed build files
git rm -r --cached bin obj node_modules dist
echo "bin/\nobj/\nnode_modules/\ndist/" >> .gitignore
git add .gitignore
git commit -m "chore: remove build artifacts and update .gitignore"
```

## 📝 Code Style & Standards

### Backend (.NET)
- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep controllers thin - move business logic to services

### Frontend (Angular/TypeScript)
- Follow [Angular Style Guide](https://angular.io/guide/styleguide)
- Use TypeScript strict mode
- Prefer reactive programming with RxJS
- Component names should be descriptive and use kebab-case

### Commit Messages
Use [Conventional Commits](https://www.conventionalcommits.org/):
- `feat:` - New features
- `fix:` - Bug fixes
- `docs:` - Documentation changes
- `refactor:` - Code refactoring
- `test:` - Adding/updating tests
- `chore:` - Maintenance tasks

## 🚀 Pull Request Process

1. **Ensure your branch is up to date:**
   ```bash
   git checkout main
   git pull origin main
   git checkout your-feature-branch
   git rebase main
   ```

2. **Create a clear PR description:**
   - What does this change do?
   - Why is this change needed?
   - How can reviewers test it?
   - Any breaking changes?

3. **Checklist before submitting:**
   - [ ] Code builds successfully
   - [ ] Tests pass (if any)
   - [ ] Application runs with `./dev-start-all.sh`
   - [ ] Commit messages follow conventional format
   - [ ] Documentation updated (if needed)

## 🆘 Getting Help

- **Issues:** Create a GitHub issue for bugs or feature requests
- **Questions:** Use GitHub Discussions for general questions
- **Code Review:** PRs will be reviewed by maintainers

## 🎯 Areas for Contribution

We welcome contributions in these areas:

### Backend
- Repository pattern implementation
- Unit tests and integration tests
- Performance optimizations
- Additional API endpoints
- Better error handling

### Frontend
- UI/UX improvements
- Additional features (ratings, reviews, etc.)
- Better responsive design
- Frontend testing
- Accessibility improvements

### DevOps/Tooling
- Docker containerization
- CI/CD pipeline improvements
- Deployment automation
- Monitoring and logging

### Documentation
- API documentation
- Code comments
- Tutorial content
- Architecture diagrams

## 🙏 Thank You!
