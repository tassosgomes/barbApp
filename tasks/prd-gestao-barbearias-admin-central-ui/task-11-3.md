# Task 11.3: Integration Tests for Services

**Status**: 🔵 Not Started | **Priority**: Alta | **Effort**: 0.5 day | **Phase**: 11 - Testing

## Description
Write integration tests for barbershopService methods using MSW to mock API responses.

## Acceptance Criteria
- [ ] Tests for all barbershopService methods (getAll, getById, create, update, deactivate, reactivate)
- [ ] MSW handlers for all endpoints
- [ ] Test success scenarios
- [ ] Test error scenarios (404, 500, network errors)
- [ ] Test request/response data transformations
- [ ] All integration tests pass

## Dependencies
**Blocking**: Task 11.1 (Test Config with MSW), Task 2.3 (API Service)
**Blocked**: None

## Implementation Notes
Follow examples from Tech Spec section 9.4 (Integration Tests).

## Reference
- **Tech Spec**: 9.4 (Integration Tests)
- **PRD**: API reliability requirements

## Next Steps
→ **Task 11.4**: E2E Tests with Playwright
