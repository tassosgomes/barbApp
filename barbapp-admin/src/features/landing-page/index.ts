/**
 * Landing Page Feature Module
 * 
 * Este arquivo centraliza todas as exportações do módulo de Landing Page.
 * Organiza types, constants, utils e facilita a importação em outros módulos.
 * 
 * @version 1.0
 * @date 2025-10-21
 */

// ============================================================================
// Types and Interfaces
// ============================================================================

export type {
  // Core Types
  LandingPageConfig,
  LandingPageService,
  Template,
  TemplateColors,
  TemplateConfig,
  
  // Request/Response Types
  CreateLandingPageInput,
  UpdateLandingPageInput,
  LandingPageConfigOutput,
  PublicLandingPageOutput,
  PublicService,
  
  // Form Types
  LandingPageFormData,
  ServiceFormState,
  
  // Upload Types
  LogoUploadState,
  LogoUploadConfig,
  
  // Validation Types
  ValidationErrors,
  ValidationRules,
  
  // Hook Types
  UseLandingPageResult,
  UseTemplatesResult,
  UseServiceSelectionResult,
  
  // Component Props Types
  TemplateGalleryProps,
  ServiceManagerProps,
  LogoUploaderProps,
  PreviewPanelProps,
  
  // API Types
  ApiResponse,
  LogoUploadResponse,
  
  // Utility Types
  TemplateId,
  TemplateTheme,
  LandingPageStatus,
  PreviewDevice,
  ServiceAction,
  
  // Constants Types
  LandingPageConstants,
  
  // Event Types
  LandingPageEvents,
  
  // State Types
  LandingPageState,
} from './types/landing-page.types';

// ============================================================================
// Constants
// ============================================================================

// Templates
export {
  TEMPLATES,
  TEMPLATES_MAP,
  TEMPLATE_IDS,
  DEFAULT_TEMPLATE_ID,
  DEFAULT_TEMPLATE,
  TEMPLATE_CATEGORIES,
  TEMPLATE_COLOR_THEMES,
  TEMPLATE_METADATA,
  TEMPLATE_PREVIEW_URLS,
  TEMPLATE_PLACEHOLDER_URL,
  
  // Template utilities
  getTemplateById,
  getTemplateByTheme,
  isValidTemplateId,
  getTemplatesByCategory,
  getTemplatePrimaryColor,
  getTemplateColors,
  getTemplateConfig,
} from './constants/templates';

// Validation
export {
  CHARACTER_LIMITS,
  VALIDATION_PATTERNS,
  LOGO_UPLOAD_CONFIG,
  ALLOWED_FILE_EXTENSIONS,
  IMAGE_SIZE_LIMITS,
  SERVICE_VALIDATION,
  VALIDATION_RULES,
  ERROR_MESSAGES,
  DEFAULT_VALUES,
  LANDING_PAGE_CONSTANTS,
  
  // Validation utilities
  validateWhatsApp,
  validateUrl,
  validateInstagram,
  validateFacebook,
  validateTextLength,
  validateFileType,
  validateFileSize,
  normalizeInstagramUrl,
  normalizeFacebookUrl,
  formatWhatsApp,
} from './constants/validation';

// ============================================================================
// Re-exports for Convenience
// ============================================================================

/**
 * Principais constantes para uso rápido
 */
export const LANDING_PAGE_CONFIG = {
  DEFAULT_TEMPLATE_ID: 1,
  MAX_ABOUT_TEXT_LENGTH: 1000,
  MAX_OPENING_HOURS_LENGTH: 500,
  MAX_LOGO_SIZE: 2 * 1024 * 1024, // 2MB
  MIN_VISIBLE_SERVICES: 1,
} as const;

/**
 * Mensagens de erro mais comuns
 */
export const COMMON_ERRORS = {
  ABOUT_TOO_LONG: 'Texto muito longo. Máximo de 1000 caracteres.',
  HOURS_TOO_LONG: 'Horário muito longo. Máximo de 500 caracteres.',
  INVALID_WHATSAPP: 'Número de WhatsApp inválido. Use: +5511999999999',
  INVALID_URL: 'URL inválida. Use: https://exemplo.com',
  FILE_TOO_LARGE: 'Arquivo muito grande. Máximo: 2MB',
  INVALID_FILE: 'Tipo inválido. Use: JPG, PNG ou SVG',
  NO_SERVICES: 'Selecione pelo menos um serviço.',
  REQUIRED: 'Este campo é obrigatório.',
} as const;

/**
 * Padrões de validação mais usados
 */
export const COMMON_PATTERNS = {
  WHATSAPP: /^\+55\d{2}\d{8,9}$/,
  URL: /^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_+.~#?&//=]*)$/,
  INSTAGRAM: /^(?:https?:\/\/)?(?:www\.)?instagram\.com\/[a-zA-Z0-9_.]+\/?$|^@[a-zA-Z0-9_.]+$/,
  FACEBOOK: /^(?:https?:\/\/)?(?:www\.)?facebook\.com\/[a-zA-Z0-9.]+\/?$/,
} as const;

// ============================================================================
// Default Export
// ============================================================================

/**
 * Export padrão com as principais funcionalidades
 */
const LandingPageModule = {
  // Quick access configs
  CONFIG: LANDING_PAGE_CONFIG,
  ERRORS: COMMON_ERRORS,
  PATTERNS: COMMON_PATTERNS,
} as const;

export default LandingPageModule;

// ============================================================================
// Module Information
// ============================================================================

/**
 * Informações do módulo
 */
export const MODULE_INFO = {
  name: 'Landing Page',
  version: '1.0.0',
  description: 'Módulo de Landing Page personalizável para barbearias',
  author: 'BarbApp Team',
  lastUpdated: '2025-10-21',
  features: [
    'Gerenciamento de templates',
    'Upload de logo',
    'Personalização de informações',
    'Gerenciamento de serviços',
    'Validação de formulários',
    'Preview em tempo real',
  ],
} as const;