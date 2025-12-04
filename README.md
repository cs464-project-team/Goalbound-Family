# Goalbound Family 🏠💰

A gamified family budget management application that turns household expense tracking into an engaging experience. Built with React (Vite + TypeScript) frontend and .NET backend following a controller-service-repository pattern.

🚀 **Live Demo**: [https://goalbound-family.vercel.app/](https://goalbound-family.vercel.app/)

## 🌟 Key Features

- **👥 Multi-Household Support** - Create or join multiple households with invitation system
- **📊 Smart Budgeting** - Set category-based budgets and track spending in real-time
- **📸 Receipt OCR** - Scan receipts with Azure Computer Vision API for automatic expense entry
- **🎮 Gamification** - Earn XP, badges, and complete quests through responsible spending
- **📈 Dashboard Analytics** - Visual insights into spending patterns and budget health
- **🏆 Family Leaderboard** - Compete with family members through quests and achievements
- **🔐 Secure Authentication** - JWT-based auth with Supabase integration

## Project Structure

```
Goalbound-Family/
├── frontend/          # React + Vite + TypeScript
│   ├── src/
│   │   ├── components/  # Reusable UI components (shadcn/ui)
│   │   ├── pages/       # Route pages (Dashboard, Expenses, etc.)
│   │   ├── services/    # API and auth services
│   │   ├── hooks/       # Custom React hooks (useAuth, etc.)
│   │   └── context/     # React context providers
│   ├── public/
│   └── package.json
│
├── backend/           # .NET Web API
│   ├── Controllers/     # API endpoints
│   ├── Services/        # Business logic layer
│   ├── Repositories/    # Data access layer
│   ├── Models/          # Entity models
│   ├── DTOs/            # Data transfer objects
│   ├── Data/            # EF Core DbContext
│   ├── Migrations/      # Database migrations
│   └── GoalboundFamily.Api.csproj
│
├── start-app.sh       # Automated startup script
└── README.md
```

## Tech Stack

### Frontend
- **React 19** - UI framework
- **TypeScript** - Type safety
- **Vite** - Build tool and dev server
- **TailwindCSS 4** - Styling
- **shadcn/ui** - Component library (Radix UI primitives)
- **React Router 7** - Client-side routing
- **Vitest** - Unit testing

### Backend
- **.NET 9.0** - Runtime and framework
- **ASP.NET Core Web API** - REST API
- **Entity Framework Core 9.0** - ORM
- **PostgreSQL (Supabase)** - Database
- **JWT Bearer Authentication** - Security
- **Azure Computer Vision** - OCR for receipt scanning
- **xUnit + Moq** - Unit testing
- **Controller-Service-Repository Pattern** - Architecture



**Manual Setup:**

```bash
# 1. Install EF Core tools (one-time setup)
dotnet tool install --global dotnet-ef

# 2. Configure environment variables
cd backend
cp .env.example .env
# Edit .env with your Supabase and Azure credentials

# 3. Apply database migrations
dotnet ef database update

# 4. Start backend (in one terminal)
dotnet run

# 5. Start frontend (in another terminal)
cd ../frontend
npm install
npm run dev
```

Visit `http://localhost:5173` to access the application.

> 📖 **Need help with environment setup?** See [ENVIRONMENT_SETUP.md](ENVIRONMENT_SETUP.md) and [backend/ENV_SETUP.md](backend/ENV_SETUP.md) for detailed configuration instructions.

## Getting Started

### Prerequisites
- **Node.js** (v18 or higher) - [Download](https://nodejs.org/)
- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Supabase Account** - For PostgreSQL database ([Sign up](https://supabase.com))
- **Azure Account** (Optional) - For receipt OCR functionality ([Sign up](https://azure.microsoft.com))
- **EF Core CLI Tools**: `dotnet tool install --global dotnet-ef`

### Environment Configuration

#### Required Environment Variables

Create a `.env` file in the `backend/` directory with the following:

```bash
# Database Configuration (Supabase)
DB_HOST=db.xxxxx.supabase.co
DB_PORT=5432
DB_NAME=postgres
DB_USER=postgres.xxxxx
DB_PASSWORD=your-database-password

# Supabase Authentication
VITE_SUPABASE_URL=https://xxxxx.supabase.co
VITE_SUPABASE_ANON_KEY=your-anon-key
JWT_SECRET=your-jwt-secret

# Azure Computer Vision (Optional - for OCR)
AZURE_VISION_ENDPOINT=https://xxxxx.cognitiveservices.azure.com/
AZURE_VISION_KEY=your-azure-key
```

**Where to find these values:**

1. **Supabase Database Credentials**:
   - Dashboard → Settings → Database → Connection Info
   
2. **Supabase Auth Keys**:
   - Dashboard → Settings → API
   - Copy Project URL and anon/public key
   - JWT Secret is in JWT Settings section

3. **Azure Computer Vision** (Optional):
   - Azure Portal → Create Computer Vision resource
   - Copy Endpoint and Key from Keys and Endpoint section

> 📘 See [backend/ENV_SETUP.md](backend/ENV_SETUP.md) for detailed configuration instructions.

### Database Setup (Supabase)

### Database Setup (Supabase)

#### Apply Migrations to Create Database Schema

```bash
cd backend

# Verify your .env file is configured correctly
cat .env

# Apply all migrations to your Supabase database
dotnet ef database update
```

This creates all necessary tables:
- Users, Households, HouseholdMembers
- Expenses, Receipts, ReceiptItems
- BudgetCategories, HouseholdBudgets
- Invitations, Badges, Quests
- And more...

#### Viewing Your Database

After applying migrations:
1. **Supabase Dashboard**: Project → Table Editor
2. **SQL Editor**: Run custom queries
3. **pgAdmin/TablePlus**: Connect using your connection string

### Running the Application

#### Option 1: Manual Start

**Terminal 1 - Backend:**
```bash
cd backend
dotnet run
# Backend runs on http://localhost:5073
```

**Terminal 2 - Frontend:**
```bash
cd frontend
npm install
npm run dev
# Frontend runs on http://localhost:5173
```

The frontend is configured with CORS to communicate with the backend API.

## 🎯 Application Features

### Authentication & User Management
- Sign up / Sign in with email and password
- JWT-based authentication with HttpOnly cookies
- Session management with automatic token refresh
- User profile management

### Household Management
- Create new households
- Generate invitation links for family members
- Join existing households via invitation tokens
- View and manage household members
- Role-based access (Parent/Child)

### Budget Tracking
- Create custom budget categories
- Set monthly budget limits per category
- Real-time spending tracking
- Visual budget health indicators
- Month-over-month comparisons

### Expense Management
- Manual expense entry
- Receipt scanning with OCR (Azure Computer Vision)
- Automatic expense categorization
- Item-level expense assignment
- Expense history and filtering

### Gamification System
- **XP System**: Earn experience points for responsible spending
- **Badges**: Unlock achievements (Saver, Streak Master, etc.)
- **Quests**: Daily and weekly challenges
- **Leaderboard**: Family rankings based on XP and quest completion
- **Streaks**: Maintain daily login and spending awareness

### Dashboard & Analytics
- Total expenses overview
- Category-wise spending breakdown
- Budget utilization charts
- Recent transactions
- Quick actions and insights

## 📁 Key Application Pages

- **`/auth`** - Login and signup
- **`/dashboard`** - Main overview and analytics
- **`/expenses`** - View and manage expenses
- **`/budgets`** - Configure budget categories and limits
- **`/receipt-scanner`** - Scan and process receipts
- **`/family`** - Household management and invitations
- **`/leaderboard`** - Family rankings and quests
- **`/profile`** - User settings and preferences

## 🛠️ Development

### Working with Database Migrations

After making changes to entity models in `backend/Models/`:

```bash
cd backend

# Create a new migration
dotnet ef migrations add DescriptiveMigrationName

# Review the generated migration in Migrations/ folder

# Apply the migration to Supabase
dotnet ef database update

# Rollback to a previous migration (if needed)
dotnet ef database update PreviousMigrationName

# Remove the last migration (if not yet applied)
dotnet ef migrations remove
```

**Best Practices**:
- Always review migrations before applying to production
- Use descriptive migration names
- Test migrations on a development database first
- Keep migrations small and focused

📚 **For detailed migration workflows**, see [backend/MIGRATIONS.md](backend/MIGRATIONS.md)

### Testing

**Backend Tests:**
```bash
cd backend
dotnet test
```

**Frontend Tests:**
```bash
cd frontend
npm test              # Run in watch mode
npm run test:run      # Run once
npm run test:coverage # With coverage report
```

### Code Structure

**Backend follows Clean Architecture principles:**

```
Controllers → Services → Repositories → Database
     ↓           ↓            ↓
   DTOs    Business Logic  EF Core
```

- **Controllers**: Handle HTTP requests, validation, and responses
- **Services**: Implement business logic and orchestrate operations
- **Repositories**: Abstract data access using EF Core
- **Models**: Domain entities that map to database tables
- **DTOs**: Data Transfer Objects for API contracts

**Frontend follows Component-Based Architecture:**

```
Pages → Components → Services → API
  ↓         ↓           ↓
Hooks    UI Logic    Auth/Data
```

### API Endpoints

**Authentication:**
- `POST /api/auth/login` - User login
- `POST /api/auth/signup` - User registration  
- `POST /api/auth/refresh` - Refresh access token
- `GET /api/auth/me` - Get current user
- `POST /api/auth/logout` - Logout

**Households:**
- `GET /api/households` - Get user's households
- `POST /api/households` - Create new household
- `GET /api/households/{id}` - Get household details
- `GET /api/households/{id}/members` - Get members

**Expenses:**
- `GET /api/expenses` - Get expenses (filterable)
- `POST /api/expenses` - Create expense
- `PUT /api/expenses/{id}` - Update expense
- `DELETE /api/expenses/{id}` - Delete expense

**Receipts:**
- `POST /api/receipts/upload` - Upload receipt image
- `POST /api/receipts/process-ocr` - Process with OCR
- `POST /api/receipts/confirm` - Confirm and save receipt

**Budgets:**
- `GET /api/household-budgets` - Get household budgets
- `POST /api/household-budgets` - Create budget
- `PUT /api/household-budgets/{id}` - Update budget

**Dashboard:**
- `GET /api/dashboard/summary` - Get dashboard analytics

> 📖 Full API documentation: See `backend/GoalboundFamily.Api.http` for request examples

## Architecture

### Backend Architecture

The backend follows a **layered architecture** with clean separation of concerns:

```
┌─────────────────────────────────────────┐
│          Controllers Layer              │  ← HTTP Requests/Responses
│   (AuthController, ExpensesController)  │     API endpoints, DTOs
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│          Services Layer                 │  ← Business Logic
│    (UserService, ExpenseService)        │     Validation, orchestration
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│        Repositories Layer               │  ← Data Access
│ (UserRepository, ExpenseRepository)     │     CRUD operations
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│       Entity Framework Core             │  ← ORM
│      (ApplicationDbContext)             │     Query generation
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│      Supabase PostgreSQL Database       │  ← Data Storage
│         (Cloud-hosted)                  │     Persistent data
└─────────────────────────────────────────┘
```

**Layer Responsibilities**:

- **Controllers**: Handle HTTP requests, validation, and return responses as DTOs
- **Services**: Implement business logic, coordinate between repositories
- **Repositories**: Provide abstraction over EF Core for data operations
- **Models**: Domain entities that represent database tables
- **DTOs**: Data Transfer Objects for API input/output contracts
- **Data**: DbContext configuration and database connection

### Frontend Architecture

Component-based architecture with React:

```
┌─────────────────────────────────────────┐
│           Pages (Routes)                │  ← Top-level route components
│  (Dashboard, Expenses, Budgets, etc.)   │
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│        Reusable Components              │  ← UI components
│   (Layout, Sidebar, Cards, Tables)      │     shadcn/ui + custom
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│      Hooks & Context                    │  ← State management
│    (useAuth, AuthProvider)              │     React hooks, context
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│          Services                       │  ← API communication
│  (authService, authenticatedFetch)      │     HTTP requests, auth
└──────────────┬──────────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│         Backend API                     │  ← REST API calls
│    (JWT Authentication)                 │
└─────────────────────────────────────────┘
```

**Key Patterns**:
- **Authenticated Fetch**: Automatic token refresh on 401 errors
- **Context Providers**: Global auth state management
- **Custom Hooks**: Reusable logic (useAuth, useMobile)
- **Component Composition**: shadcn/ui + custom business components

## 🔒 Security Features

- **JWT Authentication**: Secure token-based auth with Supabase
- **HttpOnly Cookies**: Refresh tokens stored securely
- **Access Token in Memory**: XSS protection (lost on page refresh)
- **Automatic Token Refresh**: Seamless reauthentication on 401
- **Role-Based Access**: Parent/Child role differentiation
- **Input Validation**: Both client and server-side validation
- **CORS Configuration**: Restricted to allowed origins
- **SQL Injection Protection**: EF Core parameterized queries

## 📚 Documentation

### Project Documentation
- [Environment Variables Setup](backend/ENV_SETUP.md) - Complete guide to .env configuration
- [Production Environment Setup](PRODUCTION_ENV_SETUP.md) - Deploying to production (Render)
- [Migrations Guide](backend/MIGRATIONS.md) - Database migration workflows
- [Commands Cheatsheet](backend/COMMANDS_CHEATSHEET.md) - Quick reference for common commands
- [Setup Checklist](SETUP_CHECKLIST.md) - Initial setup verification

### Feature Documentation
- [OCR Implementation](backend/OCR_IMPLEMENTATION.md) - Azure Computer Vision integration
- [JWT Authorization Summary](JWT_AUTHORIZATION_SUMMARY.md) - Authentication flow details
- [Receipt UI Enhancements](RECEIPT_UI_ENHANCEMENTS.md) - Receipt scanning features
- [Parser Improvements](PARSER_IMPROVEMENTS.md) - OCR data processing

### Testing & Quality
- [Backend Tests](backend/Backend.Tests/README.md) - Unit and integration tests
- [Frontend Tests](frontend/tests/) - Component and service tests

## 🚀 Deployment

### Backend Deployment (Render)

1. Create a new Web Service on Render
2. Connect your GitHub repository
3. Configure environment variables (see [PRODUCTION_ENV_SETUP.md](PRODUCTION_ENV_SETUP.md))
4. Set build command: `dotnet restore && dotnet build`
5. Set start command: `dotnet run --urls "http://0.0.0.0:$PORT"`

### Frontend Deployment (Vercel)

1. Connect your GitHub repository to Vercel
2. Set root directory to `frontend/`
3. Configure environment variables:
   - `VITE_API_URL` - Your backend API URL
4. Deploy with automatic CI/CD


Built with ❤️ using React, .NET, and Supabase