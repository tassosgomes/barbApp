/**
 * Landing Page Types and Interfaces
 * 
 * Este arquivo contém todas as interfaces e tipos necessários para o módulo de Landing Page.
 * Baseado no PRD de Landing Page Personalizável para Barbearias.
 * 
 * @see PRD: Landing Page Personalizável para Barbearias
 * @version 1.0
 * @date 2025-10-21
 */

// ============================================================================
// Core Landing Page Configuration Types
// ============================================================================

/**
 * Interface principal para configuração da Landing Page
 */
export interface LandingPageConfig {
  /** ID único da configuração */
  id: string;
  /** ID da barbearia proprietária */
  barbershopId: string;
  /** ID do template selecionado (1-5) */
  templateId: number;
  /** URL do logo carregado */
  logoUrl?: string;
  /** Texto sobre a barbearia (máx. 1000 chars) */
  aboutText?: string;
  /** Horário de funcionamento (máx. 500 chars) */
  openingHours?: string;
  /** URL do Instagram */
  instagramUrl?: string;
  /** URL do Facebook */
  facebookUrl?: string;
  /** Número do WhatsApp no formato +5511999999999 */
  whatsappNumber: string;
  /** Se a landing page está publicada */
  isPublished: boolean;
  /** Lista de serviços configurados */
  services: LandingPageService[];
  /** Timestamp da última atualização */
  updatedAt: string;
  /** Data de criação */
  createdAt: string;
}

/**
 * Interface para serviços exibidos na Landing Page
 */
export interface LandingPageService {
  /** ID do serviço */
  serviceId: string;
  /** Nome do serviço */
  serviceName: string;
  /** Descrição do serviço (opcional) */
  description?: string;
  /** Duração em minutos */
  duration: number;
  /** Preço do serviço */
  price: number;
  /** Ordem de exibição (1, 2, 3...) */
  displayOrder: number;
  /** Se o serviço está visível na landing page */
  isVisible: boolean;
}

// ============================================================================
// Template Types
// ============================================================================

/**
 * Interface para definição de template
 */
export interface Template {
  /** ID único do template (1-5) */
  id: number;
  /** Nome do template */
  name: string;
  /** Tema do template */
  theme: string;
  /** Descrição do template */
  description: string;
  /** URL da imagem de preview */
  previewImage: string;
  /** Paleta de cores do template */
  colors: TemplateColors;
  /** Configurações específicas do template */
  config?: TemplateConfig;
}

/**
 * Interface para cores do template
 */
export interface TemplateColors {
  /** Cor primária */
  primary: string;
  /** Cor secundária */
  secondary: string;
  /** Cor de destaque/accent */
  accent: string;
  /** Cor de fundo (opcional) */
  background?: string;
  /** Cor do texto (opcional) */
  text?: string;
}

/**
 * Configurações específicas do template
 */
export interface TemplateConfig {
  /** Fonte primária */
  primaryFont?: string;
  /** Fonte secundária */
  secondaryFont?: string;
  /** Layout do header */
  headerLayout?: 'center' | 'left' | 'split';
  /** Estilo dos cards de serviço */
  serviceCardStyle?: 'classic' | 'modern' | 'minimal';
  /** Animações habilitadas */
  animations?: boolean;
}

// ============================================================================
// Request/Response Types
// ============================================================================

/**
 * Payload para criação de Landing Page
 */
export interface CreateLandingPageInput {
  /** ID da barbearia */
  barbershopId: string;
  /** ID do template inicial */
  templateId?: number;
  /** URL do logo */
  logoUrl?: string;
  /** Texto sobre a barbearia */
  aboutText?: string;
  /** Horário de funcionamento */
  openingHours?: string;
  /** URL do Instagram */
  instagramUrl?: string;
  /** URL do Facebook */
  facebookUrl?: string;
  /** Número do WhatsApp */
  whatsappNumber: string;
  /** Serviços iniciais */
  services?: Array<{
    serviceId: string;
    displayOrder: number;
    isVisible: boolean;
  }>;
}

/**
 * Payload para atualização de Landing Page
 */
export interface UpdateLandingPageInput {
  /** ID do template */
  templateId?: number;
  /** Texto sobre a barbearia */
  aboutText?: string;
  /** Horário de funcionamento */
  openingHours?: string;
  /** URL do Instagram */
  instagramUrl?: string;
  /** URL do Facebook */
  facebookUrl?: string;
  /** Número do WhatsApp */
  whatsappNumber?: string;
  /** Configuração dos serviços */
  services?: Array<{
    serviceId: string;
    displayOrder: number;
    isVisible: boolean;
  }>;
}

/**
 * Response da API para Landing Page
 */
export interface LandingPageConfigOutput {
  /** Configuração da landing page */
  landingPage: LandingPageConfig;
  /** Informações da barbearia */
  barbershop: {
    id: string;
    name: string;
    code: string;
    address: string;
  };
  /** URL pública da landing page */
  publicUrl: string;
}

// ============================================================================
// Public Landing Page Types (Para página pública)
// ============================================================================

/**
 * Interface para dados públicos da Landing Page
 */
export interface PublicLandingPageOutput {
  /** Informações da barbearia */
  barbershop: {
    id: string;
    name: string;
    code: string;
    address: string;
  };
  /** Configuração da landing page */
  landingPage: {
    templateId: number;
    logoUrl?: string;
    aboutText?: string;
    openingHours?: string;
    instagramUrl?: string;
    facebookUrl?: string;
    whatsappNumber: string;
    services: PublicService[];
  };
}

/**
 * Interface para serviços na página pública
 */
export interface PublicService {
  /** ID do serviço */
  id: string;
  /** Nome do serviço */
  name: string;
  /** Descrição do serviço */
  description?: string;
  /** Duração em minutos */
  duration: number;
  /** Preço do serviço */
  price: number;
}

// ============================================================================
// Form Types
// ============================================================================

/**
 * Dados do formulário de informações da Landing Page
 */
export interface LandingPageFormData {
  /** Texto sobre a barbearia */
  aboutText?: string;
  /** Horário de funcionamento */
  openingHours?: string;
  /** URL do Instagram */
  instagramUrl?: string;
  /** URL do Facebook */
  facebookUrl?: string;
  /** Número do WhatsApp */
  whatsappNumber: string;
}

/**
 * Estado do formulário de serviços
 */
export interface ServiceFormState {
  /** Lista de serviços */
  services: LandingPageService[];
  /** Serviços selecionados */
  selectedServices: Set<string>;
  /** Estado de carregamento */
  isLoading: boolean;
  /** Mensagem de erro */
  error?: string;
}

// ============================================================================
// Upload Types
// ============================================================================

/**
 * Estado do upload de logo
 */
export interface LogoUploadState {
  /** Status do upload */
  status: 'idle' | 'selected' | 'uploading' | 'success' | 'error';
  /** Progresso do upload (0-100) */
  progress: number;
  /** URL final após upload bem-sucedido */
  url: string | null;
  /** Erro do upload */
  error: string | null;
}

/**
 * Configurações de upload de logo
 */
export interface LogoUploadConfig {
  /** Tamanho máximo em bytes (padrão: 2MB) */
  maxSize: number;
  /** Tipos aceitos */
  allowedTypes: string[];
  /** Dimensões recomendadas */
  recommendedSize: {
    width: number;
    height: number;
  };
}

// ============================================================================
// Validation Types
// ============================================================================

/**
 * Erros de validação do formulário
 */
export interface ValidationErrors {
  /** Erro no texto sobre */
  aboutText?: string;
  /** Erro no horário */
  openingHours?: string;
  /** Erro no Instagram */
  instagramUrl?: string;
  /** Erro no Facebook */
  facebookUrl?: string;
  /** Erro no WhatsApp */
  whatsappNumber?: string;
  /** Erro nos serviços */
  services?: string;
}

/**
 * Regras de validação
 */
export interface ValidationRules {
  /** Tamanho máximo do texto sobre */
  aboutTextMaxLength: number;
  /** Tamanho máximo do horário */
  openingHoursMaxLength: number;
  /** Padrão do WhatsApp */
  whatsappPattern: RegExp;
  /** Padrão de URL */
  urlPattern: RegExp;
  /** Mínimo de serviços visíveis */
  minVisibleServices: number;
}

// ============================================================================
// Hook Types
// ============================================================================

/**
 * Resultado do hook useLandingPage
 */
export interface UseLandingPageResult {
  /** Configuração da landing page */
  config?: LandingPageConfig;
  /** Estado de carregamento */
  isLoading: boolean;
  /** Estado de criação */
  isCreating: boolean;
  /** Estado de atualização */
  isUpdating: boolean;
  /** Estado de publicação */
  isPublishing: boolean;
  /** Estado de despublicação */
  isUnpublishing: boolean;
  /** Estado de upload de logo */
  isUploadingLogo: boolean;
  /** Estado de deleção de logo */
  isDeletingLogo: boolean;
  /** Estado de geração de preview */
  isGeneratingPreview: boolean;
  /** Erro geral */
  error: unknown;
  /** Erro de criação */
  createError: unknown;
  /** Erro de atualização */
  updateError: unknown;
  /** Erro de publicação */
  publishError: unknown;
  /** Erro de despublicação */
  unpublishError: unknown;
  /** Erro de upload de logo */
  uploadLogoError: unknown;
  /** Erro de deleção de logo */
  deleteLogoError: unknown;
  /** Erro de preview */
  previewError: unknown;
  /** Função para criar configuração */
  createConfig: (data: CreateLandingPageInput) => void;
  /** Função para atualizar configuração */
  updateConfig: (data: UpdateLandingPageInput) => void;
  /** Função para publicar landing page */
  publishConfig: () => void;
  /** Função para despublicar landing page */
  unpublishConfig: () => void;
  /** Função para fazer upload de logo */
  uploadLogo: (file: File) => void;
  /** Função para deletar logo */
  deleteLogo: () => void;
  /** Função para gerar preview */
  generatePreview: () => void;
  /** Função para refazer query */
  refetch: () => void;
  /** Função para invalidar queries */
  invalidateQueries: () => void;
}

/**
 * Resultado do hook useTemplates
 */
export interface UseTemplatesResult {
  /** Lista de templates */
  templates: Template[];
  /** Estado de carregamento */
  isLoading: boolean;
  /** Erro */
  error: unknown;
  /** Função para refazer query */
  refetch: () => void;
  /** Função para buscar template por ID */
  getTemplateById: (id: number) => Template | undefined;
  /** Função para buscar templates por tema */
  getTemplatesByTheme: (theme: string) => Template[];
  /** Função para obter temas disponíveis */
  getAvailableThemes: () => string[];
}

/**
 * Resultado do hook useLogoUpload
 */
export interface UseLogoUploadResult {
  /** Se está fazendo upload */
  isUploading: boolean;
  /** Se está deletando logo */
  isDeleting: boolean;
  /** Erro de upload */
  uploadError: unknown;
  /** Erro de deleção */
  deleteError: unknown;
  /** Erro de validação */
  validationError: LogoUploadValidationError | null;
  /** URL de preview do logo */
  previewUrl: string | null;
  /** Função para fazer upload de logo */
  uploadLogo: (file: File) => void;
  /** Função para deletar logo */
  deleteLogo: () => void;
  /** Função para criar preview */
  createPreview: (file: File) => void;
  /** Função para limpar preview */
  clearPreview: () => void;
  /** Função para validar arquivo */
  validateFile: (file: File) => LogoUploadValidationError | null;
}

/**
 * Erro de validação de upload de logo
 */
export interface LogoUploadValidationError {
  /** Tipo de erro */
  type: 'size' | 'type' | 'network';
  /** Mensagem de erro */
  message: string;
}

/**
 * Resultado do hook useServiceSelection
 */
export interface UseServiceSelectionResult {
  /** IDs dos serviços selecionados */
  selectedIds: Set<string>;
  /** Serviços selecionados */
  selectedServices: PublicService[];
  /** Preço total */
  totalPrice: number;
  /** Duração total */
  totalDuration: number;
  /** Função para alternar seleção */
  toggleService: (serviceId: string) => void;
  /** Se tem alguma seleção */
  hasSelection: boolean;
  /** Limpar seleção */
  clearSelection: () => void;
}

// ============================================================================
// Component Props Types
// ============================================================================

/**
 * Props do componente TemplateGallery
 */
export interface TemplateGalleryProps {
  /** ID do template selecionado */
  selectedTemplateId: number;
  /** Callback de seleção de template */
  onSelectTemplate: (templateId: number) => void;
  /** Se está carregando */
  loading?: boolean;
}

/**
 * Props do componente ServiceManager
 */
export interface ServiceManagerProps {
  /** Lista de serviços */
  services: LandingPageService[];
  /** Callback de mudança */
  onChange: (services: LandingPageService[]) => void;
  /** Se está desabilitado */
  disabled?: boolean;
}

/**
 * Props do componente LogoUploader
 */
export interface LogoUploaderProps {
  /** ID da barbearia */
  barbershopId: string;
  /** URL atual do logo */
  currentLogoUrl?: string;
  /** Callback após upload */
  onUploadComplete?: (logoUrl: string) => void;
  /** Se está desabilitado */
  disabled?: boolean;
}

/**
 * Props do componente PreviewPanel
 */
export interface PreviewPanelProps {
  /** Configuração da landing page */
  config?: LandingPageConfig;
  /** Se está em modo tela cheia */
  fullScreen?: boolean;
  /** Dispositivo para preview */
  device?: 'mobile' | 'tablet' | 'desktop';
}

// ============================================================================
// API Response Types
// ============================================================================

/**
 * Response padrão da API
 */
export interface ApiResponse<T = unknown> {
  /** Dados da resposta */
  data?: T;
  /** Mensagem */
  message?: string;
  /** Status de sucesso */
  success: boolean;
  /** Código de erro */
  errorCode?: string;
}

/**
 * Response de upload de logo
 */
export interface LogoUploadResponse {
  /** URL do logo carregado */
  logoUrl: string;
  /** Mensagem de sucesso */
  message: string;
}

// ============================================================================
// Utility Types
// ============================================================================

/**
 * Tipo para template ID válido
 */
export type TemplateId = 1 | 2 | 3 | 4 | 5;

/**
 * Tipo para tema do template
 */
export type TemplateTheme = 'classic' | 'modern' | 'vintage' | 'urban' | 'premium';

/**
 * Tipo para status da landing page
 */
export type LandingPageStatus = 'draft' | 'published' | 'archived';

/**
 * Tipo para dispositivo de preview
 */
export type PreviewDevice = 'mobile' | 'tablet' | 'desktop';

/**
 * Tipo para ação de serviço
 */
export type ServiceAction = 'select_all' | 'deselect_all' | 'toggle' | 'reorder';

// ============================================================================
// Constants Types
// ============================================================================

/**
 * Configurações constantes do módulo
 */
export interface LandingPageConstants {
  /** Limites de caracteres */
  limits: {
    aboutText: number;
    openingHours: number;
    whatsappLength: number;
  };
  /** Configurações de upload */
  upload: LogoUploadConfig;
  /** URLs padrão */
  defaultUrls: {
    placeholder: string;
    previewImages: Record<TemplateId, string>;
  };
  /** Validação */
  validation: ValidationRules;
}

// ============================================================================
// Event Types
// ============================================================================

/**
 * Eventos do componente Landing Page
 */
export interface LandingPageEvents {
  /** Evento de mudança de template */
  onTemplateChange: (templateId: number) => void;
  /** Evento de upload de logo */
  onLogoUpload: (file: File) => void;
  /** Evento de atualização de informações */
  onInfoUpdate: (data: LandingPageFormData) => void;
  /** Evento de mudança de serviços */
  onServicesChange: (services: LandingPageService[]) => void;
  /** Evento de publicação */
  onPublish: () => void;
}

// ============================================================================
// State Types
// ============================================================================

/**
 * Estado global do módulo Landing Page
 */
export interface LandingPageState {
  /** Configuração atual */
  config?: LandingPageConfig;
  /** Template selecionado */
  selectedTemplate: Template;
  /** Estado de carregamento */
  loading: boolean;
  /** Estado de salvamento */
  saving: boolean;
  /** Erros de validação */
  errors: ValidationErrors;
  /** Preview habilitado */
  previewEnabled: boolean;
  /** Dispositivo de preview */
  previewDevice: PreviewDevice;
}