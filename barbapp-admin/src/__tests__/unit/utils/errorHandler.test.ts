/* eslint-disable @typescript-eslint/no-explicit-any */
import { describe, it, expect } from 'vitest';
import { handleApiError } from '@/utils/errorHandler';
import axios, { AxiosError } from 'axios';

describe('handleApiError', () => {
  it('should return user-friendly message for axios error with response', () => {
    const axiosError = {
      isAxiosError: true,
      response: {
        data: {
          message: 'Barbearia não encontrada',
        },
        status: 404,
      },
    } as unknown as AxiosError;

    const result = handleApiError(axiosError);
    expect(result).toBe('Barbearia não encontrada');
  });

  it('should return user-friendly message for axios error with title in response data', () => {
    const axiosError = {
      isAxiosError: true,
      response: {
        data: {
          title: 'Erro de validação',
        },
        status: 400,
      },
    } as unknown as AxiosError;

    const result = handleApiError(axiosError);
    expect(result).toBe('Erro de validação');
  });

  it('should return default message when axios error response has no message or title', () => {
    const axiosError = {
      isAxiosError: true,
      response: {
        data: {},
        status: 500,
      },
    } as unknown as AxiosError;

    const result = handleApiError(axiosError);
    expect(result).toBe('Erro ao processar requisição');
  });

  it('should return network error message for axios error with request but no response', () => {
    const axiosError = {
      isAxiosError: true,
      request: {},
    } as unknown as AxiosError;

    const result = handleApiError(axiosError);
    expect(result).toBe('Servidor não respondeu. Verifique sua conexão.');
  });

  it('should return generic error message for non-axios errors', () => {
    const genericError = new Error('Some unexpected error');

    const result = handleApiError(genericError);
    expect(result).toBe('Erro inesperado. Tente novamente.');
  });

  it('should return generic error message for string errors', () => {
    const stringError = 'String error message';

    const result = handleApiError(stringError);
    expect(result).toBe('Erro inesperado. Tente novamente.');
  });

  it('should return generic error message for null or undefined', () => {
    expect(handleApiError(null)).toBe('Erro inesperado. Tente novamente.');
    expect(handleApiError(undefined)).toBe('Erro inesperado. Tente novamente.');
  });

  it('should handle axios error with nested error structure', () => {
    const axiosError = {
      isAxiosError: true,
      response: {
        data: {
          errors: {
            name: ['Nome é obrigatório'],
            email: ['Email inválido'],
          },
        },
        status: 422,
      },
    } as unknown as AxiosError;

    const result = handleApiError(axiosError);
    expect(result).toBe('Erro ao processar requisição');
  });

  it('should handle axios error with array message', () => {
    const axiosError = {
      isAxiosError: true,
      response: {
        data: {
          message: ['Primeiro erro', 'Segundo erro'],
        },
        status: 400,
      },
    } as unknown as AxiosError;

    const result = handleApiError(axiosError);
    // The function returns the array as-is since it doesn't handle arrays specially
    expect(result).toEqual(['Primeiro erro', 'Segundo erro']);
  });

  it('should handle real axios error object', () => {
    const realAxiosError = new axios.AxiosError('Request failed');
    realAxiosError.response = {
      data: { message: 'Custom error message' },
      status: 400,
      statusText: 'Bad Request',
      headers: {},
      config: {} as any,
    };

    const result = handleApiError(realAxiosError);
    expect(result).toBe('Custom error message');
  });

  it('should handle axios timeout error', () => {
    const timeoutError = new axios.AxiosError('Timeout');
    timeoutError.code = 'ECONNABORTED';

    // Since the function doesn't specifically check for timeout codes,
    // it falls back to the generic error message
    const result = handleApiError(timeoutError);
    expect(result).toBe('Erro inesperado. Tente novamente.');
  });
});