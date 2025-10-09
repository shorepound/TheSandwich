# Contributing

Short developer commands for running and developing TheSandwich locally.

Prerequisites
- .NET SDK (matching project target, e.g. .NET 10)
- Node.js and npm (for the Angular frontend)
- Docker (optional, for running the SQL server container)

Common workflows

1) Run backend (with Docker SQL)

Set the connection string (example using dockerized SQL Server):

```bash
export DOCKER_DB_CONNECTION="Server=127.0.0.1,1433;Database=sandwich_app;User Id=sa;Password=MyStrongPass123;TrustServerCertificate=True;"
dotnet run --project BackOfTheHouse
```

2) Run backend (SQLite fallback)

If you don't have the Docker DB, the app will fall back to a local SQLite file. Just:

```bash
dotnet run --project BackOfTheHouse
```

3) Run frontend (Angular dev server with proxy)

```bash
cd FrontOfTheHouse
npm install
ng serve --proxy-config proxy.conf.json
```

4) Common db/migrations (EF Core)

Scaffold, add, or update migrations from the project root:

```bash
dotnet ef migrations add <Name> --project BackOfTheHouse --startup-project BackOfTheHouse
dotnet ef database update --project BackOfTheHouse --startup-project BackOfTheHouse
```

5) Git & submodules

If you update the frontend submodule, commit inside the submodule first, push it, then update the top-level repo:

```bash
# inside FrontOfTheHouse (submodule)
git add .
git commit -m "UI: ..."
git push

# back in top-level repo
git add FrontOfTheHouse
git commit -m "Update FrontOfTheHouse submodule"
git push
```

6) Troubleshooting

- If port 4200 is in use, find and kill the process: `lsof -i :4200` then `kill <pid>`.
- If the frontend proxy shows ECONNREFUSED for `/api`, ensure the backend is running on `127.0.0.1:5251` (or adjust `proxy.conf.json`).
- If you accidentally commit build artifacts, remove them from git index and add to `.gitignore`:

```bash
git rm -r --cached bin obj
git commit -m "chore: remove build artifacts from repo"
git push
```

7) Tests

Run any unit tests (if present) from the project root:

```bash
dotnet test
```

Thanks for contributing — open a PR for feature work and reference any relevant issue numbers.
