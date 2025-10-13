# Task 3.2: Auth Hooks and Protected Routes

**Status**: 🔵 Not Started
**Priority**: Crítica
**Estimated Effort**: 0.5 day
**Phase**: Phase 3 - Authentication

## Description
Create useAuth hook for authentication state management and ProtectedRoute component to guard authenticated routes.

## Acceptance Criteria
- [ ] useAuth hook with isAuthenticated, logout methods
- [ ] ProtectedRoute component redirects unauthenticated users to login
- [ ] Token validation on app load
- [ ] Logout clears token and redirects
- [ ] Hook and component unit tested

## Dependencies
**Blocking**: Task 3.1 (Login Page)
**Blocked**: Task 4.1 (Routing) - uses ProtectedRoute

## Implementation Notes
Create `src/hooks/useAuth.ts` and `src/components/ProtectedRoute.tsx` per Tech Spec section 7.3.

## Reference
- **Tech Spec**: 8.2 (ProtectedRoute), 7.3 (useAuth hook)
- **PRD**: Section 8 (Access and Security)

## Next Steps
→ **Task 3.3**: API Interceptors and Session Management
