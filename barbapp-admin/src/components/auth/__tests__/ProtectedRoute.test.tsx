import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { ProtectedRoute } from '../ProtectedRoute';
import { AuthContext } from '@/contexts/AuthContext';
import type { AuthContextType } from '@/types/auth.types';

describe('ProtectedRoute', () => {
  const mockLogin = vi.fn();
  const mockLogout = vi.fn();
  const mockValidateSession = vi.fn();
  
  const mockAuthContext: AuthContextType = {
    user: null,
    isAuthenticated: false,
    isLoading: false,
    login: mockLogin,
    logout: mockLogout,
    validateSession: mockValidateSession,
  };
  
  beforeEach(() => {
    vi.clearAllMocks();
  });
  
  const renderProtectedRoute = (
    authContext: Partial<AuthContextType> = {},
    initialRoute = '/protected'
  ) => {
    const contextValue = { ...mockAuthContext, ...authContext };
    
    return render(
      <AuthContext.Provider value={contextValue}>
        <MemoryRouter initialEntries={[initialRoute]}>
          <Routes>
            <Route path="/login" element={<div data-testid="login-page">Login</div>} />
            <Route element={<ProtectedRoute />}>
              <Route path="/protected" element={<div data-testid="protected-content">Protected</div>} />
            </Route>
          </Routes>
        </MemoryRouter>
      </AuthContext.Provider>
    );
  };
  
  describe('Estado de Loading', () => {
    it('deve exibir loading quando isLoading é true', () => {
      renderProtectedRoute({ isLoading: true });
      
      expect(screen.getByTestId('protected-route-loading')).toBeInTheDocument();
      expect(screen.getByTestId('loading-spinner')).toBeInTheDocument();
      expect(screen.getByText(/verificando autenticação/i)).toBeInTheDocument();
    });
    
    it('não deve exibir conteúdo protegido durante loading', () => {
      renderProtectedRoute({ isLoading: true });
      
      expect(screen.queryByTestId('protected-content')).not.toBeInTheDocument();
    });
    
    it('não deve redirecionar para login durante loading', () => {
      renderProtectedRoute({ isLoading: true });
      
      expect(screen.queryByTestId('login-page')).not.toBeInTheDocument();
    });
  });
  
  describe('Usuário Não Autenticado', () => {
    it('deve redirecionar para /login quando não autenticado', () => {
      renderProtectedRoute({ isAuthenticated: false, isLoading: false });
      
      expect(screen.getByTestId('login-page')).toBeInTheDocument();
      expect(screen.queryByTestId('protected-content')).not.toBeInTheDocument();
    });
    
    it('não deve exibir loading quando não autenticado', () => {
      renderProtectedRoute({ isAuthenticated: false, isLoading: false });
      
      expect(screen.queryByTestId('protected-route-loading')).not.toBeInTheDocument();
    });
  });
  
  describe('Usuário Autenticado', () => {
    it('deve renderizar conteúdo protegido quando autenticado', () => {
      renderProtectedRoute({
        isAuthenticated: true,
        isLoading: false,
        user: {
          id: '1',
          name: 'João Silva',
          email: 'joao@example.com',
          role: 'Barbeiro',
          barbeariaId: '1',
          nomeBarbearia: 'Barbearia Teste',
        },
      });
      
      expect(screen.getByTestId('protected-content')).toBeInTheDocument();
      expect(screen.getByText('Protected')).toBeInTheDocument();
    });
    
    it('não deve exibir loading quando autenticado', () => {
      renderProtectedRoute({
        isAuthenticated: true,
        isLoading: false,
        user: {
          id: '1',
          name: 'João Silva',
          email: 'joao@example.com',
          role: 'Barbeiro',
          barbeariaId: '1',
          nomeBarbearia: 'Barbearia Teste',
        },
      });
      
      expect(screen.queryByTestId('protected-route-loading')).not.toBeInTheDocument();
    });
    
    it('não deve redirecionar para login quando autenticado', () => {
      renderProtectedRoute({
        isAuthenticated: true,
        isLoading: false,
        user: {
          id: '1',
          name: 'João Silva',
          email: 'joao@example.com',
          role: 'Barbeiro',
          barbeariaId: '1',
          nomeBarbearia: 'Barbearia Teste',
        },
      });
      
      expect(screen.queryByTestId('login-page')).not.toBeInTheDocument();
    });
  });
  
  describe('Transições de Estado', () => {
    it('deve transitar de loading para conteúdo quando autenticado', () => {
      const { rerender } = renderProtectedRoute({ isLoading: true });
      
      expect(screen.getByTestId('protected-route-loading')).toBeInTheDocument();
      
      // Simula fim do loading com usuário autenticado
      rerender(
        <AuthContext.Provider
          value={{
            ...mockAuthContext,
            isAuthenticated: true,
            isLoading: false,
            user: {
              id: '1',
              name: 'João Silva',
              email: 'joao@example.com',
              role: 'Barbeiro',
              barbeariaId: '1',
              nomeBarbearia: 'Barbearia Teste',
            },
          }}
        >
          <MemoryRouter initialEntries={['/protected']}>
            <Routes>
              <Route path="/login" element={<div data-testid="login-page">Login</div>} />
              <Route element={<ProtectedRoute />}>
                <Route path="/protected" element={<div data-testid="protected-content">Protected</div>} />
              </Route>
            </Routes>
          </MemoryRouter>
        </AuthContext.Provider>
      );
      
      expect(screen.queryByTestId('protected-route-loading')).not.toBeInTheDocument();
      expect(screen.getByTestId('protected-content')).toBeInTheDocument();
    });
    
    it('deve transitar de loading para login quando não autenticado', () => {
      const { rerender } = renderProtectedRoute({ isLoading: true });
      
      expect(screen.getByTestId('protected-route-loading')).toBeInTheDocument();
      
      // Simula fim do loading sem autenticação
      rerender(
        <AuthContext.Provider
          value={{
            ...mockAuthContext,
            isAuthenticated: false,
            isLoading: false,
          }}
        >
          <MemoryRouter initialEntries={['/protected']}>
            <Routes>
              <Route path="/login" element={<div data-testid="login-page">Login</div>} />
              <Route element={<ProtectedRoute />}>
                <Route path="/protected" element={<div data-testid="protected-content">Protected</div>} />
              </Route>
            </Routes>
          </MemoryRouter>
        </AuthContext.Provider>
      );
      
      expect(screen.queryByTestId('protected-route-loading')).not.toBeInTheDocument();
      expect(screen.getByTestId('login-page')).toBeInTheDocument();
    });
  });
  
  describe('Acessibilidade', () => {
    it('deve ter estrutura semântica correta no loading', () => {
      renderProtectedRoute({ isLoading: true });
      
      const loadingContainer = screen.getByTestId('protected-route-loading');
      expect(loadingContainer).toHaveClass('min-h-screen');
      expect(loadingContainer).toHaveClass('flex');
      expect(loadingContainer).toHaveClass('items-center');
      expect(loadingContainer).toHaveClass('justify-center');
    });
    
    it('deve ter texto explicativo durante loading', () => {
      renderProtectedRoute({ isLoading: true });
      
      const text = screen.getByText(/verificando autenticação/i);
      expect(text).toHaveClass('text-gray-700');
      expect(text).toHaveClass('font-medium');
      expect(text).toHaveClass('text-base');
    });
  });
});
