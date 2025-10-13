# Task 5.2: Custom Form Components

**Status**: 🔵 Not Started | **Priority**: Alta | **Effort**: 0.5 day | **Phase**: 5 - Reusable Components

## Description
Create reusable form components: FormField wrapper, MaskedInput for Brazilian formats (CEP, phone), and SelectField wrapper.

## Acceptance Criteria
- [ ] FormField component with label, input, error message
- [ ] MaskedInput for phone (99) 99999-9999 and CEP 99999-999
- [ ] SelectField wrapper with consistent styling
- [ ] Components accept react-hook-form props
- [ ] Unit tests for mask formatting

## Dependencies
**Blocking**: Task 5.1 (shadcn/ui)
**Blocked**: Tasks 7.1 (Create), 8.1 (Edit)

## Implementation Notes
Create mask utilities in `src/utils/formatters.ts` and wrapper components.

## Reference
- **Tech Spec**: 6.3 (BarbershopForm), 7.2 (Formatters)
- **PRD**: Section 2 (Form Requirements)

## Next Steps
→ **Task 5.3**: Table and Data Display Components
