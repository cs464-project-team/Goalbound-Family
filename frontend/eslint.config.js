import js from '@eslint/js'
import globals from 'globals'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import tseslint from 'typescript-eslint'
import { defineConfig, globalIgnores } from 'eslint/config'

export default defineConfig([
  globalIgnores(['dist']),

  // 1️Special rules for shadcn UI components (treat as vendor code)
  {
    files: ['src/components/ui/**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
    },
    rules: {
      // Disable annoying rules in library code
      'react-refresh/only-export-components': 'off',
      'react-hooks/purity': 'off',
    },
  },

  // 2️Default rules for the rest of your app
  {
    files: ['**/*.{ts,tsx}'],
    ignores: ['src/components/ui/**/*'], // avoid double-processing
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
    },
    rules: {
      'no-console': 'warn',
      'no-debugger': 'error',
      'react-refresh/only-export-components': 'off',

      // Keep unused-var checks, but only as warning (not build-breaking)
      '@typescript-eslint/no-unused-vars': [
        'warn',
        { argsIgnorePattern: '^_', varsIgnorePattern: '^_' },
      ],
    },
  },
])
