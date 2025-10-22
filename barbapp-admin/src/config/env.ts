/**
 * Configurações de ambiente
 * 
 * Este arquivo centraliza o acesso às variáveis de ambiente do Vite.
 * No Docker, essas variáveis são substituídas em tempo de execução.
 */

export const env = {
  /**
   * URL base da API backend
   */
  API_URL: import.meta.env.VITE_API_URL || 'http://localhost:5070/api',
  
  /**
   * Nome da aplicação
   */
  APP_NAME: import.meta.env.VITE_APP_NAME || 'BarbApp Admin',
  
  /**
   * Indica se está em modo de desenvolvimento
   */
  IS_DEV: import.meta.env.DEV,
  
  /**
   * Indica se está em modo de produção
   */
  IS_PROD: import.meta.env.PROD,
} as const;

/**
 * Configurações derivadas das variáveis de ambiente
 */
export const config = {
  /**
   * URLs da API
   */
  api: {
    baseURL: env.API_URL,
    timeout: 10000,
    
    // Endpoints específicos
    endpoints: {
      auth: `${env.API_URL}/auth`,
      barbershops: `${env.API_URL}/barbershops`,
      landingPages: `${env.API_URL}/landing-pages`,
      uploads: `${env.API_URL}/uploads`,
    },
  },
  
  /**
   * Configurações da aplicação
   */
  app: {
    name: env.APP_NAME,
    version: '1.0.0',
    description: 'Sistema administrativo para barbearias',
  },
  
  /**
   * Configurações de desenvolvimento
   */
  dev: {
    enableDevTools: env.IS_DEV,
    logLevel: env.IS_DEV ? 'debug' : 'warn',
  },
} as const;

/**
 * Função para validar se todas as variáveis obrigatórias estão definidas
 */
export function validateEnvironment(): void {
  const requiredVars = {
    VITE_API_URL: env.API_URL,
    VITE_APP_NAME: env.APP_NAME,
  };

  const missingVars = Object.entries(requiredVars)
    .filter(([, value]) => !value || value.trim() === '')
    .map(([key]) => key);

  if (missingVars.length > 0) {
    throw new Error(
      `Variáveis de ambiente obrigatórias não estão definidas: ${missingVars.join(', ')}`
    );
  }

  console.log('✅ Configurações de ambiente validadas:', {
    API_URL: env.API_URL,
    APP_NAME: env.APP_NAME,
    IS_DEV: env.IS_DEV,
  });
}

/**
 * Log das configurações atuais (apenas em desenvolvimento)
 */
if (env.IS_DEV) {
  console.log('🔧 Configurações de ambiente:', config);
}