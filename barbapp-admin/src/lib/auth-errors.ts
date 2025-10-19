/**
 * Utilitário para gerar mensagens de erro contextuais e acionáveis
 * para diferentes cenários de autenticação.
 * 
 * Fornece feedback específico baseado no status HTTP da resposta,
 * com sugestões de correção quando apropriado.
 */

/**
 * Retorna uma mensagem de erro contextual baseada na resposta HTTP
 * 
 * @param error - Objeto de erro do Axios ou similar
 * @returns Mensagem de erro amigável e acionável
 * 
 * @example
 * ```typescript
 * try {
 *   await authService.login(data);
 * } catch (error) {
 *   const message = getAuthErrorMessage(error);
 *   toast({ variant: 'destructive', description: message });
 * }
 * ```
 */
export function getAuthErrorMessage(error: any): string {
  const status = error.response?.status;
  
  // Erros específicos por código HTTP
  switch (status) {
    case 400:
      return 'Dados inválidos. Verifique o e-mail e senha informados.';
    
    case 401:
      return 'E-mail ou senha incorretos. Verifique suas credenciais e tente novamente.';
    
    case 404:
      return 'Usuário não encontrado. Confirme seus dados com o administrador.';
    
    case 500:
      return 'Erro no servidor. Por favor, tente novamente em instantes.';
    
    case 503:
      return 'Serviço temporariamente indisponível. Tente em alguns minutos.';
    
    default:
      // Verifica se há problema de conectividade
      if (!navigator.onLine) {
        return 'Sem conexão com a internet. Verifique sua conexão e tente novamente.';
      }
      
      // Mensagem genérica para outros erros
      return 'Erro ao conectar. Por favor, tente novamente.';
  }
}

/**
 * Retorna título e descrição para toasts de erro
 * 
 * @param error - Objeto de erro do Axios ou similar
 * @returns Objeto com título e descrição para toast
 * 
 * @example
 * ```typescript
 * const { title, description } = getAuthErrorToast(error);
 * toast({ variant: 'destructive', title, description });
 * ```
 */
export function getAuthErrorToast(error: any): { title: string; description: string } {
  const status = error.response?.status;
  
  switch (status) {
    case 400:
      return {
        title: 'Dados inválidos',
        description: 'Verifique o e-mail e senha informados.',
      };
    
    case 401:
      return {
        title: 'Erro de autenticação',
        description: 'E-mail ou senha incorretos. Verifique e tente novamente.',
      };
    
    case 404:
      return {
        title: 'Usuário não encontrado',
        description: 'Confirme seus dados com o administrador da barbearia.',
      };
    
    case 500:
      return {
        title: 'Erro no servidor',
        description: 'Tente novamente em instantes.',
      };
    
    case 503:
      return {
        title: 'Serviço indisponível',
        description: 'O sistema está temporariamente fora do ar. Tente em alguns minutos.',
      };
    
    default:
      if (!navigator.onLine) {
        return {
          title: 'Sem conexão',
          description: 'Verifique sua conexão com a internet e tente novamente.',
        };
      }
      
      return {
        title: 'Erro ao conectar',
        description: 'Ocorreu um problema. Por favor, tente novamente.',
      };
  }
}
