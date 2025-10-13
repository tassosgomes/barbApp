# Task 1.2: Folder Structure and Path Aliases

**Status**: 🔵 Not Started
**Priority**: Crítica
**Estimated Effort**: 0.5 day
**Phase**: Phase 1 - Foundation

## Description

Create the complete folder structure for the application following the organization defined in the Tech Spec, configure TypeScript path aliases, and set up barrel exports (index.ts files) for clean imports.

This establishes the organizational foundation that all future code will follow.

## Acceptance Criteria

- [ ] Complete `src/` folder structure created matching Tech Spec
- [ ] Path aliases configured and working (`@/` imports)
- [ ] Barrel export files (`index.ts`) created in key directories
- [ ] Empty placeholder files created where needed for structure visualization
- [ ] TypeScript recognizes all path aliases
- [ ] VSCode (or IDE) autocomplete works with path aliases
- [ ] Sample import tests pass

## Dependencies

**Blocking Tasks**:
- Task 1.1 (Project Setup) must be completed first

**Blocked Tasks**:
- Task 2.1 (TypeScript Types) - needs types/ folder
- Task 2.2 (Zod Schemas) - needs schemas/ folder
- Task 2.3 (API Services) - needs services/ folder
- All subsequent tasks depend on proper structure

## Implementation Notes

### Folder Structure to Create

Based on Tech Spec section 2.1:

```
barbapp-admin/
├── src/
│   ├── assets/                    # Images, icons, static files
│   ├── components/
│   │   ├── ui/                    # shadcn/ui components (Task 5.1)
│   │   ├── layout/                # Header, Sidebar, Footer
│   │   │   └── index.ts
│   │   └── barbershop/            # Domain-specific components
│   │       └── index.ts
│   ├── pages/
│   │   ├── Login/
│   │   │   └── index.ts
│   │   └── Barbershops/
│   │       └── index.ts
│   ├── services/
│   │   └── index.ts
│   ├── hooks/
│   │   └── index.ts
│   ├── types/
│   │   └── index.ts
│   ├── schemas/
│   │   └── index.ts
│   ├── utils/
│   │   └── index.ts
│   ├── routes/
│   │   └── index.tsx
│   ├── App.tsx
│   ├── main.tsx
│   └── index.css
```

### Path Alias Configuration

Already configured in Task 1.1 (`tsconfig.json`), but verify:

```json
{
  "compilerOptions": {
    "baseUrl": ".",
    "paths": {
      "@/*": ["./src/*"]
    }
  }
}
```

And in `vite.config.ts`:

```typescript
import path from 'path';

export default defineConfig({
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
});
```

### Barrel Exports Pattern

Create `index.ts` files in key directories to enable clean imports:

```typescript
// Example: src/types/index.ts
export * from './barbershop';
export * from './auth';
export * from './pagination';

// Usage:
// import { Barbershop, LoginRequest, PaginatedResponse } from '@/types';
```

### Placeholder Files

Create empty TypeScript files with TODO comments to maintain structure:

```typescript
// src/services/api.ts
// TODO: Implement Axios configuration (Task 2.3)
export {};

// src/services/barbershop.service.ts
// TODO: Implement barbershop service methods (Task 2.3)
export {};
```

## Testing Requirements

- [ ] Import from `@/types` works without errors
- [ ] Import from `@/components/ui` works (after creating folder)
- [ ] Import from `@/services` works
- [ ] TypeScript doesn't show path errors in imports
- [ ] VSCode shows autocomplete suggestions for `@/` imports
- [ ] Test file with multiple path alias imports compiles successfully

**Test File Example** (`src/test-imports.ts`):
```typescript
// This file tests path aliases - delete after verification
import type { } from '@/types';
import type { } from '@/services';
import type { } from '@/hooks';
import type { } from '@/utils';

console.log('Path aliases working correctly!');
```

## Files to Create/Modify

**Directories to Create**:
- `src/assets/`
- `src/components/ui/`
- `src/components/layout/`
- `src/components/barbershop/`
- `src/pages/Login/`
- `src/pages/Barbershops/`
- `src/services/`
- `src/hooks/`
- `src/types/`
- `src/schemas/`
- `src/utils/`
- `src/routes/`

**Files to Create**:
- `src/components/layout/index.ts`
- `src/components/barbershop/index.ts`
- `src/pages/Login/index.ts`
- `src/pages/Barbershops/index.ts`
- `src/services/index.ts`
- `src/hooks/index.ts`
- `src/types/index.ts`
- `src/schemas/index.ts`
- `src/utils/index.ts`
- `src/routes/index.tsx`
- Placeholder service files
- Test imports file (temporary)

**Files to Verify** (from Task 1.1):
- `tsconfig.json` - path aliases configured
- `vite.config.ts` - path aliases configured

## Verification Checklist

Before marking as complete:

1. ✅ All directories exist and are properly nested
2. ✅ All barrel export files (`index.ts`) created
3. ✅ Placeholder files created with TODO comments
4. ✅ Path aliases work in imports (`@/types`, `@/services`, etc.)
5. ✅ TypeScript compilation succeeds with no path errors
6. ✅ VSCode autocomplete suggests files from `@/` imports
7. ✅ Test imports file compiles without errors
8. ✅ Project structure matches Tech Spec diagram exactly
9. ✅ Git commit created for folder structure

## Reference

- **Tech Spec Section**: 2.1 (Project Structure)
- **PRD**: N/A (Technical organization task)
- **Pattern**: Domain-driven folder organization

## Notes

- Use consistent naming: kebab-case for folders, PascalCase for component files
- Keep barrel exports updated as new files are added
- Don't commit empty folders without at least a .gitkeep or index.ts
- This structure supports scalability - easy to add new features later

## Script to Create Structure

```bash
#!/bin/bash
# create-structure.sh - Run from project root

mkdir -p src/assets
mkdir -p src/components/ui
mkdir -p src/components/layout
mkdir -p src/components/barbershop
mkdir -p src/pages/Login
mkdir -p src/pages/Barbershops
mkdir -p src/services
mkdir -p src/hooks
mkdir -p src/types
mkdir -p src/schemas
mkdir -p src/utils
mkdir -p src/routes

# Create barrel exports
touch src/components/layout/index.ts
touch src/components/barbershop/index.ts
touch src/pages/Login/index.ts
touch src/pages/Barbershops/index.ts
touch src/services/index.ts
touch src/hooks/index.ts
touch src/types/index.ts
touch src/schemas/index.ts
touch src/utils/index.ts

echo "Folder structure created successfully!"
```

## Next Steps

After completion:
→ Proceed to **Task 2.1**: TypeScript Types and Interfaces
