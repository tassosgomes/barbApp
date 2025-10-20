/**
 * Token Manager - Sistema Centralizado de Gerenciamento de Tokens
 * 
 * Este serviço garante que não haja conflito entre tokens de diferentes tipos de usuários
 * no localStorage. Cada tipo de usuário tem seu próprio token e contexto isolado.
 * 
 * Tipos de usuários:
 * - Admin Central: Gerencia todas as barbearias
 * - Admin Barbearia: Gerencia uma barbearia específica
 * - Barbeiro: Acessa sua própria agenda
 * 
 * @module TokenManager
 */

/**
 * Tipos de usuários suportados pelo sistema
 */
export enum UserType {
  ADMIN_CENTRAL = 'admin_central',
  ADMIN_BARBEARIA = 'admin_barbearia',
  BARBEIRO = 'barbeiro',
}

/**
 * Chaves do localStorage para cada tipo de usuário
 */
const TOKEN_KEYS = {
  [UserType.ADMIN_CENTRAL]: 'auth_token',
  [UserType.ADMIN_BARBEARIA]: 'admin_barbearia_token',
  [UserType.BARBEIRO]: 'barbapp-barber-token',
} as const;

const CONTEXT_KEYS = {
  [UserType.ADMIN_BARBEARIA]: 'admin_barbearia_context',
} as const;

/**
 * Lista de todas as chaves possíveis no localStorage
 */
const ALL_AUTH_KEYS = [
  ...Object.values(TOKEN_KEYS),
  ...Object.values(CONTEXT_KEYS),
  'authToken', // Tokens legados que podem existir
  'auth-token',
  'token',
] as const;

/**
 * Token Manager - Gerenciador centralizado de tokens
 */
export class TokenManager {
  /**
   * Define o token para um tipo específico de usuário
   * IMPORTANTE: Remove automaticamente tokens de outros tipos para evitar conflitos
   * 
   * @param userType - Tipo de usuário
   * @param token - Token JWT
   * 
   * @example
   * TokenManager.setToken(UserType.BARBEIRO, 'eyJhbGc...');
   */
  static setToken(userType: UserType, token: string): void {
    // Limpar TODOS os tokens antes de definir o novo
    this.clearAllTokens();
    
    // Definir o novo token
    const key = TOKEN_KEYS[userType];
    localStorage.setItem(key, token);
    
    console.debug(`[TokenManager] Token definido para ${userType}`);
  }

  /**
   * Obtém o token para um tipo específico de usuário
   * 
   * @param userType - Tipo de usuário
   * @returns Token JWT ou null se não existir
   * 
   * @example
   * const token = TokenManager.getToken(UserType.BARBEIRO);
   */
  static getToken(userType: UserType): string | null {
    const key = TOKEN_KEYS[userType];
    return localStorage.getItem(key);
  }

  /**
   * Remove o token de um tipo específico de usuário
   * 
   * @param userType - Tipo de usuário
   * 
   * @example
   * TokenManager.removeToken(UserType.BARBEIRO);
   */
  static removeToken(userType: UserType): void {
    const key = TOKEN_KEYS[userType];
    localStorage.removeItem(key);
    console.debug(`[TokenManager] Token removido para ${userType}`);
  }

  /**
   * Define o contexto para Admin Barbearia
   * IMPORTANTE: Só funciona para UserType.ADMIN_BARBEARIA
   * 
   * @param data - Dados do contexto da barbearia
   * 
   * @example
   * TokenManager.setContext(UserType.ADMIN_BARBEARIA, { id: '123', nome: 'Barbearia X' });
   */
  static setContext(userType: UserType.ADMIN_BARBEARIA, data: unknown): void {
    const key = CONTEXT_KEYS[userType];
    localStorage.setItem(key, JSON.stringify(data));
    console.debug(`[TokenManager] Contexto definido para ${userType}`);
  }

  /**
   * Obtém o contexto para Admin Barbearia
   * 
   * @returns Dados do contexto ou null se não existir
   * 
   * @example
   * const context = TokenManager.getContext(UserType.ADMIN_BARBEARIA);
   */
  static getContext<T = unknown>(userType: UserType.ADMIN_BARBEARIA): T | null {
    const key = CONTEXT_KEYS[userType];
    const data = localStorage.getItem(key);
    
    if (!data) return null;
    
    try {
      return JSON.parse(data) as T;
    } catch {
      console.error('[TokenManager] Erro ao fazer parse do contexto');
      return null;
    }
  }

  /**
   * Remove o contexto de um tipo específico de usuário
   * 
   * @param userType - Tipo de usuário
   */
  static removeContext(userType: UserType.ADMIN_BARBEARIA): void {
    const key = CONTEXT_KEYS[userType];
    localStorage.removeItem(key);
    console.debug(`[TokenManager] Contexto removido para ${userType}`);
  }

  /**
   * Remove TODOS os tokens e contextos do localStorage
   * Usado durante logout ou ao trocar de tipo de usuário
   * 
   * @example
   * TokenManager.clearAllTokens();
   */
  static clearAllTokens(): void {
    ALL_AUTH_KEYS.forEach((key) => {
      localStorage.removeItem(key);
    });
    console.debug('[TokenManager] Todos os tokens foram removidos');
  }

  /**
   * Realiza logout completo removendo token e contexto do usuário
   * 
   * @param userType - Tipo de usuário
   * 
   * @example
   * TokenManager.logout(UserType.BARBEIRO);
   */
  static logout(userType: UserType): void {
    this.removeToken(userType);
    
    // Remover contexto se for Admin Barbearia
    if (userType === UserType.ADMIN_BARBEARIA) {
      this.removeContext(UserType.ADMIN_BARBEARIA);
    }
    
    console.debug(`[TokenManager] Logout realizado para ${userType}`);
  }

  /**
   * Verifica se há tokens conflitantes no localStorage
   * IMPORTANTE: Detecta se há mais de um tipo de token presente
   * 
   * @returns true se há conflito, false caso contrário
   * 
   * @example
   * if (TokenManager.hasConflictingTokens()) {
   *   console.warn('Tokens conflitantes detectados!');
   *   TokenManager.clearAllTokens();
   * }
   */
  static hasConflictingTokens(): boolean {
    const tokensPresent = Object.entries(TOKEN_KEYS)
      .filter(([_, key]) => localStorage.getItem(key) !== null)
      .map(([type]) => type);
    
    const hasConflict = tokensPresent.length > 1;
    
    if (hasConflict) {
      console.warn(
        `[TokenManager] CONFLITO DETECTADO! Múltiplos tokens encontrados: ${tokensPresent.join(', ')}`
      );
    }
    
    return hasConflict;
  }

  /**
   * Detecta qual tipo de usuário está autenticado com base no token presente
   * 
   * @returns UserType ou null se nenhum token for encontrado
   * 
   * @example
   * const currentUserType = TokenManager.getCurrentUserType();
   * if (currentUserType === UserType.BARBEIRO) {
   *   console.log('Usuário barbeiro autenticado');
   * }
   */
  static getCurrentUserType(): UserType | null {
    // Verificar se há conflito primeiro
    if (this.hasConflictingTokens()) {
      console.error('[TokenManager] Múltiplos tokens detectados! Limpando todos...');
      this.clearAllTokens();
      return null;
    }
    
    // Encontrar qual token existe
    for (const [type, key] of Object.entries(TOKEN_KEYS)) {
      if (localStorage.getItem(key)) {
        return type as UserType;
      }
    }
    
    return null;
  }

  /**
   * Valida a integridade do localStorage de autenticação
   * Remove tokens órfãos e inconsistências
   * 
   * @example
   * // Executar no início da aplicação
   * TokenManager.validateAuthState();
   */
  static validateAuthState(): void {
    console.debug('[TokenManager] Validando estado de autenticação...');
    
    // Verificar e limpar conflitos
    if (this.hasConflictingTokens()) {
      console.warn('[TokenManager] Limpando tokens conflitantes...');
      this.clearAllTokens();
      return;
    }
    
    // Verificar se contexto existe sem token (caso de Admin Barbearia)
    const hasContext = localStorage.getItem(CONTEXT_KEYS[UserType.ADMIN_BARBEARIA]);
    const hasAdminToken = localStorage.getItem(TOKEN_KEYS[UserType.ADMIN_BARBEARIA]);
    
    if (hasContext && !hasAdminToken) {
      console.warn('[TokenManager] Contexto órfão detectado! Removendo...');
      this.removeContext(UserType.ADMIN_BARBEARIA);
    }
    
    console.debug('[TokenManager] Validação concluída');
  }
}

/**
 * Inicializar validação ao carregar o módulo
 */
if (typeof window !== 'undefined') {
  TokenManager.validateAuthState();
}
