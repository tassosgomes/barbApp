import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { authService } from '../auth.service';
import api from '../api';
import type { LoginInput, AuthResponse, User } from '@/types/auth.types';

// Mock do módulo api
vi.mock('../api');

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
        email: 'barbeiro@example.com',
        password: 'SenhaSegura123!'
      };

      const expectedResponse: AuthResponse = {
        token: 'fake-jwt-token',
        tipoUsuario: 'Barbeiro',
        barbeariaId: 'barbershop-123',
        nomeBarbearia: 'Barbearia Teste',
        codigoBarbearia: 'ABC123',
        expiresAt: new Date(Date.now() + 3600000).toISOString()
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
        email: 'barbeiro@example.com',
        password: 'SenhaSegura123!'
      });
      expect(result).toEqual(expectedResponse);
    });

    it('deve enviar email e password sem transformações', async () => {
      // Arrange
      const loginData: LoginInput = {
        email: 'TESTE@EXAMPLE.COM',
        password: 'MinhaSenh@123'
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
          email: 'TESTE@EXAMPLE.COM',
          password: 'MinhaSenh@123'
        })
      );
    });

    it('deve lançar erro quando credenciais são inválidas (401)', async () => {
      // Arrange
      const loginData: LoginInput = {
        email: 'invalido@example.com',
        password: 'senhaerrada'
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
        email: '',
        password: ''
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
        email: 'barbeiro@example.com',
        password: 'SenhaSegura123!'
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
        email: 'barbeiro@example.com',
        role: 'Barbeiro',
        barbeariaId: 'barbershop-123',
        nomeBarbearia: 'Barbearia Teste'
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
      // Arrange - coloca um token no localStorage
      localStorage.setItem('barbapp-barber-token', 'fake-token');
      expect(localStorage.getItem('barbapp-barber-token')).toBe('fake-token');
      
      // Act
      authService.logout();

      // Assert - verifica que foi removido
      expect(localStorage.getItem('barbapp-barber-token')).toBeNull();
    });

    it('deve funcionar mesmo se token não existir', () => {
      // Arrange - garante que não há token
      localStorage.removeItem('barbapp-barber-token');
      
      // Act & Assert - não deve lançar erro
      expect(() => authService.logout()).not.toThrow();
      expect(localStorage.getItem('barbapp-barber-token')).toBeNull();
    });

    it('deve remover apenas o token do barbeiro', () => {
      // Arrange - coloca múltiplos tokens
      localStorage.setItem('barbapp-barber-token', 'fake-token');
      localStorage.setItem('barbapp-admin-token', 'admin-token');
      localStorage.setItem('other-key', 'other-value');
      
      // Act
      authService.logout();

      // Assert - deve remover apenas o token do barbeiro
      expect(localStorage.getItem('barbapp-barber-token')).toBeNull();
      // Outros tokens devem permanecer
      expect(localStorage.getItem('barbapp-admin-token')).toBe('admin-token');
      expect(localStorage.getItem('other-key')).toBe('other-value');
    });
  });
});
