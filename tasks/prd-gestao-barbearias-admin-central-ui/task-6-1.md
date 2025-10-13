# Task 6.1: List Page with Table and useBarbershops Hook

**Status**: 🔵 Not Started | **Priority**: Alta | **Effort**: 3 days | **Phase**: 6 - CRUD List (PARALLELIZABLE)

## Description
Implement complete barbershop list page with table, search, filters, pagination, and useBarbershops custom hook for data fetching.

## Acceptance Criteria
- [ ] BarbershopList page with table rendering barbershops
- [ ] useBarbershops hook with filters, loading, error states
- [ ] Search by name/email/city with 300ms debounce
- [ ] Filter by status (all/active/inactive)
- [ ] Pagination controls (20 items/page)
- [ ] Actions: View, Edit, Deactivate/Reactivate buttons
- [ ] Loading skeleton during fetch
- [ ] Empty state when no results
- [ ] Filter/search state preserved on navigation
- [ ] Unit and integration tests pass

## Dependencies
**Blocking**: Tasks 5.1-5.4 (Reusable Components), Task 2.3 (API Service)
**Blocked**: None (can parallelize with Tasks 7.1, 8.1, 9.1)

## Implementation Notes
Implement per Tech Spec sections 5.2 (List Page), 5.2.1 (useBarbershops hook).

## Reference
- **Tech Spec**: 5.2 (List Page Implementation)
- **PRD**: Section 1 (Listagem e Busca)

## Next Steps
Can work in parallel with Tasks 7.1, 8.1, 9.1
→ After all CRUD pages: **Task 10.1** (Deactivate/Reactivate)
