import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { LoginForm } from '../LoginForm';
import { AuthContext } from '@/contexts/AuthContext';
import type { AuthContextType } from '@/types/auth.types';

// Mock do useToast
vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: vi.fn(),
  }),
}));

describe('LoginForm', () => {
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
  
  const renderLoginForm = (authContext: Partial<AuthContextType> = {}) => {
    const contextValue = { ...mockAuthContext, ...authContext };
    
    return render(
      <AuthContext.Provider value={contextValue}>
        <LoginForm />
      </AuthContext.Provider>
    );
  };
  
  describe('Renderização', () => {
    it('deve renderizar todos os campos do formulário', () => {
      renderLoginForm();
      
      expect(screen.getByLabelText(/e-mail/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/senha/i)).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /entrar/i })).toBeInTheDocument();
    });
    
    it('deve renderizar placeholders corretos', () => {
      renderLoginForm();
      
      expect(screen.getByPlaceholderText(/seu.email@exemplo.com/i)).toBeInTheDocument();
      expect(screen.getByPlaceholderText(/••••••/i)).toBeInTheDocument();
    });
    
    it('deve ter campos com autocomplete correto', () => {
      renderLoginForm();
      
      const emailInput = screen.getByLabelText(/e-mail/i);
      const passwordInput = screen.getByLabelText(/senha/i);
      
      expect(emailInput).toHaveAttribute('autocomplete', 'email');
      expect(passwordInput).toHaveAttribute('autocomplete', 'current-password');
    });
  });
  
  describe('Validação', () => {
    it('deve exibir erro quando e-mail está vazio', async () => {
      const user = userEvent.setup();
      renderLoginForm();
      
      const submitButton = screen.getByRole('button', { name: /entrar/i });
      await user.click(submitButton);
      
      await waitFor(() => {
        expect(screen.getByText(/e-mail é obrigatório/i)).toBeInTheDocument();
      });
    });
    
    it('deve exibir erro quando e-mail é inválido', async () => {
      const user = userEvent.setup();
      renderLoginForm();
      
      const emailInput = screen.getByLabelText(/e-mail/i);
      const passwordInput = screen.getByLabelText(/senha/i);
      const submitButton = screen.getByRole('button', { name: /entrar/i });
      
      // Primeiro preencher o campo com texto válido para passar validação HTML5
      await user.type(emailInput, 'email-invalido');
      // Forçar o campo a aceitar valor inválido removendo o type="email"
      emailInput.setAttribute('type', 'text');
      await user.clear(emailInput);
      await user.type(emailInput, 'email-invalido');
      await user.type(passwordInput, 'senha123');
      await user.click(submitButton);
      
      await waitFor(() => {
        expect(screen.getByText(/e-mail inválido/i)).toBeInTheDocument();
      });
    });
    
    it('deve exibir erro quando senha está vazia', async () => {
      const user = userEvent.setup();
      renderLoginForm();
      
      const emailInput = screen.getByLabelText(/e-mail/i);
      const submitButton = screen.getByRole('button', { name: /entrar/i });
      
      await user.type(emailInput, 'barbeiro@example.com');
      await user.click(submitButton);
      
      await waitFor(() => {
        expect(screen.getByText(/senha é obrigatória/i)).toBeInTheDocument();
      });
    });
    
    it('deve exibir erro quando senha tem menos de 6 caracteres', async () => {
      const user = userEvent.setup();
      renderLoginForm();
      
      const emailInput = screen.getByLabelText(/e-mail/i);
      const passwordInput = screen.getByLabelText(/senha/i);
      const submitButton = screen.getByRole('button', { name: /entrar/i });
      
      await user.type(emailInput, 'barbeiro@example.com');
      await user.type(passwordInput, '12345');
      await user.click(submitButton);
      
      await waitFor(() => {
        expect(screen.getByText(/senha deve ter no mínimo 6 caracteres/i)).toBeInTheDocument();
      });
    });
    
    it('deve adicionar borda vermelha em campos com erro', async () => {
      const user = userEvent.setup();
      renderLoginForm();
      
      const submitButton = screen.getByRole('button', { name: /entrar/i });
      await user.click(submitButton);
      
      await waitFor(() => {
        const emailInput = screen.getByLabelText(/e-mail/i);
        const passwordInput = screen.getByLabelText(/senha/i);
        
        expect(emailInput).toHaveClass('border-red-500');
        expect(passwordInput).toHaveClass('border-red-500');
      });
    });
  });
  
  describe('Submissão', () => {
    it('deve chamar login com dados corretos quando formulário é válido', async () => {
      const user = userEvent.setup();
      mockLogin.mockResolvedValueOnce(undefined);
      renderLoginForm();
      
      const emailInput = screen.getByLabelText(/e-mail/i);
      const passwordInput = screen.getByLabelText(/senha/i);
      const submitButton = screen.getByRole('button', { name: /entrar/i });
      
      await user.type(emailInput, 'barbeiro@example.com');
      await user.type(passwordInput, 'senha123');
      await user.click(submitButton);
      
      await waitFor(() => {
        expect(mockLogin).toHaveBeenCalledWith({
          email: 'barbeiro@example.com',
          password: 'senha123',
        });
      });
    });
    
    it('deve exibir estado de loading durante submissão', async () => {
      const user = userEvent.setup();
      mockLogin.mockImplementation(() => new Promise(resolve => setTimeout(resolve, 1000)));
      renderLoginForm();
      
      const emailInput = screen.getByLabelText(/e-mail/i);
      const passwordInput = screen.getByLabelText(/senha/i);
      const submitButton = screen.getByRole('button', { name: /entrar/i });
      
      await user.type(emailInput, 'barbeiro@example.com');
      await user.type(passwordInput, 'senha123');
      await user.click(submitButton);
      
      // Durante loading
      await waitFor(() => {
        expect(screen.getByText(/entrando\.\.\./i)).toBeInTheDocument();
      });
      
      // Campos desabilitados
      expect(emailInput).toBeDisabled();
      expect(passwordInput).toBeDisabled();
      expect(submitButton).toBeDisabled();
    });
    
    it('deve desabilitar campos durante submissão', async () => {
      const user = userEvent.setup();
      mockLogin.mockImplementation(() => new Promise(resolve => setTimeout(resolve, 1000)));
      renderLoginForm();
      
      const emailInput = screen.getByLabelText(/e-mail/i);
      const passwordInput = screen.getByLabelText(/senha/i);
      const submitButton = screen.getByRole('button', { name: /entrar/i });
      
      await user.type(emailInput, 'barbeiro@example.com');
      await user.type(passwordInput, 'senha123');
      await user.click(submitButton);
      
      await waitFor(() => {
        expect(emailInput).toBeDisabled();
        expect(passwordInput).toBeDisabled();
        expect(submitButton).toBeDisabled();
      });
    });
  });
  
  describe('Tratamento de Erros', () => {
    it('deve tratar erro 401 (credenciais inválidas)', async () => {
      const user = userEvent.setup();
      const error = {
        response: { status: 401 },
      };
      mockLogin.mockRejectedValueOnce(error);
      renderLoginForm();
      
      const emailInput = screen.getByLabelText(/e-mail/i);
      const passwordInput = screen.getByLabelText(/senha/i);
      const submitButton = screen.getByRole('button', { name: /entrar/i });
      
      await user.type(emailInput, 'barbeiro@example.com');
      await user.type(passwordInput, 'senha-errada');
      await user.click(submitButton);
      
      await waitFor(() => {
        expect(mockLogin).toHaveBeenCalled();
      });
      
      // O erro é tratado pelo hook useToast
      expect(mockLogin).toHaveBeenCalledTimes(1);
    });
    
    it('deve tratar erro 400 (dados inválidos)', async () => {
      const user = userEvent.setup();
      const error = {
        response: { status: 400 },
      };
      mockLogin.mockRejectedValueOnce(error);
      renderLoginForm();
      
      const emailInput = screen.getByLabelText(/e-mail/i);
      const passwordInput = screen.getByLabelText(/senha/i);
      const submitButton = screen.getByRole('button', { name: /entrar/i });
      
      await user.type(emailInput, 'barbeiro@example.com');
      await user.type(passwordInput, 'senha123');
      await user.click(submitButton);
      
      await waitFor(() => {
        expect(mockLogin).toHaveBeenCalled();
      });
    });
    
    it('deve tratar erro de conexão (erro genérico)', async () => {
      const user = userEvent.setup();
      const error = new Error('Network error');
      mockLogin.mockRejectedValueOnce(error);
      renderLoginForm();
      
      const emailInput = screen.getByLabelText(/e-mail/i);
      const passwordInput = screen.getByLabelText(/senha/i);
      const submitButton = screen.getByRole('button', { name: /entrar/i });
      
      await user.type(emailInput, 'barbeiro@example.com');
      await user.type(passwordInput, 'senha123');
      await user.click(submitButton);
      
      await waitFor(() => {
        expect(mockLogin).toHaveBeenCalled();
      });
    });
    
    it('não deve limpar campos após erro de autenticação', async () => {
      const user = userEvent.setup();
      const error = {
        response: { status: 401 },
      };
      mockLogin.mockRejectedValueOnce(error);
      renderLoginForm();
      
      const emailInput = screen.getByLabelText(/e-mail/i) as HTMLInputElement;
      const passwordInput = screen.getByLabelText(/senha/i) as HTMLInputElement;
      const submitButton = screen.getByRole('button', { name: /entrar/i });
      
      await user.type(emailInput, 'barbeiro@example.com');
      await user.type(passwordInput, 'senha-errada');
      await user.click(submitButton);
      
      await waitFor(() => {
        expect(mockLogin).toHaveBeenCalled();
      });
      
      // Campos devem manter os valores
      expect(emailInput.value).toBe('barbeiro@example.com');
      expect(passwordInput.value).toBe('senha-errada');
    });
  });
});
