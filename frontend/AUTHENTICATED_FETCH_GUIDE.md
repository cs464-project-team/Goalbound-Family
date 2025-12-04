# Using authenticatedFetch for API Calls

The frontend now handles access tokens properly. Here's how to use it:

## What Changed

1. **Access tokens are now stored in memory** (not in cookies)
2. **Refresh tokens remain in HttpOnly cookies** (secure)
3. **All authenticated API calls should use `authenticatedFetch`** instead of regular `fetch`

## How to Use

### Import the helper

```typescript
import { authenticatedFetch } from '../services/authService'
import { getApiUrl } from '../config/api'
```

### Replace regular fetch calls

**Before:**
```typescript
const response = await fetch(getApiUrl('/api/budgets'), {
  method: 'GET',
  credentials: 'include',
})
```

**After:**
```typescript
const response = await authenticatedFetch(getApiUrl('/api/budgets'), {
  method: 'GET',
})
```

### Examples

#### GET Request
```typescript
const response = await authenticatedFetch(getApiUrl('/api/budgets'))
const budgets = await response.json()
```

#### POST Request
```typescript
const response = await authenticatedFetch(getApiUrl('/api/budgets'), {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify(budgetData),
})
```

#### DELETE Request
```typescript
const response = await authenticatedFetch(getApiUrl(`/api/budgets/${id}`), {
  method: 'DELETE',
})
```

## What authenticatedFetch Does Automatically

1. **Adds Authorization header**: `Authorization: Bearer {accessToken}`
2. **Includes credentials**: For cookie-based refresh token
3. **Handles 401 errors**: Automatically refreshes the token and retries the request once
4. **No manual token management**: You don't need to worry about tokens

## Files That Need Updating

Search for `fetch(getApiUrl(` in these files and replace with `authenticatedFetch`:

- `/src/pages/Budgets.tsx`
- `/src/pages/Dashboard.tsx`
- `/src/pages/Family.tsx`
- `/src/pages/Expenses.tsx`
- `/src/components/ReceiptUploadWithAssignment.tsx`
- `/src/components/ReceiptUpload.tsx`
- `/src/pages/AcceptInvite.tsx`
- And any other files making API calls to protected endpoints

## Important Notes

- **Don't use `authenticatedFetch` for auth endpoints** (login, signup, logout) - those use regular `fetch`
- **The access token is stored in memory only** - it's cleared on page refresh, which is why we have the refresh mechanism
- **Your friend's backend needs to**:
  1. Return `accessToken` in the login/signup response body
  2. Create a `/api/auth/refresh` endpoint that returns a new access token
  3. Configure JWT Bearer authentication to validate the `Authorization: Bearer {token}` header
