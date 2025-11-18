# 🔐 Environment Variables Setup

## Quick Start

### Step 1: Create Your .env File

Copy the example file to create your own `.env`:

```bash
cp .env.example .env
```

### Step 2: Add Your Supabase Credentials

Open `.env` in your editor and update with your actual values:

```bash
DB_HOST=db.YOUR-PROJECT-REF.supabase.co
DB_PORT=5432
DB_NAME=postgres
DB_USER=postgres
DB_PASSWORD=YOUR-ACTUAL-PASSWORD
```

The app will automatically build the connection string from these individual values.

## Required Environment Variables

### DB_HOST

**What it is**: Your Supabase database host address

**Where to get it**:
1. Go to [Supabase Dashboard](https://supabase.com/dashboard)
2. Select your project
3. Navigate to **Settings** → **Database** → **Connection Info**
4. Find the **Host** field (e.g., `db.abcdefghijk.supabase.co`)

**Example**: `db.abcdefghijklmnop.supabase.co`

### DB_PORT

**What it is**: PostgreSQL port number

**Default**: `5432` (always this for Supabase)

### DB_NAME

**What it is**: Database name

**Default**: `postgres` (Supabase default)

### DB_USER

**What it is**: Database username

**Default**: `postgres` (Supabase default)

### DB_PASSWORD

**What it is**: Your Supabase project's database password

**Where to get it**:
- You set this when creating your Supabase project
- You can reset it in: **Settings** → **Database** → **Database Password** → **Reset Database Password**

**Important**: This is different from your Supabase account password!

### Example .env File

```bash
# ============================================
# Goalbound Family - Required Configuration
# ============================================

# Supabase Database Credentials
DB_HOST=db.abcdefghijklmnop.supabase.co
DB_PORT=5432
DB_NAME=postgres
DB_USER=postgres
DB_PASSWORD=my-super-secret-password-123

# Optional: Application Environment (defaults to Development)
ASPNETCORE_ENVIRONMENT=Development
```

## Optional Environment Variables

### ASPNETCORE_ENVIRONMENT
**Default**: `Development`
**Purpose**: Sets the application runtime environment
**Options**: `Development`, `Staging`, `Production`

### FRONTEND_URL
**Default**: `http://localhost:5173`
**Purpose**: Override the default frontend URL for CORS
**Example**: `FRONTEND_URL=http://localhost:3000`

## Verification

After setting up your `.env` file, verify it works:

```bash
cd backend
dotnet run
```

✅ **Success looks like**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7xxx
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand...
```

❌ **Error looks like**:
```
Unhandled exception. System.ArgumentNullException: Value cannot be null. (Parameter 'connectionString')
```

If you see an error, check:
1. `.env` file exists in project root or `backend/` directory
2. All required variables are set: `DB_HOST` and `DB_PASSWORD` (minimum required)
3. Variable names are spelled correctly (case-sensitive!)
4. No extra spaces around `=` signs
5. Password doesn't contain special characters that need escaping

## Security Notes

### ✅ DO
- ✅ Use `.env` for local development
- ✅ Keep `.env` file out of git (already in `.gitignore`)
- ✅ Share `.env.example` with team members
- ✅ Use different credentials for dev/staging/production
- ✅ Rotate credentials if accidentally exposed

### ❌ DON'T
- ❌ Commit `.env` to version control
- ❌ Share `.env` via email or Slack
- ❌ Use production credentials in development
- ❌ Store `.env` in cloud storage unencrypted

## Troubleshooting

### Problem: "Database connection string not configured"

**Cause**: `.env` file not found or required variables not set

**Solution**:
```bash
# Check if .env exists
ls -la .env

# Check contents (required variables)
cat .env | grep DB_HOST
cat .env | grep DB_PASSWORD

# Should show your values
# DB_HOST=db.xxxxx.supabase.co
# DB_PASSWORD=your-password
```

### Problem: "Cannot connect to database"

**Cause**: Invalid connection string or wrong credentials

**Solution**:
1. Verify Supabase project is active (not paused)
2. Check password is correct
3. Ensure SSL Mode and Trust Server Certificate are included
4. Test connection in Supabase Dashboard → SQL Editor

### Problem: ".env file not loading"

**Cause**: Filename or location issue

**Solution**:
```bash
# File must be named exactly .env (not .env.txt)
ls -la | grep "\.env"

# Should be in backend/ directory or project root
pwd
ls -la .env
```

## Next Steps

After configuring your `.env` file:

1. **Run migrations**:
   ```bash
   cd backend
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

2. **Start the backend**:
   ```bash
   dotnet run
   ```

3. **Verify connection**: Check that you can see the database tables in Supabase Dashboard → Table Editor

## Need Help?

- 📖 [Complete Environment Variables Guide](backend/ENV_SETUP.md)
- 📖 [Backend README](backend/README.md)
- 📖 [Migrations Guide](backend/MIGRATIONS.md)
