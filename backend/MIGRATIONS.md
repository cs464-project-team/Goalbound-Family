# Database Migrations Guide - Supabase + Entity Framework Core

This guide covers everything you need to know about working with database migrations in the GoalboundFamily project using EF Core and Supabase PostgreSQL.

## Table of Contents
1. [Initial Setup](#initial-setup)
2. [Creating Migrations](#creating-migrations)
3. [Applying Migrations](#applying-migrations)
4. [Common Workflows](#common-workflows)
5. [Supabase-Specific Considerations](#supabase-specific-considerations)
6. [Troubleshooting](#troubleshooting)

## Initial Setup

### 1. Install EF Core CLI Tools

```bash
dotnet tool install --global dotnet-ef
```

Verify installation:
```bash
dotnet ef --version
```

### 2. Configure Supabase Connection

Get your connection details from Supabase:
- **Dashboard** → **Project Settings** → **Database**
- Find your connection string or individual parameters

**Option A: Using appsettings.json (Not recommended for production)**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.xxxxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your-password;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

**Option B: Using User Secrets (Recommended for development)**
```bash
cd backend
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=db.xxxxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your-password;SSL Mode=Require;Trust Server Certificate=true"
```

**Option C: Using Environment Variables (Recommended for production)**
```bash
export ConnectionStrings__DefaultConnection="your-connection-string"
```

### 3. Create Initial Migration

```bash
cd backend
dotnet ef migrations add InitialCreate
```

This generates:
- `Migrations/XXXXXX_InitialCreate.cs` - Migration code
- `Migrations/ApplicationDbContextModelSnapshot.cs` - Current model state

### 4. Apply Migration to Supabase

```bash
dotnet ef database update
```

This executes the migration against your Supabase PostgreSQL database.

## Creating Migrations

### When to Create a Migration

Create a migration whenever you:
- Add a new entity (model class)
- Modify an existing entity (add/remove/change properties)
- Change relationships between entities
- Add indexes, constraints, or configurations

### Migration Naming Conventions

Use descriptive names that explain what the migration does:

```bash
# Good examples
dotnet ef migrations add AddUserEntity
dotnet ef migrations add AddEmailIndexToUsers
dotnet ef migrations add CreateFamilyGoalRelationship
dotnet ef migrations add AddIsActiveColumnToUsers

# Avoid vague names
dotnet ef migrations add Update1
dotnet ef migrations add Changes
```

### Example: Adding a New Entity

1. **Create the Model**
```csharp
// Models/Family.cs
public class Family
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

2. **Add DbSet to ApplicationDbContext**
```csharp
// Data/ApplicationDbContext.cs
public DbSet<Family> Families { get; set; }
```

3. **Configure in OnModelCreating (optional)**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Family>(entity =>
    {
        entity.HasIndex(f => f.Name);
        entity.Property(f => f.CreatedAt)
            .HasDefaultValueSql("NOW()");
    });
}
```

4. **Create Migration**
```bash
dotnet ef migrations add AddFamilyEntity
```

5. **Review Generated Migration**
```csharp
// Check Migrations/XXXXXX_AddFamilyEntity.cs
public partial class AddFamilyEntity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Families",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Families", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Families_Name",
            table: "Families",
            column: "Name");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Families");
    }
}
```

6. **Apply Migration**
```bash
dotnet ef database update
```

## Applying Migrations

### Apply All Pending Migrations

```bash
dotnet ef database update
```

### Apply to Specific Migration

```bash
# Apply up to a specific migration
dotnet ef database update MigrationName

# Rollback to a specific migration
dotnet ef database update AddUserEntity
```

### Rollback All Migrations

```bash
dotnet ef database update 0
```

**Warning**: This will drop all tables!

### Generate SQL Script

Instead of applying directly, generate SQL for review:

```bash
# Generate SQL for all migrations
dotnet ef migrations script

# Generate SQL from one migration to another
dotnet ef migrations script AddUserEntity AddFamilyEntity

# Generate SQL from specific migration to latest
dotnet ef migrations script AddUserEntity

# Output to file
dotnet ef migrations script -o migration.sql
```

## Common Workflows

### Workflow 1: Adding a Column

1. **Update Model**
```csharp
public class User
{
    // ... existing properties
    public string? PhoneNumber { get; set; }  // New property
}
```

2. **Create and Apply Migration**
```bash
dotnet ef migrations add AddPhoneNumberToUser
dotnet ef database update
```

### Workflow 2: Removing a Column

1. **Remove from Model**
```csharp
public class User
{
    // ... removed PhoneNumber property
}
```

2. **Create and Apply Migration**
```bash
dotnet ef migrations add RemovePhoneNumberFromUser
dotnet ef database update
```

**Note**: Data in that column will be lost!

### Workflow 3: Renaming a Column

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.RenameColumn(
        name: "Name",
        table: "Users",
        newName: "FullName");
}
```

### Workflow 4: Adding a Relationship

1. **Update Models**
```csharp
public class User
{
    public Guid? FamilyId { get; set; }  // Foreign key
    public Family? Family { get; set; }   // Navigation property
}

public class Family
{
    public ICollection<User> Members { get; set; } = new List<User>();
}
```

2. **Configure Relationship**
```csharp
modelBuilder.Entity<User>()
    .HasOne(u => u.Family)
    .WithMany(f => f.Members)
    .HasForeignKey(u => u.FamilyId)
    .OnDelete(DeleteBehavior.SetNull);
```

3. **Create and Apply**
```bash
dotnet ef migrations add AddUserFamilyRelationship
dotnet ef database update
```

### Workflow 5: Undoing a Migration

**If not yet applied to database:**
```bash
dotnet ef migrations remove
```

**If already applied to database:**
```bash
# Rollback to previous migration
dotnet ef database update PreviousMigrationName

# Then remove the migration
dotnet ef migrations remove
```

## Supabase-Specific Considerations

### SSL Connection Required

Always include `SSL Mode=Require` in your connection string:
```
Host=db.xxxxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=xxx;SSL Mode=Require;Trust Server Certificate=true
```

### Connection Pooling

Supabase has connection limits. The connection string is configured with retry logic:
```csharp
options.UseNpgsql(
    connectionString,
    npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(10),
        errorCodesToAdd: null
    )
)
```

### Viewing Migrations in Supabase

After applying migrations:

1. **Table Editor**:
   - Supabase Dashboard → Table Editor
   - View all tables including `__EFMigrationsHistory`

2. **SQL Editor**:
   - Run queries to verify schema
   ```sql
   SELECT * FROM "__EFMigrationsHistory";
   SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';
   ```

### Supabase Row Level Security (RLS)

If you enable RLS in Supabase, remember:
- EF Core connects with the `postgres` user (bypasses RLS)
- RLS policies apply to client connections (e.g., from frontend using supabase-js)
- Configure RLS policies in Supabase Dashboard if needed

### Time Zones

PostgreSQL on Supabase uses UTC by default:
- Use `DateTime.UtcNow` in your code
- EF Core maps `DateTime` to `timestamp with time zone`
- Use `HasDefaultValueSql("NOW()")` for timestamps

## Troubleshooting

### Error: "No migrations configuration type was found"

**Solution**: Make sure you're in the `backend` directory:
```bash
cd backend
dotnet ef migrations add MigrationName
```

### Error: "Unable to connect to database"

**Checks**:
1. Verify connection string is correct
2. Check Supabase project is not paused
3. Ensure `SSL Mode=Require` is in connection string
4. Verify password is correct

**Test connection**:
```bash
# Using user secrets
dotnet user-secrets list

# Test with psql (if installed)
psql "your-connection-string"
```

### Error: "A migration is pending"

```bash
# View pending migrations
dotnet ef migrations list

# Apply pending migrations
dotnet ef database update
```

### Error: "The migration has already been applied"

This usually means your local migration state is out of sync.

**Solution**:
```bash
# Check what's applied in database
dotnet ef migrations list

# Remove local migration that's already applied
dotnet ef migrations remove
```

### Resetting Database (Development Only!)

**WARNING**: This deletes all data!

```bash
# Drop all tables
dotnet ef database drop

# Recreate from migrations
dotnet ef database update
```

### Checking Migration History

```bash
# List all migrations and their status
dotnet ef migrations list

# View migration SQL without applying
dotnet ef migrations script

# Check specific migration
dotnet ef migrations script PreviousMigration TargetMigration
```

### Common PostgreSQL/Supabase Errors

**"column does not exist"**
- You may have old data. Check migration order.

**"relation already exists"**
- Table already exists. Check `__EFMigrationsHistory` table.

**"permission denied"**
- Using wrong credentials. Verify connection string.

**"SSL connection required"**
- Add `SSL Mode=Require` to connection string.

## Best Practices

1. **Always review migrations** before applying to production
2. **Test migrations** on a development database first
3. **Use descriptive migration names**
4. **Don't modify applied migrations** - create a new one instead
5. **Commit migrations to git** along with model changes
6. **Use User Secrets** for development credentials
7. **Backup production database** before applying migrations
8. **Generate SQL scripts** for production deployments
9. **Keep migrations small and focused** - one change per migration
10. **Document breaking changes** in migration comments

## Production Deployment

For production, generate SQL scripts and review before applying:

```bash
# Generate SQL for all unapplied migrations
dotnet ef migrations script --idempotent -o production-migration.sql

# Review the SQL file
cat production-migration.sql

# Apply manually in Supabase SQL Editor or via command line
```

The `--idempotent` flag makes the script safe to run multiple times.

## Useful Commands Reference

```bash
# List all migrations
dotnet ef migrations list

# Add migration
dotnet ef migrations add MigrationName

# Remove last migration
dotnet ef migrations remove

# Apply all migrations
dotnet ef database update

# Rollback to specific migration
dotnet ef database update MigrationName

# Rollback all
dotnet ef database update 0

# Generate SQL script
dotnet ef migrations script

# Drop database
dotnet ef database drop

# Get help
dotnet ef --help
dotnet ef migrations --help
dotnet ef database --help
```

## Additional Resources

- [EF Core Migrations Docs](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Supabase Database Documentation](https://supabase.com/docs/guides/database)
- [Npgsql EF Core Provider](https://www.npgsql.org/efcore/)
