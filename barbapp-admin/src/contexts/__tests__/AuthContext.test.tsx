import { renderHook, act, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { AuthProvider, useAuth } from '../AuthContext';
import { authService } from '@/services/auth.service';
import type { AuthResponse, User } from '@/types/auth.types';

// Mock do authService
vi.mock('@/services/auth.service', () => ({
  authService: {
    login: vi.fn(),
    validateToken: vi.fn(),
    logout: vi.fn()
  }
}));

// Mock do useNavigate
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate
  };
});

describe('AuthContext', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  afterEach(() => {
    localStorage.clear();
  });

  const wrapper = ({ children }: { children: React.ReactNode }) => (
    <BrowserRouter>
      <AuthProvider>{children}</AuthProvider>
    </BrowserRouter>
  );

  describe('useAuth', () => {
    it('deve lançar erro se usado fora do AuthProvider', () => {
      // Suprimir console.error para o teste
      const originalError = console.error;
      console.error = vi.fn();

      expect(() => {
        renderHook(() => useAuth());
      }).toThrow('useAuth must be used within an AuthProvider');

      console.error = originalError;
    });

    it('deve retornar o contexto quando usado dentro do AuthProvider', () => {
      const { result } = renderHook(() => useAuth(), { wrapper });

      expect(result.current).toBeDefined();
      expect(result.current).toHaveProperty('user');
      expect(result.current).toHaveProperty('isAuthenticated');
      expect(result.current).toHaveProperty('isLoading');
      expect(result.current).toHaveProperty('login');
      expect(result.current).toHaveProperty('logout');
      expect(result.current).toHaveProperty('validateSession');
    });
  });

  describe('Estado inicial', () => {
    it('deve iniciar com user null e isAuthenticated false', async () => {
      vi.mocked(authService.validateToken).mockRejectedValue(new Error('No token'));

      const { result } = renderHook(() => useAuth(), { wrapper });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(result.current.user).toBeNull();
      expect(result.current.isAuthenticated).toBe(false);
    });

    it('deve validar sessão automaticamente ao montar', async () => {
      const mockUser: User = {
        id: '123',
        name: 'João Silva',
        email: 'joao@test.com',
        role: 'Barbeiro',
        barbeariaId: 'barb-123',
        nomeBarbearia: 'Barbearia Teste'
      };

      localStorage.setItem('barbapp-barber-token', 'valid-token');
      vi.mocked(authService.validateToken).mockResolvedValue(mockUser);

      const { result } = renderHook(() => useAuth(), { wrapper });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(result.current.user).toEqual(mockUser);
      expect(result.current.isAuthenticated).toBe(true);
      expect(authService.validateToken).toHaveBeenCalledOnce();
    });
  });

  describe('login', () => {
    it('deve realizar login com sucesso', async () => {
      const mockAuthResponse: AuthResponse = {
        token: 'new-token',
        tipoUsuario: 'Barbeiro',
        barbeariaId: 'barb-123',
        nomeBarbearia: 'Barbearia Teste',
        codigoBarbearia: 'ABC123',
        expiresAt: '2025-10-20T18:00:00Z'
      };

      const mockUser: User = {
        id: '123',
        name: 'João Silva',
        email: 'joao@test.com',
        role: 'Barbeiro',
        barbeariaId: 'barb-123',
        nomeBarbearia: 'Barbearia Teste'
      };

      vi.mocked(authService.login).mockResolvedValue(mockAuthResponse);
      vi.mocked(authService.validateToken).mockResolvedValue(mockUser);

      const { result } = renderHook(() => useAuth(), { wrapper });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      await act(async () => {
        await result.current.login({
          email: 'joao@test.com',
          password: 'senha123'
        });
      });

      expect(authService.login).toHaveBeenCalledWith({
        email: 'joao@test.com',
        password: 'senha123'
      });
      expect(localStorage.getItem('barbapp-barber-token')).toBe('new-token');
      expect(result.current.user).toEqual(mockUser);
      expect(result.current.isAuthenticated).toBe(true);
      expect(mockNavigate).toHaveBeenCalledWith('/barber/schedule');
    });

    it('deve propagar erro quando login falha', async () => {
      const loginError = new Error('Credenciais inválidas');
      vi.mocked(authService.login).mockRejectedValue(loginError);

      const { result } = renderHook(() => useAuth(), { wrapper });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      await expect(async () => {
        await act(async () => {
          await result.current.login({
            email: 'joao@test.com',
            password: 'senha-errada'
          });
        });
      }).rejects.toThrow('Credenciais inválidas');

      expect(result.current.user).toBeNull();
      expect(result.current.isAuthenticated).toBe(false);
      expect(localStorage.getItem('barbapp-barber-token')).toBeNull();
    });
  });

  describe('logout', () => {
    it('deve realizar logout e limpar estado', async () => {
      const mockUser: User = {
        id: '123',
        name: 'João Silva',
        email: 'joao@test.com',
        role: 'Barbeiro',
        barbeariaId: 'barb-123',
        nomeBarbearia: 'Barbearia Teste'
      };

      localStorage.setItem('barbapp-barber-token', 'valid-token');
      vi.mocked(authService.validateToken).mockResolvedValue(mockUser);

      const { result } = renderHook(() => useAuth(), { wrapper });

      await waitFor(() => {
        expect(result.current.user).toEqual(mockUser);
      });

      act(() => {
        result.current.logout();
      });

      expect(authService.logout).toHaveBeenCalledOnce();
      expect(result.current.user).toBeNull();
      expect(result.current.isAuthenticated).toBe(false);
      expect(mockNavigate).toHaveBeenCalledWith('/login');
    });
  });

  describe('validateSession', () => {
    it('deve retornar false quando não há token', async () => {
      const { result } = renderHook(() => useAuth(), { wrapper });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      const isValid = await act(async () => {
        return await result.current.validateSession();
      });

      expect(isValid).toBe(false);
      expect(result.current.user).toBeNull();
    });

    it('deve retornar true e definir usuário quando token é válido', async () => {
      const mockUser: User = {
        id: '123',
        name: 'João Silva',
        email: 'joao@test.com',
        role: 'Barbeiro',
        barbeariaId: 'barb-123',
        nomeBarbearia: 'Barbearia Teste'
      };

      localStorage.setItem('barbapp-barber-token', 'valid-token');
      vi.mocked(authService.validateToken).mockResolvedValue(mockUser);

      const { result } = renderHook(() => useAuth(), { wrapper });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      const isValid = await act(async () => {
        return await result.current.validateSession();
      });

      expect(isValid).toBe(true);
      expect(result.current.user).toEqual(mockUser);
    });

    it('deve retornar false e limpar token quando validação falha', async () => {
      localStorage.setItem('barbapp-barber-token', 'invalid-token');
      vi.mocked(authService.validateToken).mockRejectedValue(new Error('Token expired'));

      const { result } = renderHook(() => useAuth(), { wrapper });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      const isValid = await act(async () => {
        return await result.current.validateSession();
      });

      expect(isValid).toBe(false);
      expect(result.current.user).toBeNull();
      expect(localStorage.getItem('barbapp-barber-token')).toBeNull();
    });
  });
});
