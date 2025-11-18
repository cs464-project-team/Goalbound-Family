# Goalbound Family - Frontend

React + TypeScript + Vite application for Goalbound Family.

## Tech Stack

- **React 18** - UI library
- **TypeScript** - Type safety
- **Vite** - Fast build tool and dev server
- **ESLint** - Code linting

## Getting Started

```bash
# Install dependencies
npm install

# Start dev server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Run linter
npm run lint
```

## Development Server

The dev server runs at `http://localhost:5173` by default.

## Recommended Project Structure

```
src/
├── assets/       # Static assets (images, fonts, etc.)
├── components/   # Reusable React components
├── pages/        # Page components
├── services/     # API service layer
├── hooks/        # Custom React hooks
├── types/        # TypeScript type definitions
├── utils/        # Utility functions
├── App.tsx       # Main App component
└── main.tsx      # Application entry point
```

## Connecting to Backend

The backend API runs at `http://localhost:5xxx` (check backend console for exact port).

Create an API service in `src/services/` to communicate with the backend:

```typescript
// src/services/api.ts
const API_BASE_URL = 'http://localhost:5000'; // Update with actual backend port

export const apiClient = {
  get: async (endpoint: string) => {
    const response = await fetch(`${API_BASE_URL}${endpoint}`);
    return response.json();
  },
  // Add other methods as needed
};
```

## Vite Plugins

This template uses [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) for Fast Refresh.
