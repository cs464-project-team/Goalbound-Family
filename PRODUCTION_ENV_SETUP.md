# 🚀 Production Environment Variables Setup (Render)

## Critical Issue Fixed

**Problem**: Authentication was failing in production with 401 Unauthorized errors.

**Root Cause**: The JWT configuration in `Program.cs` was trying to read from configuration keys that weren't set in production environment variables.

**Solution**: Updated `Program.cs` to properly read from environment variables with fallbacks.

## Required Environment Variables for Render

You MUST set these environment variables in your Render dashboard for the backend to work in production:

### 1. Database Configuration

```bash
DB_HOST=db.YOUR-PROJECT-REF.supabase.co
DB_PORT=5432
DB_NAME=postgres
DB_USER=postgres
DB_PASSWORD=your-supabase-db-password
```

**Where to get these**:
- Go to Supabase Dashboard → Settings → Database → Connection Info

### 2. Supabase Authentication

```bash
VITE_SUPABASE_URL=https://YOUR-PROJECT-REF.supabase.co
VITE_SUPABASE_ANON_KEY=your-supabase-anon-key
```

**Where to get these**:
- Go to Supabase Dashboard → Settings → API
- Copy the **Project URL** for `VITE_SUPABASE_URL`
- Copy the **anon/public** key for `VITE_SUPABASE_ANON_KEY`

### 3. JWT Secret (CRITICAL FOR AUTH)

```bash
JWT_SECRET=your-supabase-jwt-secret
```

**Where to get this**:
- Go to Supabase Dashboard → Settings → API
- Scroll down to **JWT Settings**
- Copy the **JWT Secret** (this is a long string)

⚠️ **This is the most critical variable for authentication to work!**

### 4. Azure OCR (Optional, if using receipt scanning)

```bash
AZURE_VISION_ENDPOINT=https://your-resource.cognitiveservices.azure.com/
AZURE_VISION_KEY=your-azure-vision-key
```

### 5. Application Environment

```bash
ASPNETCORE_ENVIRONMENT=Production
```

## How to Set Environment Variables in Render

1. Go to your Render dashboard
2. Select your backend web service
3. Go to **Environment** tab
4. Click **Add Environment Variable**
5. Add each variable one by one:
   - Key: `JWT_SECRET`
   - Value: `your-actual-jwt-secret-from-supabase`
6. Click **Save Changes**
7. Render will automatically redeploy your service

## Verification Checklist

After setting all environment variables:

- [ ] `DB_HOST` - Supabase database host
- [ ] `DB_PASSWORD` - Supabase database password
- [ ] `VITE_SUPABASE_URL` - Supabase project URL
- [ ] `VITE_SUPABASE_ANON_KEY` - Supabase anon key
- [ ] `JWT_SECRET` - Supabase JWT secret (CRITICAL!)
- [ ] `ASPNETCORE_ENVIRONMENT` - Set to "Production"

## Testing Authentication

After deployment, test these endpoints:

### 1. Health Check (No Auth Required)
```bash
curl https://goalbound-backend.onrender.com/api/auth/me
```
Should return 204 No Content (not logged in)

### 2. Login
```bash
curl -X POST https://goalbound-backend.onrender.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"password123"}'
```
Should return access token and user info

### 3. Protected Endpoint
```bash
curl https://goalbound-backend.onrender.com/api/householdmembers/user/USER_ID \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```
Should return 200 OK with data (not 401)

## Common Issues

### Issue: Still getting 401 Unauthorized

**Check**:
1. Is `JWT_SECRET` set correctly in Render?
2. Does it match the JWT Secret from Supabase Dashboard?
3. Did Render redeploy after adding the variable?

**Solution**:
```bash
# In Render dashboard, verify JWT_SECRET is set
# Then manually trigger a redeploy
```

### Issue: "JWT_SECRET not configured" error in logs

**Cause**: Environment variable not set in Render

**Solution**:
1. Go to Render → Environment tab
2. Add `JWT_SECRET` with your Supabase JWT secret
3. Save and wait for redeploy

### Issue: CORS errors from frontend

**Check**: The frontend URL is whitelisted in `Program.cs`:
```csharp
policy.WithOrigins(
    "http://localhost:5173",
    "https://goalbound-family.vercel.app"  // ✅ Production frontend
)
```

## Security Best Practices

### ✅ DO
- ✅ Use different JWT secrets for dev/staging/production
- ✅ Rotate JWT secret if accidentally exposed
- ✅ Use Render's environment variable encryption
- ✅ Never commit secrets to git

### ❌ DON'T
- ❌ Share JWT secret in chat or email
- ❌ Use the same secret across environments
- ❌ Hardcode secrets in code
- ❌ Log JWT secrets

## Code Changes Made

### Program.cs (Lines 93-122)

Updated JWT configuration to properly read from environment variables:

```csharp
// Try multiple configuration keys for flexibility
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? builder.Configuration["Supabase:JwtSecret"]
    ?? builder.Configuration["JWT_SECRET"]
    ?? throw new InvalidOperationException("JWT_SECRET not configured");

var supabaseUrlForJwt = Environment.GetEnvironmentVariable("VITE_SUPABASE_URL")
    ?? builder.Configuration["Supabase:Url"]
    ?? throw new InvalidOperationException("VITE_SUPABASE_URL not configured");
```

This ensures:
- Environment variables take precedence (for production)
- Fallback to configuration keys (for local development)
- Clear error messages if not configured

## Next Steps

1. **Set all environment variables in Render** (see checklist above)
2. **Wait for automatic redeploy** (or trigger manual redeploy)
3. **Test authentication** using the curl commands above
4. **Verify frontend can authenticate** by logging in at https://goalbound-family.vercel.app

## Need Help?

If authentication still doesn't work after following this guide:

1. Check Render logs for error messages
2. Verify all environment variables are set correctly
3. Ensure JWT_SECRET matches exactly from Supabase
4. Check that frontend is sending Authorization header
