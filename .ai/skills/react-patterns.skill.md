SKILL: React patterns for VocaPlay
- All API calls go through src/api/*.ts — never fetch() directly in components
- useAuth() hook from context/AuthContext — never read localStorage directly in components
- Protected routes wrap all authenticated pages via ProtectedRoute component
- Loading states: show spinner while API call in-flight, never render partial data
- Error states: show inline error message below the form field or at top of page
- ChatWidget is rendered in App.tsx outside Router so it persists across navigation
- No prop drilling deeper than 2 levels — use context or co-locate state
- TypeScript: all API response shapes typed in src/types/index.ts
