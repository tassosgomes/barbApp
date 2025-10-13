# Task 4.1: React Router Configuration

**Status**: 🔵 Not Started
**Priority**: Crítica
**Estimated Effort**: 0.5 day
**Phase**: Phase 4 - Routing and Layout

## Description
Configure React Router v6 with all application routes, including public (login) and protected routes (barbershop management).

## Acceptance Criteria
- [ ] Router configured with all routes from Tech Spec
- [ ] Public route: /login
- [ ] Protected routes: /, /barbearias, /barbearias/nova, /barbearias/:id, /barbearias/:id/editar
- [ ] Default redirect / → /barbearias
- [ ] 404 handling (redirect to /barbearias)
- [ ] Route navigation works without page reload

## Dependencies
**Blocking**: Task 3.2 (ProtectedRoute), Task 1.2 (Structure)
**Blocked**: All page implementations

## Implementation Notes
Create `src/routes/index.tsx` per Tech Spec section 8.1.

## Reference
- **Tech Spec**: 8.1 (React Router Configuration)
- **PRD**: All sections (routing foundation)

## Next Steps
→ **Task 4.2**: Header Component and Base Layout
