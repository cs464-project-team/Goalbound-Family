# Quick Command Reference - Goalbound Family

Quick reference for the most commonly used commands in the project.

## EF Core Migrations

```bash
# Create a new migration
dotnet ef migrations add MigrationName

# Apply all pending migrations to database
dotnet ef database update

# Rollback to a specific migration
dotnet ef database update PreviousMigrationName

# Remove the last migration (if not applied)
dotnet ef migrations remove

# List all migrations
dotnet ef migrations list

# Generate SQL script
dotnet ef migrations script

# Generate SQL script to file
dotnet ef migrations script -o migration.sql

# Drop database (DELETES ALL DATA!)
dotnet ef database drop

# Drop and recreate
dotnet ef database drop && dotnet ef database update
```

## .NET CLI

```bash
# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run application
dotnet run

# Run with auto-reload
dotnet watch run

# Clean build artifacts
dotnet clean

# Install a package
dotnet add package PackageName

# Remove a package
dotnet remove package PackageName

# List installed packages
dotnet list package
```

## User Secrets (Development)

```bash
# Initialize user secrets
dotnet user-secrets init

# Set a secret
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"

# List all secrets
dotnet user-secrets list

# Remove a secret
dotnet user-secrets remove "ConnectionStrings:DefaultConnection"

# Clear all secrets
dotnet user-secrets clear
```

## Git

```bash
# Check status
git status

# Stage all changes
git add .

# Commit changes
git commit -m "Your message"

# Push to remote
git push

# Pull from remote
git pull

# Create new branch
git checkout -b feature/branch-name

# Switch branches
git checkout branch-name

# Merge branch
git merge branch-name

# View commit history
git log --oneline
```

## Frontend (npm)

```bash
# Install dependencies
npm install

# Start dev server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Run linter
npm run lint

# Install a package
npm install package-name

# Install dev dependency
npm install -D package-name

# Uninstall a package
npm uninstall package-name

# Update all packages
npm update
```

## Supabase Connection String Format

```
Host=db.xxxxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your-password;SSL Mode=Require;Trust Server Certificate=true
```

## Common Workflows

### Create a New Entity

```bash
# 1. Create Model, DTOs, Repository, Service, Controller
# 2. Add DbSet to ApplicationDbContext
# 3. Register in Program.cs
# 4. Create migration
dotnet ef migrations add AddYourEntityName

# 5. Review migration
cat Migrations/*_AddYourEntityName.cs

# 6. Apply to database
dotnet ef database update

# 7. Test the API
dotnet run
```

### Undo a Migration (Not Applied)

```bash
dotnet ef migrations remove
```

### Undo a Migration (Already Applied)

```bash
# Rollback database to previous migration
dotnet ef database update PreviousMigrationName

# Remove the migration file
dotnet ef migrations remove
```

### Fresh Database (Development Only!)

```bash
dotnet ef database drop --force
dotnet ef database update
```

### Deploy to Production

```bash
# Generate SQL script
dotnet ef migrations script --idempotent -o production.sql

# Review the SQL
cat production.sql

# Apply in Supabase SQL Editor or via CLI
```

## Troubleshooting

```bash
# Check EF Core version
dotnet ef --version

# Reinstall EF Core CLI tools
dotnet tool uninstall --global dotnet-ef
dotnet tool install --global dotnet-ef

# Verify connection to Supabase
dotnet ef database drop  # Will fail if can't connect

# Check current user secrets
dotnet user-secrets list

# Rebuild project
dotnet clean && dotnet restore && dotnet build

# Clear NuGet cache
dotnet nuget locals all --clear
```

## Testing API Endpoints

### Using curl

```bash
# GET all users
curl https://localhost:7xxx/api/users

# GET user by ID
curl https://localhost:7xxx/api/users/{id}

# POST create user
curl -X POST https://localhost:7xxx/api/users \
  -H "Content-Type: application/json" \
  -d '{"firstName":"John","lastName":"Doe","email":"john@example.com"}'

# PUT update user
curl -X PUT https://localhost:7xxx/api/users/{id} \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Jane"}'

# DELETE user
curl -X DELETE https://localhost:7xxx/api/users/{id}
```

### Using HTTPie (if installed)

```bash
# GET all users
http https://localhost:7xxx/api/users

# POST create user
http POST https://localhost:7xxx/api/users \
  firstName=John lastName=Doe email=john@example.com

# PUT update user
http PUT https://localhost:7xxx/api/users/{id} firstName=Jane

# DELETE user
http DELETE https://localhost:7xxx/api/users/{id}
```

## Useful SQL Queries (Supabase SQL Editor)

```sql
-- List all tables
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public';

-- View migration history
SELECT * FROM "__EFMigrationsHistory"
ORDER BY "MigrationId";

-- Count records in Users table
SELECT COUNT(*) FROM "Users";

-- View all users
SELECT * FROM "Users";

-- Check table structure
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'Users';

-- View indexes
SELECT indexname, indexdef
FROM pg_indexes
WHERE tablename = 'Users';
```

## Port Reference

| Service | Port | URL |
|---------|------|-----|
| Frontend (Vite) | 5173 | http://localhost:5173 |
| Backend (HTTPS) | 7xxx | https://localhost:7xxx |
| Backend (HTTP) | 5xxx | http://localhost:5xxx |
| Supabase PostgreSQL | 5432 | db.xxxxx.supabase.co:5432 |

## Environment Variables

```bash
# .env file format (if using)
ConnectionStrings__DefaultConnection=your-connection-string
ASPNETCORE_ENVIRONMENT=Development
```

## Quick Links

- [EF Core CLI Reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
- [Supabase Dashboard](https://supabase.com/dashboard)
- [.NET SDK Download](https://dotnet.microsoft.com/download)
- [Node.js Download](https://nodejs.org/)
