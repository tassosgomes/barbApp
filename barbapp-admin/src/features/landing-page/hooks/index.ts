/**
 * Landing Page Hooks Module
 * 
 * Centraliza todas as exportações dos hooks customizados
 * para gerenciamento de landing page.
 * 
 * @version 1.0
 * @date 2025-10-21
 */

// ============================================================================
// Main Hooks
// ============================================================================

export {
  useLandingPage,
  usePublicLandingPage,
  useDuplicateTemplate,
  LANDING_PAGE_QUERY_KEYS,
  type UseLandingPageResult,
} from './useLandingPage';

export {
  useLogoUpload,
  useLogoDropzone,
  validateImageDimensions,
  resizeImage,
  UPLOAD_CONFIG,
  type UseLogoUploadResult,
  type UseLogoUploadOptions,
} from './useLogoUpload';

// ============================================================================
// Re-exports for Convenience
// ============================================================================

/**
 * Hook principal para gerenciamento de landing page
 */
export { useLandingPage as default } from './useLandingPage';

// ============================================================================
// Hook Utilities
// ============================================================================

/**
 * Utilitários dos hooks exportados para uso direto
 */
export const LandingPageHooks = {
  // Query keys para cache management
  QUERY_KEYS: {
    LANDING_PAGE: ['landingPage'],
    CONFIG: (id: string) => ['landingPage', 'config', id],
    PUBLIC: (code: string) => ['landingPage', 'public', code],
  },
  
  // Upload configuration
  UPLOAD: {
    MAX_SIZE: 2 * 1024 * 1024, // 2MB
    ALLOWED_TYPES: ['image/jpeg', 'image/png', 'image/svg+xml'],
    RECOMMENDED_SIZE: '300x300px',
  },
} as const;