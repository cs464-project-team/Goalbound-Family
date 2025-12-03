# Setup Checklist - Fix CORS & Database Issues

## ✅ Issues Fixed

1. **Auth added back** to `/receipt-scanner` route
2. **Frontend URL updated** from port 5000 → 5073
3. **Supabase client** has fallback values

## 🚨 Required: Apply Database Migration

The backend is running on **port 5073** but needs the database tables created.

### Option 1: Run Migration (Recommended)

**In a new terminal:**
```bash
cd backend
dotnet ef database update
```

If this fails, you need to configure your `.env` file first:
```bash
# backend/.env
DB_HOST=your-project.supabase.co
DB_PASSWORD=your-password
DB_NAME=postgres
DB_USER=postgres.your-project-ref
```

### Option 2: Run SQL Manually on Supabase

1. Open Supabase Dashboard: https://app.supabase.com
2. Go to SQL Editor
3. Copy and paste the entire [migration.sql](backend/migration.sql) file
4. Click "Run"

## 🧪 Test After Migration

1. **Restart backend** (Ctrl+C and `dotnet run`)
2. **Login to your app** at http://localhost:5173/auth
3. **Navigate to** http://localhost:5173/receipt-scanner
4. **Upload a receipt** - should work now!

## Current Ports

- **Backend**: http://localhost:5073
- **Frontend**: http://localhost:5173

## What's in the Terminal

Your backend is running but showing a 500 error because the `Receipts` and `ReceiptItems` tables don't exist yet.

After applying the migration, everything should work! 🚀
