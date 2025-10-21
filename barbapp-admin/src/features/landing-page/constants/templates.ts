/**
 * Templates Constants
 * 
 * Este arquivo contém os 5 templates mockados conforme especificação do PRD.
 * Cada template tem características visuais únicas e paleta de cores específica.
 * 
 * @see PRD: Seção 2.2 - Templates disponíveis
 * @version 1.0
 * @date 2025-10-21
 */

import { Template, TemplateId } from '../types/landing-page.types';

// ============================================================================
// Template Definitions
// ============================================================================

/**
 * Template 1 - Clássico
 * Tema: Elegante e tradicional
 * Ideal para: Barbearias com conceito premium e tradicional
 */
const TEMPLATE_CLASSIC: Template = {
  id: 1,
  name: 'Clássico',
  theme: 'classic',
  description: 'Elegante e tradicional, ideal para barbearias com conceito premium e atmosfera clássica.',
  previewImage: '/templates/classic-preview.png',
  colors: {
    primary: '#1A1A1A', // Preto elegante
    secondary: '#D4AF37', // Dourado clássico
    accent: '#FFFFFF', // Branco puro
    background: '#F8F8F8', // Cinza muito claro
    text: '#2C2C2C', // Cinza escuro para texto
  },
  config: {
    primaryFont: 'Playfair Display', // Serif clássica
    secondaryFont: 'Inter', // Sans-serif moderna
    headerLayout: 'center',
    serviceCardStyle: 'classic',
    animations: true,
  },
};

/**
 * Template 2 - Moderno
 * Tema: Limpo e minimalista
 * Ideal para: Barbearias com visual contemporâneo
 */
const TEMPLATE_MODERN: Template = {
  id: 2,
  name: 'Moderno',
  theme: 'modern',
  description: 'Limpo e minimalista, perfeito para um visual contemporâneo e profissional.',
  previewImage: '/templates/modern-preview.png',
  colors: {
    primary: '#2C3E50', // Cinza escuro moderno
    secondary: '#3498DB', // Azul elétrico
    accent: '#ECF0F1', // Branco/Cinza claro
    background: '#FFFFFF', // Branco puro
    text: '#34495E', // Cinza médio
  },
  config: {
    primaryFont: 'Montserrat', // Sans-serif moderna
    secondaryFont: 'Open Sans', // Sans-serif legível
    headerLayout: 'left',
    serviceCardStyle: 'modern',
    animations: true,
  },
};

/**
 * Template 3 - Vintage
 * Tema: Retrô anos 50/60
 * Ideal para: Barbearias com conceito clássico e nostálgico
 */
const TEMPLATE_VINTAGE: Template = {
  id: 3,
  name: 'Vintage',
  theme: 'vintage',
  description: 'Estilo retrô anos 50/60, para barbearias com conceito clássico e atmosfera nostálgica.',
  previewImage: '/templates/vintage-preview.png',
  colors: {
    primary: '#5D4037', // Marrom vintage
    secondary: '#B71C1C', // Vermelho escuro
    accent: '#F5E6D3', // Creme vintage
    background: '#FFF8E1', // Amarelo muito claro
    text: '#3E2723', // Marrom escuro
  },
  config: {
    primaryFont: 'Fredoka One', // Display vintage
    secondaryFont: 'Roboto', // Sans-serif para corpo
    headerLayout: 'center',
    serviceCardStyle: 'classic',
    animations: false, // Mantém estilo mais estático
  },
};

/**
 * Template 4 - Urbano
 * Tema: Street/Hip-hop
 * Ideal para: Barbearias jovens e descoladas
 */
const TEMPLATE_URBAN: Template = {
  id: 4,
  name: 'Urbano',
  theme: 'urban',
  description: 'Visual street/hip-hop, ideal para barbearias jovens, descoladas e com estilo urbano.',
  previewImage: '/templates/urban-preview.png',
  colors: {
    primary: '#000000', // Preto intenso
    secondary: '#E74C3C', // Vermelho vibrante
    accent: '#95A5A6', // Cinza urbano
    background: '#1C1C1C', // Cinza muito escuro
    text: '#FFFFFF', // Branco para contraste
  },
  config: {
    primaryFont: 'Bebas Neue', // Display bold e impactante
    secondaryFont: 'Roboto', // Sans-serif para legibilidade
    headerLayout: 'split',
    serviceCardStyle: 'modern',
    animations: true,
  },
};

/**
 * Template 5 - Premium
 * Tema: Luxuoso e sofisticado
 * Ideal para: Barbearias de alto padrão
 */
const TEMPLATE_PREMIUM: Template = {
  id: 5,
  name: 'Premium',
  theme: 'premium',
  description: 'Luxuoso e sofisticado, para barbearias de alto padrão que buscam exclusividade.',
  previewImage: '/templates/premium-preview.png',
  colors: {
    primary: '#1C1C1C', // Preto profundo
    secondary: '#C9A961', // Dourado metálico
    accent: '#2E2E2E', // Cinza escuro elegante
    background: '#0F0F0F', // Preto suave
    text: '#F5F5F5', // Branco suave
  },
  config: {
    primaryFont: 'Cinzel', // Serif elegante e luxuosa
    secondaryFont: 'Lato', // Sans-serif sofisticada
    headerLayout: 'center',
    serviceCardStyle: 'minimal',
    animations: true,
  },
};

// ============================================================================
// Templates Array
// ============================================================================

/**
 * Array com todos os templates disponíveis
 * Ordem: Clássico, Moderno, Vintage, Urbano, Premium
 */
export const TEMPLATES: Template[] = [
  TEMPLATE_CLASSIC,
  TEMPLATE_MODERN,
  TEMPLATE_VINTAGE,
  TEMPLATE_URBAN,
  TEMPLATE_PREMIUM,
];

// ============================================================================
// Template Utilities
// ============================================================================

/**
 * Mapa de templates por ID para acesso rápido
 */
export const TEMPLATES_MAP: Record<TemplateId, Template> = {
  1: TEMPLATE_CLASSIC,
  2: TEMPLATE_MODERN,
  3: TEMPLATE_VINTAGE,
  4: TEMPLATE_URBAN,
  5: TEMPLATE_PREMIUM,
} as const;

/**
 * Lista de IDs de templates válidos
 */
export const TEMPLATE_IDS: TemplateId[] = [1, 2, 3, 4, 5];

/**
 * Template padrão (usado na criação automática)
 */
export const DEFAULT_TEMPLATE_ID: TemplateId = 1;

/**
 * Template padrão
 */
export const DEFAULT_TEMPLATE: Template = TEMPLATE_CLASSIC;

// ============================================================================
// Template Categories
// ============================================================================

/**
 * Categorização dos templates por estilo
 */
export const TEMPLATE_CATEGORIES = {
  traditional: [1, 3], // Clássico e Vintage
  contemporary: [2, 4], // Moderno e Urbano
  luxury: [1, 5], // Clássico e Premium
  youth: [2, 4], // Moderno e Urbano
  premium: [5], // Apenas Premium
} as const;

/**
 * Cores predominantes por template
 */
export const TEMPLATE_COLOR_THEMES = {
  1: 'black-gold', // Preto e dourado
  2: 'blue-modern', // Azul moderno
  3: 'brown-vintage', // Marrom vintage
  4: 'black-red', // Preto e vermelho
  5: 'black-gold-premium', // Preto e dourado premium
} as const;

// ============================================================================
// Template Metadata
// ============================================================================

/**
 * Metadados adicionais dos templates
 */
export const TEMPLATE_METADATA = {
  1: {
    suitableFor: ['Traditional', 'Upscale', 'Classic'],
    difficulty: 'easy',
    popularity: 'high',
    keywords: ['elegant', 'traditional', 'premium', 'classic', 'sophisticated'],
  },
  2: {
    suitableFor: ['Modern', 'Minimalist', 'Tech-savvy'],
    difficulty: 'easy',
    popularity: 'very-high',
    keywords: ['clean', 'minimal', 'contemporary', 'professional', 'simple'],
  },
  3: {
    suitableFor: ['Vintage', 'Retro', 'Classic'],
    difficulty: 'medium',
    popularity: 'medium',
    keywords: ['retro', 'vintage', 'nostalgic', 'classic', '50s', '60s'],
  },
  4: {
    suitableFor: ['Urban', 'Young', 'Trendy'],
    difficulty: 'medium',
    popularity: 'high',
    keywords: ['urban', 'street', 'hip-hop', 'trendy', 'bold', 'young'],
  },
  5: {
    suitableFor: ['Luxury', 'High-end', 'Exclusive'],
    difficulty: 'easy',
    popularity: 'medium',
    keywords: ['luxury', 'premium', 'exclusive', 'sophisticated', 'high-end'],
  },
} as const;

// ============================================================================
// Helper Functions
// ============================================================================

/**
 * Busca template por ID
 */
export const getTemplateById = (id: TemplateId): Template | undefined => {
  return TEMPLATES_MAP[id];
};

/**
 * Busca template por tema
 */
export const getTemplateByTheme = (theme: string): Template | undefined => {
  return TEMPLATES.find(template => template.theme === theme);
};

/**
 * Valida se um ID de template é válido
 */
export const isValidTemplateId = (id: number): id is TemplateId => {
  return TEMPLATE_IDS.includes(id as TemplateId);
};

/**
 * Retorna templates por categoria
 */
export const getTemplatesByCategory = (category: keyof typeof TEMPLATE_CATEGORIES): Template[] => {
  const templateIds = TEMPLATE_CATEGORIES[category];
  return templateIds.map(id => TEMPLATES_MAP[id]);
};

/**
 * Retorna cor primária de um template
 */
export const getTemplatePrimaryColor = (templateId: TemplateId): string => {
  const template = getTemplateById(templateId);
  return template?.colors.primary || '#000000';
};

/**
 * Retorna todas as cores de um template
 */
export const getTemplateColors = (templateId: TemplateId) => {
  const template = getTemplateById(templateId);
  return template?.colors || TEMPLATE_CLASSIC.colors;
};

/**
 * Retorna configuração de um template
 */
export const getTemplateConfig = (templateId: TemplateId) => {
  const template = getTemplateById(templateId);
  return template?.config || TEMPLATE_CLASSIC.config;
};

// ============================================================================
// Preview URLs (Placeholders)
// ============================================================================

/**
 * URLs de preview dos templates
 * Nota: Estas são URLs placeholder que devem ser substituídas por imagens reais
 */
export const TEMPLATE_PREVIEW_URLS: Record<TemplateId, string> = {
  1: '/templates/classic-preview.png',
  2: '/templates/modern-preview.png',
  3: '/templates/vintage-preview.png',
  4: '/templates/urban-preview.png',
  5: '/templates/premium-preview.png',
} as const;

/**
 * Placeholder genérico para templates sem preview
 */
export const TEMPLATE_PLACEHOLDER_URL = '/templates/placeholder.png';

// ============================================================================
// Export Default
// ============================================================================

export default TEMPLATES;