/**
 * BarberHeader Component Tests
 */

import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BarberHeader } from '../BarberHeader';
import { AuthContext } from '@/contexts/AuthContext';
import type { AuthContextType, User } from '@/types/auth.types';

// Mock do contexto de autenticação
const mockUser: User = {
  id: '123',
  name: 'João Silva',
  email: 'joao@example.com',
  role: 'Barbeiro',
  barbeariaId: 'barbearia-123',
  nomeBarbearia: 'Barbearia Top',
};

const mockAuthContext: AuthContextType = {
  user: mockUser,
  isAuthenticated: true,
  isLoading: false,
  login: vi.fn(),
  logout: vi.fn(),
  validateSession: vi.fn(),
};

const renderWithAuth = (authValue: Partial<AuthContextType> = {}) => {
  const contextValue = { ...mockAuthContext, ...authValue };
  return render(
    <AuthContext.Provider value={contextValue}>
      <BarberHeader />
    </AuthContext.Provider>
  );
};

describe('BarberHeader', () => {
  describe('Renderização', () => {
    it('should render barber name', () => {
      renderWithAuth();
      expect(screen.getByText('João Silva')).toBeInTheDocument();
    });

    it('should render barbershop name', () => {
      renderWithAuth();
      expect(screen.getByText('Barbearia Top')).toBeInTheDocument();
    });

    it('should render logout button', () => {
      renderWithAuth();
      expect(screen.getByRole('button', { name: /sair/i })).toBeInTheDocument();
    });

    it('should render logout icon', () => {
      renderWithAuth();
      const button = screen.getByRole('button', { name: /sair/i });
      expect(button.querySelector('svg')).toBeInTheDocument();
    });

    it('should not render when user is null', () => {
      const { container } = renderWithAuth({ user: null });
      expect(container.firstChild).toBeNull();
    });
  });

  describe('Interações', () => {
    it('should call logout when logout button is clicked', async () => {
      const mockLogout = vi.fn();
      renderWithAuth({ logout: mockLogout });

      const logoutButton = screen.getByRole('button', { name: /sair/i });
      await userEvent.click(logoutButton);

      expect(mockLogout).toHaveBeenCalledTimes(1);
    });
  });

  describe('Responsividade', () => {
    it('should have hidden text on small screens', () => {
      renderWithAuth();
      const button = screen.getByRole('button', { name: /sair/i });
      const span = button.querySelector('span');
      
      expect(span).toHaveClass('hidden');
      expect(span).toHaveClass('sm:inline');
    });
  });

  describe('Estilos', () => {
    it('should have sticky header with proper classes', () => {
      const { container } = renderWithAuth();
      const header = container.querySelector('header');
      
      expect(header).toHaveClass('sticky');
      expect(header).toHaveClass('top-0');
      expect(header).toHaveClass('z-10');
    });

    it('should have border and shadow', () => {
      const { container } = renderWithAuth();
      const header = container.querySelector('header');
      
      expect(header).toHaveClass('border-b');
      expect(header).toHaveClass('shadow-sm');
    });

    it('should have white background', () => {
      const { container } = renderWithAuth();
      const header = container.querySelector('header');
      
      expect(header).toHaveClass('bg-white');
    });
  });

  describe('Layout', () => {
    it('should display name with larger font', () => {
      renderWithAuth();
      const name = screen.getByText('João Silva');
      
      expect(name).toHaveClass('text-xl');
      expect(name).toHaveClass('font-semibold');
    });

    it('should display barbershop name with smaller font', () => {
      renderWithAuth();
      const barbershopName = screen.getByText('Barbearia Top');
      
      expect(barbershopName).toHaveClass('text-sm');
      expect(barbershopName).toHaveClass('text-gray-600');
    });

    it('should have flex layout with space between', () => {
      const { container } = renderWithAuth();
      const contentDiv = container.querySelector('.flex.items-center.justify-between');
      
      expect(contentDiv).toBeInTheDocument();
    });
  });

  describe('Acessibilidade', () => {
    it('should have proper heading hierarchy', () => {
      renderWithAuth();
      const heading = screen.getByRole('heading', { level: 1 });
      
      expect(heading).toHaveTextContent('João Silva');
    });

    it('should have header landmark', () => {
      renderWithAuth();
      const header = screen.getByRole('banner');
      
      expect(header).toBeInTheDocument();
    });
  });

  describe('Diferentes Usuários', () => {
    it('should render with different user name', () => {
      const differentUser: User = {
        ...mockUser,
        name: 'Maria Santos',
        nomeBarbearia: 'Barbearia Elite',
      };

      renderWithAuth({ user: differentUser });

      expect(screen.getByText('Maria Santos')).toBeInTheDocument();
      expect(screen.getByText('Barbearia Elite')).toBeInTheDocument();
    });

    it('should handle long names gracefully', () => {
      const longNameUser: User = {
        ...mockUser,
        name: 'João Pedro da Silva Santos Oliveira',
        nomeBarbearia: 'Barbearia Premium de Alta Qualidade',
      };

      renderWithAuth({ user: longNameUser });

      expect(screen.getByText('João Pedro da Silva Santos Oliveira')).toBeInTheDocument();
      expect(screen.getByText('Barbearia Premium de Alta Qualidade')).toBeInTheDocument();
    });
  });
});
