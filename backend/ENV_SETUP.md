# Environment Variables Setup Guide

This guide explains how to configure environment variables for the Goalbound Family backend API.

## Table of Contents
1. [Quick Setup](#quick-setup)
2. [Environment Variables Reference](#environment-variables-reference)
3. [Configuration Methods](#configuration-methods)
4. [Security Best Practices](#security-best-practices)
5. [Troubleshooting](#troubleshooting)

## Quick Setup

### 1. Copy the Example File

From the **project root** directory:

```bash
cp .env.example .env
```

Or from the **backend** directory:

```bash
cp .env.example .env
```

### 2. Edit the .env File

Open `.env` in your editor and update the values:

```bash
# Replace with your actual Supabase connection details
DB_CONNECTION_STRING=Host=db.your-project-ref.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your-actual-password;SSL Mode=Require;Trust Server Certificate=true
```

### 3. Verify Setup

The `.env` file should be in:
- **Project root**: `/path/to/CS464-FSD/.env`
- Or **backend directory**: `/path/to/CS464-FSD/backend/.env`

Either location works! The app will load from the backend directory first, then fall back to the root.

### 4. Test the Connection

```bash
cd backend
dotnet run
```

If successful, you should see:
```
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand...
```

## Environment Variables Reference

### Required Variables

#### DB_CONNECTION_STRING
**Purpose**: PostgreSQL database connection string for Supabase

**Format**:
```
Host=db.xxxxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your-password;SSL Mode=Require;Trust Server Certificate=true
```

**Where to find it**:
1. Go to [Supabase Dashboard](https://supabase.com/dashboard)
2. Select your project
3. Go to **Settings** → **Database**
4. Find the **Connection Info** section or **Connection String** tab
5. Copy the connection string and replace `[YOUR-PASSWORD]` with your actual database password

**Parts breakdown**:
- `Host`: Your Supabase database host (e.g., `db.abcdefghij.supabase.co`)
- `Port`: Database port (always `5432` for PostgreSQL)
- `Database`: Database name (default: `postgres`)
- `Username`: Database user (default: `postgres`)
- `Password`: Your project's database password
- `SSL Mode=Require`: Required for Supabase connections
- `Trust Server Certificate=true`: Trusts the Supabase SSL certificate

### Optional Variables

#### ASPNETCORE_ENVIRONMENT
**Purpose**: Sets the application environment

**Values**: `Development`, `Staging`, `Production`

**Default**: `Development` (if not set)

**Example**:
```bash
ASPNETCORE_ENVIRONMENT=Development
```

#### FRONTEND_URL
**Purpose**: Frontend URL for CORS configuration (if you want to override the default)

**Default**: `http://localhost:5173`

**Example**:
```bash
FRONTEND_URL=http://localhost:3000
```

## Configuration Methods

The application supports three ways to configure the database connection, in order of priority:

### Priority Order

1. **Environment Variable** (`DB_CONNECTION_STRING` from `.env` file) ← **Highest Priority**
2. **appsettings.json** (`ConnectionStrings:DefaultConnection`)
3. **.NET User Secrets** (lowest priority, not recommended for this setup)

### Method 1: .env File (Recommended)

✅ **Advantages**:
- Easy to use and understand
- Standard practice across many frameworks
- Automatically excluded from git
- Easy to share template (`.env.example`)
- Works across different environments

**Setup**:
```bash
# Create .env file
cp .env.example .env

# Edit .env
nano .env  # or use your preferred editor
```

**File location**: Place in project root or `backend/` directory

### Method 2: appsettings.json

⚠️ **Use only for**:
- Non-sensitive configuration
- Default/fallback values

**Setup**: Edit `backend/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-connection-string-here"
  }
}
```

**Warning**: Never commit sensitive credentials in `appsettings.json`! Add it to `.gitignore` if it contains secrets.

### Method 3: .NET User Secrets

❌ **Not recommended** for this project, but available:

```bash
cd backend
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
```

**Why not recommended**:
- More complex than `.env` files
- .NET-specific (not transferable to frontend or other tools)
- Harder to share/document

## Security Best Practices

### ✅ DO

1. **Use .env for local development**
   ```bash
   cp .env.example .env
   # Edit .env with your credentials
   ```

2. **Never commit .env to git**
   - The `.env` file is already in `.gitignore`
   - Always verify before committing: `git status`

3. **Use different credentials per environment**
   - Development: `.env`
   - Staging: Server environment variables
   - Production: Secure secret management (Azure Key Vault, AWS Secrets Manager, etc.)

4. **Share .env.example, not .env**
   ```bash
   # Good - commit this
   git add .env.example

   # Bad - never do this!
   git add .env  # This should be blocked by .gitignore
   ```

5. **Rotate credentials if exposed**
   - If you accidentally commit credentials, immediately:
     1. Revoke them in Supabase Dashboard
     2. Generate new credentials
     3. Update your `.env` file
     4. Remove from git history using `git filter-branch` or BFG Repo-Cleaner

### ❌ DON'T

1. ❌ Don't commit `.env` files to git
2. ❌ Don't share `.env` files via Slack/Email
3. ❌ Don't use production credentials in development
4. ❌ Don't hardcode secrets in code
5. ❌ Don't store `.env` in cloud storage without encryption

## .env File Format

### Basic Format

```bash
# Comments start with #
VARIABLE_NAME=value

# No spaces around =
DB_CONNECTION_STRING=Host=localhost;...

# Quotes are optional for simple values
ASPNETCORE_ENVIRONMENT=Development

# Use quotes for values with spaces or special characters
SOME_VALUE="value with spaces"
```

### Example .env File

```bash
# ============================================
# Goalbound Family - Environment Variables
# ============================================

# Database Connection
DB_CONNECTION_STRING=Host=db.abcdefgh.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=my-secret-password;SSL Mode=Require;Trust Server Certificate=true

# Application Settings
ASPNETCORE_ENVIRONMENT=Development

# CORS (optional)
# FRONTEND_URL=http://localhost:5173
```

## Troubleshooting

### Issue: "Connection string is null"

**Cause**: `.env` file not loaded or variable not set

**Solutions**:

1. **Verify .env file exists**:
   ```bash
   ls -la .env        # In project root
   ls -la backend/.env # Or in backend directory
   ```

2. **Check file contents**:
   ```bash
   cat .env | grep DB_CONNECTION_STRING
   ```

3. **Verify no typos**:
   - Variable name must be exactly: `DB_CONNECTION_STRING`
   - No spaces around `=`

4. **Check file location**:
   - Should be in backend directory: `backend/.env`
   - Or in project root: `.env`

### Issue: "Cannot connect to database"

**Cause**: Invalid connection string or network issue

**Solutions**:

1. **Verify connection string format**:
   ```bash
   # Must include all parts:
   Host=...;Port=5432;Database=postgres;Username=postgres;Password=...;SSL Mode=Require;Trust Server Certificate=true
   ```

2. **Check Supabase project status**:
   - Visit Supabase Dashboard
   - Ensure project is not paused

3. **Verify password**:
   - No special characters causing issues
   - No extra spaces
   - Correct password from Supabase

4. **Test connection with psql** (if installed):
   ```bash
   psql "postgresql://postgres:your-password@db.xxxxx.supabase.co:5432/postgres?sslmode=require"
   ```

### Issue: ".env file not being loaded"

**Cause**: File location or name issue

**Solutions**:

1. **Check filename**:
   ```bash
   # Must be exactly .env (not .env.txt or env)
   ls -la | grep env
   ```

2. **Verify DotNetEnv package is installed**:
   ```bash
   dotnet list package | grep DotNetEnv
   ```

3. **Check Program.cs has Env.Load()**:
   ```csharp
   // Should be at the top of Program.cs
   Env.Load();
   ```

### Issue: "Still seeing secrets in git"

**Cause**: `.env` not in `.gitignore`

**Solutions**:

1. **Verify .gitignore contains**:
   ```bash
   cat .gitignore | grep .env
   ```

   Should show:
   ```
   .env
   .env.local
   .env.*.local
   ```

2. **Check git status**:
   ```bash
   git status
   ```

   `.env` should NOT appear in the list

3. **If .env is tracked, remove it**:
   ```bash
   git rm --cached .env
   git commit -m "Remove .env from tracking"
   ```

## Production Deployment

For production, **DO NOT** use `.env` files. Instead:

### Azure

Use **Azure App Configuration** or **Azure Key Vault**:

```bash
# Set environment variable in Azure App Service
az webapp config appsettings set \
  --resource-group myResourceGroup \
  --name myAppName \
  --settings DB_CONNECTION_STRING="your-connection-string"
```

### AWS

Use **AWS Systems Manager Parameter Store** or **AWS Secrets Manager**:

```bash
aws ssm put-parameter \
  --name "/myapp/db-connection-string" \
  --value "your-connection-string" \
  --type "SecureString"
```

### Docker

Use **environment variables** or **Docker secrets**:

```yaml
# docker-compose.yml
services:
  api:
    environment:
      - DB_CONNECTION_STRING=${DB_CONNECTION_STRING}
```

### Environment Variables in CI/CD

Set in your CI/CD platform:
- **GitHub Actions**: Repository Secrets
- **GitLab CI**: CI/CD Variables
- **Azure DevOps**: Pipeline Variables (mark as secret)

## Additional Resources

- [DotNetEnv Documentation](https://github.com/tonerdo/dotnet-env)
- [Supabase Connection Strings](https://supabase.com/docs/guides/database/connecting-to-postgres)
- [.NET Configuration Providers](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [12-Factor App: Config](https://12factor.net/config)
