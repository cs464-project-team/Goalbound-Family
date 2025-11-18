# Goalbound Family API

ASP.NET Core Web API following a Controller-Service-Repository pattern.

## Project Structure

```
GoalboundFamily.Api/
├── Controllers/           # API endpoints and request/response handling
├── Services/             # Business logic layer
│   └── Interfaces/       # Service contracts
├── Repositories/         # Data access layer
│   └── Interfaces/       # Repository contracts
├── Models/               # Domain entities
├── DTOs/                 # Data Transfer Objects
├── Data/                 # Database context and configurations
├── Properties/           # Launch settings
├── Program.cs            # Application entry point and configuration
├── appsettings.json      # Application configuration
└── GoalboundFamily.Api.csproj
```

## Architecture Pattern

This project follows a **monolithic, layered architecture** with clear separation of concerns:

### 1. **Controllers** (`Controllers/`)
- Handle HTTP requests and responses
- Validate input data
- Delegate business logic to services
- Return appropriate HTTP status codes

### 2. **Services** (`Services/`)
- Contain business logic
- Orchestrate operations between repositories
- Implement domain rules and validations
- Register in `Program.cs` using Dependency Injection

### 3. **Repositories** (`Repositories/`)
- Handle data access and persistence
- Abstract database operations
- Implement CRUD operations
- Use Entity Framework Core or other ORMs

### 4. **Models** (`Models/`)
- Define domain entities
- Represent database tables
- Contain domain logic and validations

### 5. **DTOs** (`DTOs/`)
- Data Transfer Objects for API contracts
- Separate internal models from external API
- Request/Response objects

### 6. **Data** (`Data/`)
- Database context (e.g., `ApplicationDbContext`)
- Entity configurations
- Migrations

## Database Setup (Supabase + PostgreSQL)

### 1. Get Supabase Connection String

From your Supabase project dashboard:
1. Go to **Settings** → **Database**
2. Find the **Connection string** section
3. Copy the connection string in this format:
   ```
   postgresql://postgres:[YOUR-PASSWORD]@db.[YOUR-PROJECT-REF].supabase.co:5432/postgres
   ```

### 2. Configure Connection String

Update [appsettings.json](appsettings.json) with your Supabase credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.your-project-ref.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=your-password;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

**Security Note**: Never commit real credentials! Use environment variables or user secrets for production:

```bash
# Use .NET User Secrets for development
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
```

### 3. Create and Apply Migrations

```bash
# Install EF Core CLI tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration to Supabase database
dotnet ef database update
```

## Running the Application

```bash
# Restore dependencies
dotnet restore

# Run the application
dotnet run

# Run with watch mode (auto-reload)
dotnet watch run
```

The API will be available at:
- HTTPS: `https://localhost:7xxx`
- HTTP: `http://localhost:5xxx`

## CORS Configuration

CORS is configured to allow requests from the frontend Vite dev server (`http://localhost:5173`).

Update the CORS policy in [Program.cs](Program.cs#L32) if your frontend runs on a different port.

## Entity Framework Core Repository Pattern

### Base Repository

All repositories inherit from `Repository<T>` which provides:

- **Read**: `GetByIdAsync()`, `GetAllAsync()`, `FindAsync()`, `FirstOrDefaultAsync()`
- **Create**: `AddAsync()`, `AddRangeAsync()`
- **Update**: `UpdateAsync()`, `UpdateRangeAsync()`
- **Delete**: `DeleteAsync()`, `DeleteByIdAsync()`, `DeleteRangeAsync()`
- **Utilities**: `ExistsAsync()`, `CountAsync()`, `SaveChangesAsync()`

### Example Usage

See [UsersController.cs](Controllers/UsersController.cs), [UserService.cs](Services/UserService.cs), and [UserRepository.cs](Repositories/UserRepository.cs) for a complete working example.

## Adding New Features

Follow this pattern for each new entity:

### 1. Create the Model
```csharp
// Models/Goal.cs
public class Goal
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    // ... other properties
}
```

### 2. Add DbSet to ApplicationDbContext
```csharp
// Data/ApplicationDbContext.cs
public DbSet<Goal> Goals { get; set; }
```

### 3. Create DTOs
```csharp
// DTOs/GoalDto.cs
public class GoalDto { /* ... */ }
public class CreateGoalRequest { /* ... */ }
```

### 4. Create Repository Interface
```csharp
// Repositories/Interfaces/IGoalRepository.cs
public interface IGoalRepository : IRepository<Goal>
{
    // Add custom methods if needed
}
```

### 5. Implement Repository
```csharp
// Repositories/GoalRepository.cs
public class GoalRepository : Repository<Goal>, IGoalRepository
{
    public GoalRepository(ApplicationDbContext context) : base(context) { }
    // Implement custom methods
}
```

### 6. Create Service Interface & Implementation
```csharp
// Services/Interfaces/IGoalService.cs & Services/GoalService.cs
```

### 7. Create Controller
```csharp
// Controllers/GoalsController.cs
[ApiController]
[Route("api/[controller]")]
public class GoalsController : ControllerBase { /* ... */ }
```

### 8. Register in Program.cs
```csharp
builder.Services.AddScoped<IGoalRepository, GoalRepository>();
builder.Services.AddScoped<IGoalService, GoalService>();
```

### 9. Create and Apply Migration
```bash
dotnet ef migrations add AddGoalEntity
dotnet ef database update
```
