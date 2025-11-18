# Quick Setup Guide - GoalboundFamily API

## Initial Setup Steps

### 1. Configure Supabase Connection

Get your connection details from Supabase:
- Project Dashboard → Settings → Database
- Connection Info section

Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.yourproject.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your-password;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

### 2. Install EF Core CLI Tools

```bash
dotnet tool install --global dotnet-ef
```

### 3. Create Initial Migration

```bash
# Navigate to backend directory
cd backend

# Create migration
dotnet ef migrations add InitialCreate

# Apply to database
dotnet ef database update
```

### 4. Run the Application

```bash
dotnet run
# or with auto-reload
dotnet watch run
```

## Testing the API

### Using the Example User Endpoints

Once the app is running, you can test the User endpoints:

**Create a User** (POST)
```bash
curl -X POST https://localhost:7xxx/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com"
  }'
```

**Get All Users** (GET)
```bash
curl https://localhost:7xxx/api/users
```

**Get User by ID** (GET)
```bash
curl https://localhost:7xxx/api/users/{id}
```

**Update User** (PUT)
```bash
curl -X PUT https://localhost:7xxx/api/users/{id} \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Jane"
  }'
```

**Delete User** (DELETE)
```bash
curl -X DELETE https://localhost:7xxx/api/users/{id}
```

## Common EF Core Commands

```bash
# Add a new migration
dotnet ef migrations add MigrationName

# Update database to latest migration
dotnet ef database update

# Rollback to specific migration
dotnet ef database update PreviousMigrationName

# Remove last migration (not applied to DB)
dotnet ef migrations remove

# List all migrations
dotnet ef migrations list

# Generate SQL script from migrations
dotnet ef migrations script
```

## Troubleshooting

### Connection Issues

If you can't connect to Supabase:
1. Check your connection string format
2. Ensure SSL Mode is set to "Require"
3. Verify your Supabase password is correct
4. Check if your IP is allowed in Supabase dashboard

### Migration Issues

If migrations fail:
```bash
# Drop and recreate database (development only!)
dotnet ef database drop
dotnet ef database update
```

### Build Errors

If you get build errors:
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

## Next Steps

1. Configure your Supabase connection string
2. Run migrations to create the database schema
3. Test the User endpoints to verify everything works
4. Start building your own entities following the pattern in the README

## Using User Secrets (Recommended)

For better security, use .NET User Secrets instead of storing credentials in `appsettings.json`:

```bash
# Initialize user secrets
dotnet user-secrets init

# Set connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string-here"

# List all secrets
dotnet user-secrets list

# Remove a secret
dotnet user-secrets remove "ConnectionStrings:DefaultConnection"
```

User secrets are stored outside your project directory and won't be committed to git.
