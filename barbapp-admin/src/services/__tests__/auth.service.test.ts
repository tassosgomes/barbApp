import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { authService } from '../auth.service';
import api from '../api';
import type { LoginInput, AuthResponse, User } from '@/types/auth.types';

// Mock do módulo api
vi.mock('../api');

// Mock do phone-utils
vi.mock('@/lib/phone-utils', () => ({
  formatPhoneToAPI: (phone: string) => `+55${phone.replace(/\D/g, '')}`
}));

describe('authService', () => {
  beforeEach(() => {
    // Limpar localStorage antes de cada teste
    localStorage.clear();
    // Limpar mocks
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('login', () => {
    it('deve fazer login com sucesso e retornar token e usuário', async () => {
      // Arrange
      const loginData: LoginInput = {
        barbershopCode: 'barb001',
        phone: '(11) 99999-9999'
      };

      const expectedResponse: AuthResponse = {
        token: 'fake-jwt-token',
        user: {
          id: 'user-123',
          name: 'João Silva',
          phone: '+5511999999999',
          role: 'Barbeiro',
          barbershopId: 'barbershop-123'
        }
      };

      // Mock da resposta do axios
      vi.mocked(api.post).mockResolvedValue({
        data: expectedResponse,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as any
      });

      // Act
      const result = await authService.login(loginData);

      // Assert
      expect(api.post).toHaveBeenCalledWith('/auth/barbeiro/login', {
        barbershopCode: 'BARB001', // Deve converter para UPPERCASE
        phone: '+5511999999999' // Deve formatar para API
      });
      expect(result).toEqual(expectedResponse);
    });

    it('deve converter barbershopCode para uppercase', async () => {
      // Arrange
      const loginData: LoginInput = {
        barbershopCode: 'barb001',
        phone: '(11) 99999-9999'
      };

      vi.mocked(api.post).mockResolvedValue({
        data: { token: 'token', user: {} as User },
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as any
      });

      // Act
      await authService.login(loginData);

      // Assert
      expect(api.post).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          barbershopCode: 'BARB001'
        })
      );
    });

    it('deve lançar erro quando credenciais são inválidas (401)', async () => {
      // Arrange
      const loginData: LoginInput = {
        barbershopCode: 'INVALID',
        phone: '(11) 99999-9999'
      };

      const error = {
        response: {
          status: 401,
          data: { message: 'Invalid credentials' }
        }
      };

      vi.mocked(api.post).mockRejectedValue(error);

      // Act & Assert
      await expect(authService.login(loginData)).rejects.toEqual(error);
    });

    it('deve lançar erro quando dados são inválidos (400)', async () => {
      // Arrange
      const loginData: LoginInput = {
        barbershopCode: '',
        phone: ''
      };

      const error = {
        response: {
          status: 400,
          data: { message: 'Validation failed' }
        }
      };

      vi.mocked(api.post).mockRejectedValue(error);

      // Act & Assert
      await expect(authService.login(loginData)).rejects.toEqual(error);
    });

    it('deve lançar erro quando servidor retorna 500', async () => {
      // Arrange
      const loginData: LoginInput = {
        barbershopCode: 'BARB001',
        phone: '(11) 99999-9999'
      };

      const error = {
        response: {
          status: 500,
          data: { message: 'Internal server error' }
        }
      };

      vi.mocked(api.post).mockRejectedValue(error);

      // Act & Assert
      await expect(authService.login(loginData)).rejects.toEqual(error);
    });
  });

  describe('validateToken', () => {
    it('deve validar token com sucesso e retornar dados do usuário', async () => {
      // Arrange
      const expectedUser: User = {
        id: 'user-123',
        name: 'João Silva',
        phone: '+5511999999999',
        role: 'Barbeiro',
        barbershopId: 'barbershop-123'
      };

      vi.mocked(api.get).mockResolvedValue({
        data: expectedUser,
        status: 200,
        statusText: 'OK',
        headers: {},
        config: {} as any
      });

      // Act
      const result = await authService.validateToken();

      // Assert
      expect(api.get).toHaveBeenCalledWith('/barber/profile');
      expect(result).toEqual(expectedUser);
    });

    it('deve lançar erro quando token é inválido (401)', async () => {
      // Arrange
      const error = {
        response: {
          status: 401,
          data: { message: 'Invalid or expired token' }
        }
      };

      vi.mocked(api.get).mockRejectedValue(error);

      // Act & Assert
      await expect(authService.validateToken()).rejects.toEqual(error);
    });

    it('deve lançar erro quando servidor está indisponível', async () => {
      // Arrange
      const error = {
        request: {},
        message: 'Network Error'
      };

      vi.mocked(api.get).mockRejectedValue(error);

      // Act & Assert
      await expect(authService.validateToken()).rejects.toEqual(error);
    });
  });

  describe('logout', () => {
    it('deve remover token do localStorage', () => {
      // Act
      authService.logout();

      // Assert
      expect(localStorage.removeItem).toHaveBeenCalledWith('barbapp-barber-token');
    });

    it('deve funcionar mesmo se token não existir', () => {
      // Act & Assert - não deve lançar erro
      expect(() => authService.logout()).not.toThrow();
      expect(localStorage.removeItem).toHaveBeenCalledWith('barbapp-barber-token');
    });

    it('deve remover apenas o token do barbeiro', () => {
      // Act
      authService.logout();

      // Assert - deve remover apenas o token do barbeiro
      expect(localStorage.removeItem).toHaveBeenCalledWith('barbapp-barber-token');
      // Verifica que a chave específica foi usada
      const calls = vi.mocked(localStorage.removeItem).mock.calls;
      expect(calls[calls.length - 1][0]).toBe('barbapp-barber-token');
    });
  });
});
