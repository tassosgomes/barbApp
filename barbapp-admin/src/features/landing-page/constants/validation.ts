/**
 * Validation Constants and Rules
 * 
 * Este arquivo contém todas as regras de validação para o módulo de Landing Page.
 * Inclui limites de caracteres, padrões de validação e configurações de upload.
 * 
 * @see PRD: Seção 3 - Personalização de Informações da Barbearia
 * @version 1.0
 * @date 2025-10-21
 */

import { ValidationRules, LogoUploadConfig, LandingPageConstants } from '../types/landing-page.types';

// ============================================================================
// Character Limits
// ============================================================================

/**
 * Limites de caracteres para campos de texto
 */
export const CHARACTER_LIMITS = {
  /** Limite para o nome da barbearia */
  BARBERSHOP_NAME: 50,
  /** Limite para texto "Sobre a Barbearia" */
  ABOUT_TEXT: 1000,
  /** Limite para horário de funcionamento */
  OPENING_HOURS: 500,
  /** Limite para endereço completo */
  ADDRESS: 200,
  /** Limite para URLs de redes sociais */
  SOCIAL_URL: 255,
  /** Comprimento exato do WhatsApp */
  WHATSAPP_LENGTH: 14, // +5511999999999
} as const;

// ============================================================================
// Validation Patterns
// ============================================================================

/**
 * Padrões regex para validação
 */
export const VALIDATION_PATTERNS = {
  /** Padrão para número de WhatsApp brasileiro */
  WHATSAPP: /^\+55\d{2}\d{8,9}$/,
  
  /** Padrão para URLs gerais */
  URL: /^https?:\/\/(www\.)?[-a-zA-Z0-9@:%._+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_+.~#?&//=]*)$/,
  
  /** Padrão para Instagram (URL ou @usuario) */
  INSTAGRAM: /^(?:https?:\/\/)?(?:www\.)?instagram\.com\/[a-zA-Z0-9_.]+\/?$|^@[a-zA-Z0-9_.]+$/,
  
  /** Padrão para Facebook (URL) */
  FACEBOOK: /^(?:https?:\/\/)?(?:www\.)?facebook\.com\/[a-zA-Z0-9.]+\/?$/,
  
  /** Padrão para caracteres permitidos em texto sobre */
  SAFE_TEXT: /^[a-zA-Z0-9\s.,!?-()àáâãäåæçèéêëìíîïñòóôõöøùúûüýÿĀāĂăĄąĆćĈĉĊċČčĎďĐđĒēĔĕĖėĘęĚěĜĝĞğĠġĢģĤĥĦħĨĩĪīĬĭĮįİıĲĳĴĵĶķĸĹĺĻļĽľĿŀŁłŃńŅņŇňŉŊŋŌōŎŏŐőŒœŔŕŖŗŘřŚśŜŝŞşŠšŢţŤťŦŧŨũŪūŬŭŮůŰűŲųŴŵŶŷŸŹźŻżŽž]*$/,
  
  /** Padrão para nome de barbearia */
  BARBERSHOP_NAME: /^[a-zA-Z0-9\s-.&àáâãäåæçèéêëìíîïñòóôõöøùúûüýÿ]{2,50}$/,
} as const;

// ============================================================================
// Logo Upload Configuration
// ============================================================================

/**
 * Configurações para upload de logo
 */
export const LOGO_UPLOAD_CONFIG: LogoUploadConfig = {
  /** Tamanho máximo: 2MB */
  maxSize: 2 * 1024 * 1024,
  
  /** Tipos de arquivo aceitos */
  allowedTypes: [
    'image/jpeg',
    'image/jpg', 
    'image/png',
    'image/svg+xml',
    'image/webp', // Adicional para melhor performance
  ],
  
  /** Dimensões recomendadas */
  recommendedSize: {
    width: 300,
    height: 300,
  },
};

/**
 * Extensões de arquivo aceitas para upload
 */
export const ALLOWED_FILE_EXTENSIONS = [
  '.jpg',
  '.jpeg',
  '.png',
  '.svg',
  '.webp',
] as const;

/**
 * Tamanhos de imagem suportados
 */
export const IMAGE_SIZE_LIMITS = {
  /** Tamanho mínimo */
  MIN_SIZE: {
    width: 100,
    height: 100,
  },
  /** Tamanho máximo */
  MAX_SIZE: {
    width: 2000,
    height: 2000,
  },
  /** Tamanho recomendado */
  RECOMMENDED: {
    width: 300,
    height: 300,
  },
} as const;

// ============================================================================
// Service Validation
// ============================================================================

/**
 * Regras de validação para serviços
 */
export const SERVICE_VALIDATION = {
  /** Mínimo de serviços visíveis */
  MIN_VISIBLE_SERVICES: 1,
  /** Máximo de serviços por landing page */
  MAX_SERVICES: 20,
  /** Duração mínima de serviço (minutos) */
  MIN_DURATION: 5,
  /** Duração máxima de serviço (minutos) */
  MAX_DURATION: 480, // 8 horas
  /** Preço mínimo */
  MIN_PRICE: 0.01,
  /** Preço máximo */
  MAX_PRICE: 9999.99,
} as const;

// ============================================================================
// Form Validation Rules
// ============================================================================

/**
 * Regras de validação completas
 */
export const VALIDATION_RULES: ValidationRules = {
  /** Tamanho máximo do texto sobre */
  aboutTextMaxLength: CHARACTER_LIMITS.ABOUT_TEXT,
  
  /** Tamanho máximo do horário */
  openingHoursMaxLength: CHARACTER_LIMITS.OPENING_HOURS,
  
  /** Padrão do WhatsApp */
  whatsappPattern: VALIDATION_PATTERNS.WHATSAPP,
  
  /** Padrão de URL */
  urlPattern: VALIDATION_PATTERNS.URL,
  
  /** Mínimo de serviços visíveis */
  minVisibleServices: SERVICE_VALIDATION.MIN_VISIBLE_SERVICES,
};

// ============================================================================
// Error Messages
// ============================================================================

/**
 * Mensagens de erro padronizadas
 */
export const ERROR_MESSAGES = {
  /** Erros de tamanho */
  SIZE: {
    ABOUT_TEXT_TOO_LONG: `Texto muito longo. Máximo de ${CHARACTER_LIMITS.ABOUT_TEXT} caracteres.`,
    OPENING_HOURS_TOO_LONG: `Horário muito longo. Máximo de ${CHARACTER_LIMITS.OPENING_HOURS} caracteres.`,
    BARBERSHOP_NAME_TOO_LONG: `Nome muito longo. Máximo de ${CHARACTER_LIMITS.BARBERSHOP_NAME} caracteres.`,
    ADDRESS_TOO_LONG: `Endereço muito longo. Máximo de ${CHARACTER_LIMITS.ADDRESS} caracteres.`,
  },
  
  /** Erros de formato */
  FORMAT: {
    INVALID_WHATSAPP: 'Número de WhatsApp inválido. Use o formato: +5511999999999',
    INVALID_URL: 'URL inválida. Use o formato: https://exemplo.com',
    INVALID_INSTAGRAM: 'Instagram inválido. Use: @usuario ou https://instagram.com/usuario',
    INVALID_FACEBOOK: 'Facebook inválido. Use: https://facebook.com/pagina',
    INVALID_CHARACTERS: 'Caracteres inválidos detectados.',
  },
  
  /** Erros de arquivo */
  FILE: {
    FILE_TOO_LARGE: `Arquivo muito grande. Tamanho máximo: ${(LOGO_UPLOAD_CONFIG.maxSize / 1024 / 1024).toFixed(1)}MB`,
    INVALID_FILE_TYPE: `Tipo de arquivo inválido. Use: ${ALLOWED_FILE_EXTENSIONS.join(', ')}`,
    UPLOAD_FAILED: 'Falha no upload. Tente novamente.',
    IMAGE_TOO_SMALL: `Imagem muito pequena. Tamanho mínimo: ${IMAGE_SIZE_LIMITS.MIN_SIZE.width}x${IMAGE_SIZE_LIMITS.MIN_SIZE.height}px`,
    IMAGE_TOO_LARGE: `Imagem muito grande. Tamanho máximo: ${IMAGE_SIZE_LIMITS.MAX_SIZE.width}x${IMAGE_SIZE_LIMITS.MAX_SIZE.height}px`,
  },
  
  /** Erros de serviços */
  SERVICES: {
    NO_VISIBLE_SERVICES: 'Selecione pelo menos um serviço para exibir na landing page.',
    TOO_MANY_SERVICES: `Máximo de ${SERVICE_VALIDATION.MAX_SERVICES} serviços permitidos.`,
    INVALID_DURATION: `Duração deve estar entre ${SERVICE_VALIDATION.MIN_DURATION} e ${SERVICE_VALIDATION.MAX_DURATION} minutos.`,
    INVALID_PRICE: `Preço deve estar entre R$ ${SERVICE_VALIDATION.MIN_PRICE.toFixed(2)} e R$ ${SERVICE_VALIDATION.MAX_PRICE.toFixed(2)}.`,
  },
  
  /** Erros gerais */
  GENERAL: {
    REQUIRED_FIELD: 'Este campo é obrigatório.',
    NETWORK_ERROR: 'Erro de conexão. Verifique sua internet.',
    SERVER_ERROR: 'Erro no servidor. Tente novamente mais tarde.',
    INVALID_DATA: 'Dados inválidos.',
  },
} as const;

// ============================================================================
// Validation Functions
// ============================================================================

/**
 * Valida número de WhatsApp
 */
export const validateWhatsApp = (phone: string): boolean => {
  return VALIDATION_PATTERNS.WHATSAPP.test(phone);
};

/**
 * Valida URL geral
 */
export const validateUrl = (url: string): boolean => {
  if (!url) return true; // URLs são opcionais
  return VALIDATION_PATTERNS.URL.test(url);
};

/**
 * Valida Instagram
 */
export const validateInstagram = (instagram: string): boolean => {
  if (!instagram) return true; // Instagram é opcional
  return VALIDATION_PATTERNS.INSTAGRAM.test(instagram);
};

/**
 * Valida Facebook
 */
export const validateFacebook = (facebook: string): boolean => {
  if (!facebook) return true; // Facebook é opcional
  return VALIDATION_PATTERNS.FACEBOOK.test(facebook);
};

/**
 * Valida tamanho de texto
 */
export const validateTextLength = (text: string, maxLength: number): boolean => {
  return text.length <= maxLength;
};

/**
 * Valida tipo de arquivo
 */
export const validateFileType = (file: File): boolean => {
  return LOGO_UPLOAD_CONFIG.allowedTypes.includes(file.type);
};

/**
 * Valida tamanho de arquivo
 */
export const validateFileSize = (file: File): boolean => {
  return file.size <= LOGO_UPLOAD_CONFIG.maxSize;
};

/**
 * Normaliza URL do Instagram
 */
export const normalizeInstagramUrl = (input: string): string => {
  if (!input) return '';
  
  // Se começar com @, converter para URL
  if (input.startsWith('@')) {
    const username = input.slice(1);
    return `https://instagram.com/${username}`;
  }
  
  // Se não tem protocolo, adicionar https
  if (input.includes('instagram.com') && !input.startsWith('http')) {
    return `https://${input}`;
  }
  
  return input;
};

/**
 * Normaliza URL do Facebook
 */
export const normalizeFacebookUrl = (input: string): string => {
  if (!input) return '';
  
  // Se não tem protocolo, adicionar https
  if (input.includes('facebook.com') && !input.startsWith('http')) {
    return `https://${input}`;
  }
  
  return input;
};

/**
 * Formata número de WhatsApp
 */
export const formatWhatsApp = (phone: string): string => {
  // Remove todos os caracteres não numéricos
  const numbers = phone.replace(/\D/g, '');
  
  // Se já tem código do país, mantém
  if (numbers.startsWith('55')) {
    return `+${numbers}`;
  }
  
  // Se não tem, adiciona +55
  return `+55${numbers}`;
};

// ============================================================================
// Constants Export
// ============================================================================

/**
 * Todas as constantes do módulo Landing Page
 */
export const LANDING_PAGE_CONSTANTS: LandingPageConstants = {
  limits: {
    aboutText: CHARACTER_LIMITS.ABOUT_TEXT,
    openingHours: CHARACTER_LIMITS.OPENING_HOURS,
    whatsappLength: CHARACTER_LIMITS.WHATSAPP_LENGTH,
  },
  upload: LOGO_UPLOAD_CONFIG,
  defaultUrls: {
    placeholder: '/images/logo-placeholder.png',
    previewImages: {
      1: '/templates/classic-preview.png',
      2: '/templates/modern-preview.png',
      3: '/templates/vintage-preview.png',
      4: '/templates/urban-preview.png',
      5: '/templates/premium-preview.png',
    },
  },
  validation: VALIDATION_RULES,
};

// ============================================================================
// Default Values
// ============================================================================

/**
 * Valores padrão para novos formulários
 */
export const DEFAULT_VALUES = {
  /** Horário padrão */
  OPENING_HOURS: 'Segunda a Sábado: 09:00 - 19:00\nDomingo: Fechado',
  
  /** Template padrão */
  TEMPLATE_ID: 1 as const,
  
  /** Status padrão */
  IS_PUBLISHED: true,
  
  /** Texto placeholder para "sobre" */
  ABOUT_PLACEHOLDER: 'Conte um pouco sobre sua barbearia, história, diferenciais e o que torna seu estabelecimento especial...',
  
  /** Placeholder para horário */
  HOURS_PLACEHOLDER: 'Ex: Segunda a Sexta: 09:00 - 19:00\nSábado: 09:00 - 17:00\nDomingo: Fechado',
} as const;

// ============================================================================
// Note: All constants are already exported above in their respective sections
// ============================================================================