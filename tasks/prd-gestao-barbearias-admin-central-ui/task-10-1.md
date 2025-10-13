# Task 10.1: Deactivate and Reactivate Functionality

**Status**: 🔵 Not Started | **Priority**: Média | **Effort**: 1 day | **Phase**: 10 - Additional Features

## Description
Implement complete deactivate and reactivate functionality with confirmation modals and status updates across all pages.

## Acceptance Criteria
- [ ] Deactivate action opens confirmation modal with consequences
- [ ] Reactivate action opens confirmation modal
- [ ] Modal shows barbershop name and code
- [ ] Confirm button calls service method
- [ ] Success: Toast notification, update UI status
- [ ] Error: Display error message, keep modal open
- [ ] List page updates item status immediately
- [ ] Details page updates status and action button
- [ ] Integration tests for both flows

## Dependencies
**Blocking**: Tasks 6.1, 9.1 (List and Details pages), Task 5.4 (DeactivateModal)
**Blocked**: None

## Implementation Notes
Use DeactivateModal from Task 5.4, integrate with barbershopService.deactivate/reactivate methods.

## Reference
- **Tech Spec**: 5.2, 5.5 (Deactivate/Reactivate implementation)
- **PRD**: Section 4 (Desativar/Reativar Barbearia)

## Next Steps
→ **Task 11.1**: Test Configuration and Setup
