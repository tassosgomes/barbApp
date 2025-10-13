import axios from 'axios';

/**
 * Extract user-friendly error message from API error
 * @param error - Error object from API call
 * @returns User-friendly error message in Portuguese
 */
export function handleApiError(error: unknown): string {
  if (axios.isAxiosError(error)) {
    if (error.response) {
      // Backend responded with error
      const message = error.response.data?.message || error.response.data?.title;
      return message || 'Erro ao processar requisição';
    } else if (error.request) {
      // Request made but no response
      return 'Servidor não respondeu. Verifique sua conexão.';
    }
  }
  return 'Erro inesperado. Tente novamente.';
}