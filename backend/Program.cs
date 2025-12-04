using DotNetEnv;
using GoalboundFamily.Api.Data;
using GoalboundFamily.Api.Repositories;
using GoalboundFamily.Api.Repositories.Interfaces;
using GoalboundFamily.Api.Services;
using GoalboundFamily.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
var contentRoot = Directory.GetCurrentDirectory();

// Load environment variables from .env file
if (environment == "Development")
{
    var devEnvFile = Path.Combine(contentRoot, ".env.development");
    var defaultEnvFile = Path.Combine(contentRoot, ".env");

    // Try .env.development first, fallback to .env
    if (File.Exists(devEnvFile))
    {
        Env.Load(devEnvFile);
    }
    else if (File.Exists(defaultEnvFile))
    {
        Env.Load(defaultEnvFile);
    }
}
else if (environment == "Testing")
{
    Env.Load(Path.Combine(contentRoot, ".env.testing"));
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddAuthorization();

// Configure Database (Supabase PostgreSQL)
// Build connection string from individual environment variables
var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "postgres";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");


string connectionString;
if (!string.IsNullOrEmpty(dbHost) && !string.IsNullOrEmpty(dbPassword))
{
    // Build connection string from individual environment variables
    // Add Timeout and Server Compatibility Mode for better Supabase connection
    connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword};SSL Mode=Require;Trust Server Certificate=true;Timeout=30;CommandTimeout=30;Pooling=true;MinPoolSize=0;MaxPoolSize=10";
}
else
{
    // Fallback to appsettings.json
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Database connection string not configured. Please set DB_HOST and DB_PASSWORD in .env file.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null
        )
    )
);

// Configure CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                  "http://localhost:5173", // Default Vite dev server port
                  "https://goalbound-family.vercel.app" // Production frontend
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ✅ ADD THIS: Configure JWT Authentication
var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? throw new InvalidOperationException("JWT_SECRET not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Supabase:Url"] + "/auth/v1",
        ValidateAudience = true,
        ValidAudience = "authenticated", // Supabase uses "authenticated" as audience
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configure HttpClient for Python OCR service
builder.Services.AddHttpClient();

// Register Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IReceiptRepository, ReceiptRepository>();
builder.Services.AddScoped<IHouseholdRepository, HouseholdRepository>();
builder.Services.AddScoped<IHouseholdMemberRepository, HouseholdMemberRepository>();
builder.Services.AddScoped<IInvitationRepository, InvitationRepository>();
builder.Services.AddScoped<IBudgetCategoryRepository, BudgetCategoryRepository>();
builder.Services.AddScoped<IHouseholdBudgetRepository, HouseholdBudgetRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IMemberQuestRepository, MemberQuestRepository>();
builder.Services.AddScoped<IQuestRepository, QuestRepository>();
// Add more repositories here as you create them

// Register Services
builder.Services.AddScoped<ISupabaseAuthService, SupabaseAuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHouseholdService, HouseholdService>();
builder.Services.AddScoped<IHouseholdMemberService, HouseholdMemberService>();
builder.Services.AddScoped<IInvitationService, InvitationService>();
builder.Services.AddScoped<IBudgetCategoryService, BudgetCategoryService>();
builder.Services.AddScoped<IHouseholdBudgetService, HouseholdBudgetService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IMemberQuestService, MemberQuestService>();
builder.Services.AddScoped<IImagePreprocessingService, ImagePreprocessingService>();
builder.Services.AddScoped<IOcrService, AzureOcrService>(); // Azure Computer Vision (95-99% accuracy)
builder.Services.AddScoped<IReceiptParserService, ReceiptParserService>();
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddScoped<IBudgetCategoryService, BudgetCategoryService>();
builder.Services.AddScoped<ISupabaseStorageService, SupabaseStorageService>();

// Configure Supabase Client for Storage
var supabaseUrl = Environment.GetEnvironmentVariable("VITE_SUPABASE_URL")
    ?? throw new InvalidOperationException("VITE_SUPABASE_URL not configured in .env file");
var supabaseKey = Environment.GetEnvironmentVariable("VITE_SUPABASE_ANON_KEY")
    ?? throw new InvalidOperationException("VITE_SUPABASE_ANON_KEY not configured in .env file");

builder.Services.AddScoped(_ =>
{
    var options = new Supabase.SupabaseOptions
    {
        AutoConnectRealtime = false  // We don't need realtime for storage
    };
    return new Supabase.Client(supabaseUrl, supabaseKey, options);
});

// Register Supabase Storage Service


var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }