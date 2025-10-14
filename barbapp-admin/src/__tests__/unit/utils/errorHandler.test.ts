import { describe, it, expect, vi } from 'vitest';
import { handleApiError } from '@/utils/errorHandler';

// Mock axios
vi.mock('axios', async (importOriginal) => {
  const actual = await importOriginal<typeof import('axios')>();
  return {
    ...actual,
    default: {
      ...actual.default,
      isAxiosError: vi.fn(),
    },
  };
});

const mockIsAxiosError = vi.mocked((await import('axios')).default.isAxiosError);

describe('handleApiError', () => {
  it('should handle axios error with response data message', () => {
    const mockError = {
      response: {
        data: {
          message: 'Custom error message',
        },
      },
    };

    mockIsAxiosError.mockReturnValue(true);

    const result = handleApiError(mockError);

    expect(result).toBe('Custom error message');
  });

  it('should handle axios error with response data title', () => {
    const mockError = {
      response: {
        data: {
          title: 'Error title',
        },
      },
    };

    mockIsAxiosError.mockReturnValue(true);

    const result = handleApiError(mockError);

    expect(result).toBe('Error title');
  });

  it('should handle axios error with default message when no custom message', () => {
    const mockError = {
      response: {
        data: {},
      },
    };

    mockIsAxiosError.mockReturnValue(true);

    const result = handleApiError(mockError);

    expect(result).toBe('Erro ao processar requisição');
  });

  it('should handle axios error with request but no response', () => {
    const mockError = {
      request: {},
    };

    mockIsAxiosError.mockReturnValue(true);

    const result = handleApiError(mockError);

    expect(result).toBe('Servidor não respondeu. Verifique sua conexão.');
  });

  it('should handle non-axios errors', () => {
    const mockError = new Error('Some other error');

    mockIsAxiosError.mockReturnValue(false);

    const result = handleApiError(mockError);

    expect(result).toBe('Erro inesperado. Tente novamente.');
  });

  it('should handle non-Error objects', () => {
    const mockError = 'String error';

    mockIsAxiosError.mockReturnValue(false);

    const result = handleApiError(mockError);

    expect(result).toBe('Erro inesperado. Tente novamente.');
  });
});