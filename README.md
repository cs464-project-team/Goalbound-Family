# Goalbound Family

A full-stack web application built with React (Vite + TypeScript) frontend and .NET backend following a controller-service-repository pattern.

## Project Structure

```
CS464-FSD/
├── frontend/          # React + Vite + TypeScript
│   ├── src/
│   ├── public/
│   └── package.json
│
├── backend/           # .NET Web API
│   ├── Controllers/
│   ├── Services/
│   ├── Repositories/
│   ├── Models/
│   ├── DTOs/
│   ├── Data/
│   └── GoalboundFamily.Api.csproj
│
└── README.md
```

## Tech Stack

### Frontend
- React 18
- TypeScript
- Vite
- ESLint

### Backend
- .NET 9.0
- ASP.NET Core Web API
- Entity Framework Core 9.0
- PostgreSQL (via Supabase)
- Controller-Service-Repository pattern

## Quick Start

```bash
# 1. Install EF Core tools
dotnet tool install --global dotnet-ef

# 2. Set up environment variables
cp .env.example .env
# Edit .env and add your Supabase connection string

# 3. Set up backend
cd backend
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run

# 4. In a new terminal, set up frontend
cd frontend
npm install
npm run dev
```

Visit `http://localhost:5173` for the frontend and test the API at `https://localhost:7xxx/api/users`

> 📖 **Need help with environment setup?** See [ENVIRONMENT_SETUP.md](ENVIRONMENT_SETUP.md) for detailed instructions on configuring your `.env` file with Supabase credentials.

## Getting Started

### Prerequisites
- Node.js (v18 or higher)
- .NET 9.0 SDK
- Supabase account (for PostgreSQL database)
- EF Core CLI tools: `dotnet tool install --global dotnet-ef`

### Database Setup (Supabase)

#### 1. Get Supabase Connection String

From your Supabase project:
1. Go to **Project Settings** → **Database**
2. Find your connection info or connection string
3. Note down: Host, Database, Port, User, and Password

#### 2. Configure Connection

**Using .env file (Recommended)**

1. Copy the example file:
   ```bash
   cp .env.example .env
   ```

2. Edit `.env` and add your Supabase credentials (individual values):
   ```bash
   DB_HOST=db.xxxxx.supabase.co
   DB_PORT=5432
   DB_NAME=postgres
   DB_USER=postgres
   DB_PASSWORD=your-actual-password
   ```

3. The `.env` file is already in `.gitignore` and won't be committed to git.

The app automatically builds the connection string from these individual values.

#### 3. Create and Apply Migrations

```bash
cd backend

# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration to Supabase database
dotnet ef database update
```

This will create the database schema on your Supabase PostgreSQL instance.

### Running the Frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend will be available at `http://localhost:5173`

### Running the Backend

```bash
cd backend
dotnet restore
dotnet run
```

Backend API will be available at `https://localhost:7xxx` (check console for exact port)

## Development

Both frontend and backend can run simultaneously:

1. Start the backend API server
2. Start the frontend dev server
3. The frontend is configured with CORS to communicate with the backend

### Working with Database Migrations

After making changes to your models (entities in `backend/Models/`):

```bash
cd backend

# Create a new migration
dotnet ef migrations add DescriptiveMigrationName

# Review the generated migration in Migrations/ folder

# Apply the migration to Supabase
dotnet ef database update

# If you need to rollback
dotnet ef database update PreviousMigrationName

# Remove the last migration (if not yet applied)
dotnet ef migrations remove
```

**Important Notes**:
- Always review migrations before applying them to production
- Supabase PostgreSQL is accessible over SSL - ensure connection string has `SSL Mode=Require`
- You can view your database tables in Supabase Dashboard → Table Editor
- EF Core will create a `__EFMigrationsHistory` table to track applied migrations

📚 **For detailed migration workflows, troubleshooting, and best practices**, see [backend/MIGRATIONS.md](backend/MIGRATIONS.md)

### Viewing Your Database

After applying migrations, you can:
1. **Supabase Dashboard**: Project → Table Editor to view tables and data
2. **SQL Editor**: Run custom queries in Supabase SQL Editor
3. **pgAdmin/TablePlus**: Connect using your Supabase connection string

## Architecture

### Backend Architecture

The backend follows a **monolithic, layered architecture**:

```
┌─────────────────────────────────────────┐
│          Controllers Layer              │  ← HTTP Requests/Responses
│    (UsersController, GoalsController)   │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│          Services Layer                 │  ← Business Logic
│      (UserService, GoalService)         │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│        Repositories Layer               │  ← Data Access
│   (UserRepository, GoalRepository)      │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│       Entity Framework Core             │  ← ORM
│      (ApplicationDbContext)             │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│      Supabase PostgreSQL Database       │  ← Data Storage
│         (Cloud-hosted)                  │
└─────────────────────────────────────────┘
```

**Layer Responsibilities**:

- **Controllers**: Handle HTTP requests and responses
- **Services**: Contain business logic and orchestrate operations
- **Repositories**: Handle data access and persistence using EF Core
- **Models**: Domain entities that map to database tables
- **DTOs**: Data Transfer Objects for API contracts
- **Data**: Database context and EF Core configurations

## Documentation

- [Frontend README](frontend/README.md) - Frontend setup and structure
- [Backend README](backend/README.md) - Backend architecture and patterns
- [Environment Variables Setup](backend/ENV_SETUP.md) - Complete guide to .env configuration
- [Migrations Guide](backend/MIGRATIONS.md) - Complete guide to database migrations with Supabase
- [Setup Guide](backend/SETUP_GUIDE.md) - Quick reference for initial setup
- [Commands Cheatsheet](backend/COMMANDS_CHEATSHEET.md) - Quick reference for all common commands