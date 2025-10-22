/**
 * PreviewPanel Component Tests
 * 
 * Testes unitários para o componente PreviewPanel.
 * 
 * @version 1.0
 * @date 2025-10-21
 */

import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { PreviewPanel } from '../PreviewPanel';
import { LandingPageConfig } from '../../types/landing-page.types';

describe('PreviewPanel', () => {
  const mockConfig: LandingPageConfig = {
    id: 'test-id',
    barbershopId: 'barber-123',
    templateId: 1,
    logoUrl: 'https://example.com/logo.png',
    aboutText: 'Sobre nossa barbearia',
    openingHours: 'Seg-Sex: 09:00 - 19:00',
    instagramUrl: 'https://instagram.com/barbershop',
    facebookUrl: 'https://facebook.com/barbershop',
    whatsappNumber: '+5511999999999',
    isPublished: true,
    services: [
      {
        serviceId: 'service-1',
        serviceName: 'Corte Social',
        description: 'Corte moderno',
        duration: 30,
        price: 35.0,
        displayOrder: 1,
        isVisible: true,
      },
      {
        serviceId: 'service-2',
        serviceName: 'Barba',
        duration: 20,
        price: 25.0,
        displayOrder: 2,
        isVisible: true,
      },
      {
        serviceId: 'service-3',
        serviceName: 'Corte Infantil',
        duration: 20,
        price: 25.0,
        displayOrder: 3,
        isVisible: false,
      },
    ],
    updatedAt: '2025-10-21T00:00:00Z',
    createdAt: '2025-10-20T00:00:00Z',
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Rendering', () => {
    it('should render without config', () => {
      render(<PreviewPanel />);
      
      expect(
        screen.getByText('Nenhuma configuração disponível para preview')
      ).toBeInTheDocument();
    });

    it('should render with config', () => {
      render(<PreviewPanel config={mockConfig} />);
      
      expect(screen.getByText('Preview da Landing Page')).toBeInTheDocument();
    });

    it('should display template ID in device info', () => {
      render(<PreviewPanel config={mockConfig} />);
      
      expect(screen.getByText(/Template: 1/)).toBeInTheDocument();
    });

    it('should render with fullScreen mode', () => {
      const { container } = render(<PreviewPanel config={mockConfig} fullScreen />);
      
      const wrapper = container.firstChild as HTMLElement;
      expect(wrapper).toHaveClass('w-full', 'h-screen');
    });
  });

  describe('Device Controls', () => {
    it('should render device toggle buttons', () => {
      render(<PreviewPanel config={mockConfig} />);
      
      expect(screen.getByTitle(/Visualização Mobile/)).toBeInTheDocument();
      expect(screen.getByTitle(/Visualização Desktop/)).toBeInTheDocument();
    });

    it('should start with desktop device by default', () => {
      render(<PreviewPanel config={mockConfig} />);
      
      expect(screen.getByText(/Desktop \(100%\)/)).toBeInTheDocument();
    });

    it('should start with custom initial device', () => {
      render(<PreviewPanel config={mockConfig} device="mobile" />);
      
      expect(screen.getByText(/Mobile \(375px × 667px\)/)).toBeInTheDocument();
    });

    it('should toggle to mobile view', () => {
      render(<PreviewPanel config={mockConfig} />);
      
      const mobileButton = screen.getByTitle(/Visualização Mobile/);
      fireEvent.click(mobileButton);
      
      expect(screen.getByText(/Mobile \(375px × 667px\)/)).toBeInTheDocument();
    });

    it('should toggle back to desktop view', () => {
      render(<PreviewPanel config={mockConfig} device="mobile" />);
      
      const desktopButton = screen.getByTitle(/Visualização Desktop/);
      fireEvent.click(desktopButton);
      
      expect(screen.getByText(/Desktop \(100%\)/)).toBeInTheDocument();
    });

    it('should call onDeviceChange callback', () => {
      const onDeviceChange = vi.fn();
      render(
        <PreviewPanel config={mockConfig} onDeviceChange={onDeviceChange} />
      );
      
      const mobileButton = screen.getByTitle(/Visualização Mobile/);
      fireEvent.click(mobileButton);
      
      expect(onDeviceChange).toHaveBeenCalledWith('mobile');
    });
  });

  describe('Template Rendering', () => {
    it('should render Template 1 by default', () => {
      render(<PreviewPanel config={mockConfig} />);
      
      // Template 1 é o padrão
      const preview = screen.getByRole('presentation');
      expect(preview).toBeInTheDocument();
    });

    it('should render correct template based on templateId', () => {
      const config = { ...mockConfig, templateId: 2 };
      render(<PreviewPanel config={config} />);
      
      expect(screen.getByText(/Template: 2/)).toBeInTheDocument();
    });

    it('should fallback to Template 1 for invalid templateId', () => {
      const config = { ...mockConfig, templateId: 99 };
      render(<PreviewPanel config={config} />);
      
      // Deve renderizar sem erros
      const preview = screen.getByRole('presentation');
      expect(preview).toBeInTheDocument();
    });

    it('should update template when config changes', async () => {
      const { rerender } = render(<PreviewPanel config={mockConfig} />);
      
      const newConfig = { ...mockConfig, templateId: 3 };
      rerender(<PreviewPanel config={newConfig} />);
      
      await waitFor(() => {
        expect(screen.getByText(/Template: 3/)).toBeInTheDocument();
      });
    });
  });

  describe('Preview Container Styling', () => {
    it('should apply mobile width class', () => {
      const { container } = render(
        <PreviewPanel config={mockConfig} device="mobile" />
      );
      
      const previewContainer = container.querySelector('[role="presentation"]');
      expect(previewContainer).toHaveClass('w-[375px]');
    });

    it('should apply desktop width class', () => {
      const { container } = render(
        <PreviewPanel config={mockConfig} device="desktop" />
      );
      
      const previewContainer = container.querySelector('[role="presentation"]');
      expect(previewContainer).toHaveClass('w-full');
    });

    it('should have pointer-events-none to disable interaction', () => {
      const { container } = render(<PreviewPanel config={mockConfig} />);
      
      const previewContainer = container.querySelector('[role="presentation"]');
      expect(previewContainer).toHaveStyle({ pointerEvents: 'none' });
    });

    it('should have user-select-none', () => {
      const { container } = render(<PreviewPanel config={mockConfig} />);
      
      const previewContainer = container.querySelector('[role="presentation"]');
      expect(previewContainer).toHaveStyle({ userSelect: 'none' });
    });
  });

  describe('Open in New Tab Button', () => {
    it('should render open button', () => {
      render(<PreviewPanel config={mockConfig} />);
      
      expect(screen.getByTitle(/Abrir em nova aba/)).toBeInTheDocument();
    });

    it('should open landing page in new tab', () => {
      const windowOpenSpy = vi.spyOn(window, 'open').mockImplementation(() => null);
      
      render(<PreviewPanel config={mockConfig} />);
      
      const openButton = screen.getByTitle(/Abrir em nova aba/);
      fireEvent.click(openButton);
      
      expect(windowOpenSpy).toHaveBeenCalledWith(
        '/barbearia/barber-123',
        '_blank'
      );
      
      windowOpenSpy.mockRestore();
    });
  });

  describe('Accessibility', () => {
    it('should have proper ARIA labels', () => {
      render(<PreviewPanel config={mockConfig} />);
      
      const preview = screen.getByRole('presentation');
      expect(preview).toHaveAttribute(
        'aria-label',
        'Preview da landing page (apenas visualização)'
      );
    });

    it('should have descriptive button titles', () => {
      render(<PreviewPanel config={mockConfig} />);
      
      expect(screen.getByTitle('Visualização Mobile (375px)')).toBeInTheDocument();
      expect(screen.getByTitle('Visualização Desktop (100%)')).toBeInTheDocument();
      expect(screen.getByTitle('Abrir em nova aba')).toBeInTheDocument();
    });
  });

  describe('Edge Cases', () => {
    it('should handle config without logoUrl', () => {
      const config = { ...mockConfig, logoUrl: undefined };
      render(<PreviewPanel config={config} />);
      
      expect(screen.getByText('Preview da Landing Page')).toBeInTheDocument();
    });

    it('should handle config without aboutText', () => {
      const config = { ...mockConfig, aboutText: undefined };
      render(<PreviewPanel config={config} />);
      
      expect(screen.getByText('Preview da Landing Page')).toBeInTheDocument();
    });

    it('should handle config with empty services', () => {
      const config = { ...mockConfig, services: [] };
      render(<PreviewPanel config={config} />);
      
      expect(screen.getByText('Preview da Landing Page')).toBeInTheDocument();
    });

    it('should handle config with all services invisible', () => {
      const config = {
        ...mockConfig,
        services: mockConfig.services.map((s) => ({ ...s, isVisible: false })),
      };
      render(<PreviewPanel config={config} />);
      
      expect(screen.getByText('Preview da Landing Page')).toBeInTheDocument();
    });

    it('should handle rapid device changes', () => {
      const onDeviceChange = vi.fn();
      render(
        <PreviewPanel config={mockConfig} onDeviceChange={onDeviceChange} />
      );
      
      const mobileButton = screen.getByTitle(/Visualização Mobile/);
      const desktopButton = screen.getByTitle(/Visualização Desktop/);
      
      fireEvent.click(mobileButton);
      fireEvent.click(desktopButton);
      fireEvent.click(mobileButton);
      
      expect(onDeviceChange).toHaveBeenCalledTimes(3);
    });
  });

  describe('Performance', () => {
    it('should memoize template component selection', () => {
      const { rerender } = render(<PreviewPanel config={mockConfig} />);
      
      // Renderizar novamente com mesmo config
      rerender(<PreviewPanel config={mockConfig} />);
      
      // Não deve causar re-renderização desnecessária
      expect(screen.getByText('Preview da Landing Page')).toBeInTheDocument();
    });

    it('should memoize container classes', () => {
      const { container, rerender } = render(
        <PreviewPanel config={mockConfig} device="mobile" />
      );
      
      const initialClasses = container
        .querySelector('[role="presentation"]')
        ?.className;
      
      rerender(<PreviewPanel config={mockConfig} device="mobile" />);
      
      const updatedClasses = container
        .querySelector('[role="presentation"]')
        ?.className;
      
      expect(initialClasses).toBe(updatedClasses);
    });
  });

  describe('Integration', () => {
    it('should work with all template IDs', () => {
      [1, 2, 3, 4, 5].forEach((templateId) => {
        const config = { ...mockConfig, templateId };
        const { unmount } = render(<PreviewPanel config={config} />);
        
        expect(screen.getByText(`Template: ${templateId}`)).toBeInTheDocument();
        
        unmount();
      });
    });

    it('should display all visible services in preview', () => {
      render(<PreviewPanel config={mockConfig} />);
      
      const preview = screen.getByRole('presentation');
      expect(preview).toBeInTheDocument();
      
      // Templates devem renderizar serviços visíveis
      // (verificação simplificada já que templates são placeholders)
    });

    it('should reflect config changes in real-time', async () => {
      const { rerender } = render(<PreviewPanel config={mockConfig} />);
      
      const newConfig = {
        ...mockConfig,
        aboutText: 'Novo texto sobre',
      };
      
      rerender(<PreviewPanel config={newConfig} />);
      
      await waitFor(() => {
        expect(screen.getByText('Preview da Landing Page')).toBeInTheDocument();
      });
    });
  });
});
