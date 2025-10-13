# Task 3.3: API Interceptors and Session Management

**Status**: 🔵 Not Started
**Priority**: Crítica
**Estimated Effort**: 0.5 day
**Phase**: Phase 3 - Authentication

## Description
Enhance API interceptors for robust session management, including token refresh logic (future), session expiry handling, and request/response logging.

## Acceptance Criteria
- [ ] Request interceptor adds token to all authenticated requests
- [ ] Response interceptor handles 401 (session expired)
- [ ] Automatic redirect to login on session expiry
- [ ] User-friendly session expiry message
- [ ] Integration tests verify interceptor behavior

## Dependencies
**Blocking**: Task 2.3 (Axios Config), Task 3.2 (useAuth)
**Blocked**: All API-dependent pages

## Implementation Notes
Enhance `src/services/api.ts` interceptors created in Task 2.3.

## Reference
- **Tech Spec**: 3.1 (Axios Interceptors)
- **PRD**: Section 8 (Session Management)

## Next Steps
→ **Task 4.1**: React Router Configuration
