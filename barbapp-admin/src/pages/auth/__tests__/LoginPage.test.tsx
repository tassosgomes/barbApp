import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { LoginPage } from '../LoginPage';
import { AuthContext } from '@/contexts/AuthContext';
import type { AuthContextType } from '@/types/auth.types';

// Mock do useToast
vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: vi.fn(),
  }),
}));

describe('LoginPage', () => {
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
  
  const renderLoginPage = (authContext: Partial<AuthContextType> = {}) => {
    const contextValue = { ...mockAuthContext, ...authContext };
    
    return render(
      <AuthContext.Provider value={contextValue}>
        <LoginPage />
      </AuthContext.Provider>
    );
  };
  
  describe('Renderização', () => {
    it('deve renderizar a página de login', () => {
      renderLoginPage();
      
      expect(screen.getByTestId('login-page')).toBeInTheDocument();
    });
    
    it('deve renderizar título e descrição', () => {
      renderLoginPage();
      
      expect(screen.getByText(/login barbeiro/i)).toBeInTheDocument();
      expect(screen.getByText(/entre com seu e-mail e senha/i)).toBeInTheDocument();
    });
    
    it('deve renderizar o formulário de login', () => {
      renderLoginPage();
      
      expect(screen.getByTestId('login-form')).toBeInTheDocument();
    });
    
    it('deve renderizar link de ajuda', () => {
      renderLoginPage();
      
      expect(screen.getByText(/primeiro acesso\?/i)).toBeInTheDocument();
      expect(screen.getByText(/precisa de ajuda\?/i)).toBeInTheDocument();
    });
    
    it('deve ter layout centralizado e responsivo', () => {
      renderLoginPage();
      
      const container = screen.getByTestId('login-page');
      
      expect(container).toHaveClass('min-h-screen');
      expect(container).toHaveClass('flex');
      expect(container).toHaveClass('items-center');
      expect(container).toHaveClass('justify-center');
      expect(container).toHaveClass('p-4');
      // Background agora usa gradiente
      expect(container.className).toMatch(/bg-gradient|from-gray-50/);
    });
  });
  
  describe('Modal de Ajuda', () => {
    it('não deve exibir modal de ajuda inicialmente', () => {
      renderLoginPage();
      
      expect(screen.queryByTestId('help-modal')).not.toBeInTheDocument();
    });
    
    it('deve abrir modal ao clicar no link de ajuda', async () => {
      const user = userEvent.setup();
      renderLoginPage();
      
      const helpButton = screen.getByTestId('help-button');
      await user.click(helpButton);
      
      expect(screen.getByTestId('help-modal')).toBeInTheDocument();
    });
    
    it('deve exibir conteúdo correto no modal de ajuda', async () => {
      const user = userEvent.setup();
      renderLoginPage();
      
      const helpButton = screen.getByTestId('help-button');
      await user.click(helpButton);
      
      expect(screen.getByText(/como fazer login/i)).toBeInTheDocument();
      expect(screen.getByText(/📧 e-mail:/i)).toBeInTheDocument();
      expect(screen.getByText(/use o e-mail que foi cadastrado/i)).toBeInTheDocument();
      expect(screen.getByText(/🔒 senha:/i)).toBeInTheDocument();
      expect(screen.getByText(/use a senha fornecida/i)).toBeInTheDocument();
      expect(screen.getByText(/❓ não tem acesso\?/i)).toBeInTheDocument();
      expect(screen.getByText(/entre em contato com o administrador/i)).toBeInTheDocument();
    });
    
    it('deve fechar modal ao clicar no botão "Entendi"', async () => {
      const user = userEvent.setup();
      renderLoginPage();
      
      // Abrir modal
      const helpButton = screen.getByTestId('help-button');
      await user.click(helpButton);
      
      expect(screen.getByTestId('help-modal')).toBeInTheDocument();
      
      // Fechar modal
      const closeButton = screen.getByTestId('help-close-button');
      await user.click(closeButton);
      
      expect(screen.queryByTestId('help-modal')).not.toBeInTheDocument();
    });
    
    it('deve ter ícone de ajuda no título do modal', async () => {
      const user = userEvent.setup();
      renderLoginPage();
      
      const helpButton = screen.getByTestId('help-button');
      await user.click(helpButton);
      
      // Verificar que há um SVG (ícone Lucide)
      const modal = screen.getByTestId('help-modal');
      const icons = modal.querySelectorAll('svg');
      
      expect(icons.length).toBeGreaterThan(0);
    });
  });
  
  describe('Acessibilidade', () => {
    it('deve ter foco no botão de ajuda quando clicado', () => {
      renderLoginPage();
      
      const helpButton = screen.getByTestId('help-button');
      
      expect(helpButton).toHaveClass('focus:outline-none');
      expect(helpButton).toHaveClass('focus:ring-2');
      expect(helpButton).toHaveClass('focus:ring-blue-500');
    });
    
    it('deve ter estrutura semântica correta', () => {
      renderLoginPage();
      
      // Verificar que há um botão de ajuda
      const helpButton = screen.getByTestId('help-button');
      expect(helpButton.tagName).toBe('BUTTON');
      
      // Verificar que há um formulário
      const form = screen.getByTestId('login-form');
      expect(form.tagName).toBe('FORM');
    });
  });
  
  describe('Responsividade', () => {
    it('deve ter card com largura máxima', () => {
      renderLoginPage();
      
      const page = screen.getByTestId('login-page');
      const card = page.querySelector('.max-w-md');
      
      expect(card).toBeInTheDocument();
    });
    
    it('deve ter padding adequado para mobile', () => {
      renderLoginPage();
      
      const page = screen.getByTestId('login-page');
      
      expect(page).toHaveClass('p-4');
    });
  });
});
