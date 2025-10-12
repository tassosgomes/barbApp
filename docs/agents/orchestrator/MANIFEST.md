# MANIFEST — barbApp
Generated on: 2025-10-12-10:48:38
Orchestrator Path: /docs/agents/orchestrator

## Tracked Reports

### Architecture Analysis
- **Title:** Architectural Analysis Report
- **Path:** /docs/agents/architectural-analyzer/architectural-report-2025-10-12-10:50:45.md
- **Agent:** architectural-analyzer
- **Timestamp:** 2025-10-12-10:50:45
- **Status:** ✅ Completed

### Dependency Audit
- **Title:** Dependency Audit Report - barbApp
- **Path:** /docs/agents/dependency-auditor/dependencies-report-2025-10-12-10:54:56.md
- **Agent:** dependency-auditor
- **Timestamp:** 2025-10-12-10:54:56
- **Status:** ✅ Completed

### Component Analysis
Status: Pending
Total components identified: 74

#### Domain Layer Components (20 components)
- [ ] 1. Barbershop Entity
- [ ] 2. AdminCentralUser Entity
- [ ] 3. AdminBarbeariaUser Entity
- [ ] 4. Barber Entity
- [ ] 5. Customer Entity
- [ ] 6. BarbeariaCode Value Object
- [ ] 7. ITenantContext Interface
- [ ] 8. IAdminCentralUserRepository Interface
- [ ] 9. IAdminBarbeariaUserRepository Interface
- [ ] 10. IBarberRepository Interface
- [ ] 11. ICustomerRepository Interface
- [ ] 12. IBarbershopRepository Interface
- [ ] 13. DomainException
- [ ] 14. UnauthorizedException
- [ ] 15. UnauthorizedAccessException
- [ ] 16. ForbiddenException
- [ ] 17. NotFoundException
- [ ] 18. BarbeariaInactiveException
- [ ] 19. InvalidBarbeariaCodeException
- [ ] 20. ValidationException (Domain)

#### Application Layer Components (28 components)
- [ ] 21. IAuthenticationService Interface
- [ ] 22. IJwtTokenGenerator Interface
- [ ] 23. IPasswordHasher Interface
- [ ] 24. IAuthenticateAdminCentralUseCase Interface
- [ ] 25. IAuthenticateAdminBarbeariaUseCase Interface
- [ ] 26. IAuthenticateBarbeiroUseCase Interface
- [ ] 27. IAuthenticateClienteUseCase Interface
- [ ] 28. IListBarbeirosBarbeariaUseCase Interface
- [ ] 29. ITrocarContextoBarbeiroUseCase Interface
- [ ] 30. AuthenticateAdminCentralUseCase
- [ ] 31. AuthenticateAdminBarbeariaUseCase
- [ ] 32. AuthenticateBarbeiroUseCase
- [ ] 33. AuthenticateClienteUseCase
- [ ] 34. ListBarbeirosBarbeariaUseCase
- [ ] 35. TrocarContextoBarbeiroUseCase
- [ ] 36. LoginAdminCentralInputValidator
- [ ] 37. LoginAdminBarbeariaInputValidator
- [ ] 38. LoginBarbeiroInputValidator
- [ ] 39. LoginClienteInputValidator
- [ ] 40. TrocarContextoInputValidator
- [ ] 41. AuthenticationOutput DTO
- [ ] 42. AuthResponse DTO
- [ ] 43. LoginClienteInput DTO
- [ ] 44. LoginAdminBarbeariaInput DTO
- [ ] 45. LoginAdminCentralInput DTO
- [ ] 46. LoginBarbeiroInput DTO
- [ ] 47. TrocarContextoInput DTO
- [ ] 48. BarberInfo DTO

#### Infrastructure Layer Components (18 components)
- [ ] 49. BarbAppDbContext
- [ ] 50. TenantContext Service
- [ ] 51. JwtTokenGenerator Service
- [ ] 52. PasswordHasher Service
- [ ] 53. TenantMiddleware
- [ ] 54. GlobalExceptionHandlerMiddleware
- [ ] 55. AdminCentralUserRepository
- [ ] 56. AdminBarbeariaUserRepository
- [ ] 57. BarberRepository
- [ ] 58. CustomerRepository
- [ ] 59. BarbershopRepository
- [ ] 60. AdminCentralUserConfiguration
- [ ] 61. AdminBarbeariaUserConfiguration
- [ ] 62. BarberConfiguration
- [ ] 63. CustomerConfiguration
- [ ] 64. BarbershopConfiguration
- [ ] 65. JwtSettings Configuration Class
- [ ] 66. MiddlewareExtensions

#### API Layer Components (4 components)
- [ ] 67. AuthController
- [ ] 68. ValidationConfiguration Extension
- [ ] 69. SwaggerExamplesSchemaFilter
- [ ] 70. Program (Startup Configuration)

#### Test Components (4 components)
- [ ] 71. BarbApp.Domain.Tests Project
- [ ] 72. BarbApp.Application.Tests Project
- [ ] 73. BarbApp.Infrastructure.Tests Project
- [ ] 74. BarbApp.IntegrationTests Project

## Workflow

### Phase 1: Initialization
- **Status**: ✅ Completed
- **Timestamp**: 2025-10-12-10:48:38
- **Actions**: Directory structure created, MANIFEST.md initialized

### Phase 2: Parallel Analysis
- **Status**: ✅ Completed
- **Timestamp**: 2025-10-12-10:54:56
- **Target Agents**:
  - Dependency Auditor (✅ Completed)
  - Architectural Analyzer (✅ Completed)
- **Outputs**: 2 reports successfully generated
  - Architectural report: 74 components identified
  - Dependency report: 27 packages analyzed, 2 CVEs found

### Phase 3: Component Deep Analysis
- **Status**: 🔄 Ready to Begin
- **Note**: Component list received from Architectural Analyzer
- **Scope**: 74 components across 5 layers
  - Domain: 20 components
  - Application: 28 components
  - Infrastructure: 18 components
  - API: 4 components
  - Tests: 4 components

### Phase 4: Final Synthesis
- **Status**: ⏳ Pending
- **Output**: README.md in orchestrator directory

## General Information

- **Project folder**: /home/tsgomes/github-tassosgomes/barbApp
- **Output folder**: docs/agents/
- **Ignore folders**: node_modules, .git, venv, .env, dist, build
- **Project type**: To be determined by analysis
- **Git status**: Clean (main branch)
