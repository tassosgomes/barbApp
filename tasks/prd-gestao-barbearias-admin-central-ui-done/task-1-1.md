# Task 1.1: Project Setup and Initial Configuration

**Status**: ✅ Completed
**Priority**: Crítica
**Estimated Effort**: 1 day
**Phase**: Phase 1 - Foundation

## Description

Set up the complete React Vite project with TypeScript, install all required dependencies, and configure the development environment including TailwindCSS, ESLint, Prettier, and basic project structure.

This is the foundational task that enables all subsequent development work.

## Acceptance Criteria

- [x] Vite project created with React + TypeScript template
- [x] All dependencies from package.json installed successfully
- [x] TailwindCSS configured and working
- [x] ESLint and Prettier configured with project standards
- [x] Project builds successfully (`npm run build`)
- [x] Dev server runs without errors (`npm run dev`)
- [x] Git repository initialized with .gitignore
- [x] Environment variables configuration (.env.example created)

## Dependencies

**Blocking Tasks**: None (First task)

**Blocked Tasks**:
- Task 1.2 (Folder Structure)
- All subsequent tasks depend on this foundation

## Implementation Notes

### Step-by-Step Setup

1. **Create Vite Project**
```bash
npm create vite@latest barbapp-admin -- --template react-ts
cd barbapp-admin
npm install
```

2. **Install Core Dependencies**
```bash
# Routing and Forms
npm install react-router-dom react-hook-form @hookform/resolvers zod

# HTTP Client
npm install axios

# UI Components (shadcn/ui will be added in Task 5.1)
npm install class-variance-authority clsx tailwind-merge lucide-react
```

3. **Install Dev Dependencies**
```bash
# TailwindCSS
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p

# Testing (Vitest + Playwright)
npm install -D vitest @vitest/ui @testing-library/react @testing-library/jest-dom @testing-library/user-event
npm install -D @playwright/test msw

# Code Quality
npm install -D eslint prettier @typescript-eslint/eslint-plugin @typescript-eslint/parser
npm install -D tailwindcss-animate
```

4. **Configure TailwindCSS** (tailwind.config.js from Tech Spec section 10.4)

5. **Configure TypeScript** (tsconfig.json from Tech Spec section 10.3)

6. **Configure Vite** (vite.config.ts from Tech Spec section 10.2)

7. **Configure ESLint** (.eslintrc.cjs from Tech Spec section 10.6)

8. **Create Environment Files**
```bash
# .env.example
VITE_API_URL=http://localhost:5000/api
VITE_APP_NAME=BarbApp Admin
```

9. **Update package.json scripts** (Tech Spec section 10.1)

### Configuration Files Checklist

- [x] `tailwind.config.js` - TailwindCSS configuration with custom theme
- [x] `tsconfig.json` - TypeScript configuration with path aliases
- [x] `vite.config.ts` - Vite configuration with path aliases and proxy
- [x] `.eslintrc.cjs` - ESLint configuration
- [x] `.prettierrc` - Prettier configuration (create new)
- [x] `.env.example` - Environment variables template
- [x] `.gitignore` - Git ignore patterns

### Git Setup

```bash
git init
git add .
git commit -m "chore: initial project setup with Vite, React, TypeScript, TailwindCSS

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

## Testing Requirements

- [x] Run `npm run dev` - Dev server starts on port 3000
- [x] Run `npm run build` - Project builds without errors
- [x] Run `npm run lint` - No linting errors
- [x] Verify TailwindCSS classes work in a test component
- [x] Verify TypeScript path aliases work (`@/` imports)

## Files to Create/Modify

**Create**:
- `package.json` (modified by npm install)
- `tailwind.config.js`
- `tsconfig.json`
- `tsconfig.node.json`
- `vite.config.ts`
- `.eslintrc.cjs`
- `.prettierrc`
- `.env.example`
- `.gitignore`
- `postcss.config.js`

**Modify**:
- `src/App.tsx` (simple test component)
- `src/main.tsx` (React entry point)
- `src/index.css` (TailwindCSS directives)

## Verification Checklist

Before marking as complete:

1. ✅ All dependencies installed (check `node_modules/`)
2. ✅ Dev server runs: `npm run dev`
3. ✅ Build succeeds: `npm run build`
4. ✅ Linting passes: `npm run lint`
5. ✅ TailwindCSS utility classes apply styling
6. ✅ TypeScript compilation works (no errors)
7. ✅ Environment variables load correctly
8. ✅ Git repository initialized with first commit
9. ✅ `.env` file NOT committed (only .env.example)
10. ✅ Project structure matches Tech Spec section 2.1

## Reference

- **Tech Spec Section**: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6, 11.1
- **PRD**: All sections (foundation for entire UI)
- **Stack**: React 18+, Vite 5+, TypeScript 5+, TailwindCSS 3.4+

## Notes

- This task is CRITICAL - all subsequent work depends on this setup
- Take time to verify all configurations are correct
- Test builds and dev server before proceeding
- Commit frequently during setup for easy rollback if needed
- Keep a backup of working configuration files

## Next Steps

After completion:
→ Proceed to **Task 1.2**: Folder Structure and Path Aliases
