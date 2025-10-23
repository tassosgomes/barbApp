/**
 * LandingPageForm Component Tests
 * 
 * Testes unitários para o componente LandingPageForm.
 * Verifica validação, integração com hooks, e comportamento de submissão.
 * 
 * @version 1.0
 * @date 2025-10-22
 */

import { describe, it, expect, vi, beforeEach, Mock } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { LandingPageForm } from '../LandingPageForm';
import { useLandingPage } from '../../hooks/useLandingPage';
import type { LandingPageConfig, LandingPageService } from '../../types/landing-page.types';

// ============================================================================
// Mocks
// ============================================================================

vi.mock('../../hooks/useLandingPage');
vi.mock('../LogoUploader', () => ({
  LogoUploader: ({ barbershopId, currentLogoUrl, disabled }: { barbershopId: string; currentLogoUrl?: string; disabled?: boolean }) => (
    <div data-testid="logo-uploader">
      Logo Uploader - {barbershopId} - {currentLogoUrl || 'no-logo'} - {disabled ? 'disabled' : 'enabled'}
    </div>
  ),
}));
vi.mock('../ServiceManager', () => ({
  ServiceManager: ({ services, onChange, disabled }: { services: LandingPageService[]; onChange: (services: LandingPageService[]) => void; disabled?: boolean }) => (
    <div data-testid="service-manager">
      Service Manager - {services.length} services - {disabled ? 'disabled' : 'enabled'}
      <button onClick={() => onChange([...services, { serviceId: 'new', serviceName: 'New Service', duration: 30, price: 40, displayOrder: services.length + 1, isVisible: true }])}>
        Add Service
      </button>
    </div>
  ),
}));

// ============================================================================
// Test Data
// ============================================================================

const mockConfig: LandingPageConfig = {
  id: 'config-1',
  barbershopId: 'barbershop-1',
  templateId: 1,
  logoUrl: 'https://example.com/logo.png',
  aboutText: 'Uma barbearia tradicional',
  openingHours: 'Segunda a Sexta: 09:00 - 19:00',
  instagramUrl: 'https://instagram.com/barbearia',
  facebookUrl: 'https://facebook.com/barbearia',
  whatsappNumber: '+5511999999999',
  isPublished: true,
  services: [
    {
      serviceId: 'service-1',
      serviceName: 'Corte Social',
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
  ],
  updatedAt: '2025-10-22T10:00:00Z',
  createdAt: '2025-10-20T10:00:00Z',
};

// ============================================================================
// Test Helpers
// ============================================================================

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

const mockUseLandingPage = (overrides = {}) => {
  return {
    config: mockConfig,
    isLoading: false,
    isUpdating: false,
    updateConfig: vi.fn(),
    ...overrides,
  };
};

// ============================================================================
// Tests
// ============================================================================

describe('LandingPageForm', () => {
  let updateConfigMock: Mock;

  beforeEach(() => {
    vi.clearAllMocks();
    updateConfigMock = vi.fn();
    (useLandingPage as Mock).mockReturnValue(mockUseLandingPage({ updateConfig: updateConfigMock }));
  });

  describe('Rendering', () => {
    it('should render loading state', () => {
      (useLandingPage as Mock).mockReturnValue(mockUseLandingPage({ isLoading: true, config: undefined }));

      render(<LandingPageForm barbershopId="barbershop-1" />, { wrapper: createWrapper() });

      expect(screen.getByText(/carregando configuração/i)).toBeInTheDocument();
    });

    it('should render error state when config is not available', () => {
      (useLandingPage as Mock).mockReturnValue(mockUseLandingPage({ config: undefined }));

      render(<LandingPageForm barbershopId="barbershop-1" />, { wrapper: createWrapper() });

      expect(screen.getByText(/não foi possível carregar/i)).toBeInTheDocument();
    });

    it('should render all form fields', () => {
      render(<LandingPageForm barbershopId="barbershop-1" />, { wrapper: createWrapper() });

      expect(screen.getByLabelText(/sobre a barbearia/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/horário de funcionamento/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/whatsapp/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/instagram/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/facebook/i)).toBeInTheDocument();
    });

    it('should render LogoUploader component', () => {
      render(<LandingPageForm barbershopId="barbershop-1" />, { wrapper: createWrapper() });

      expect(screen.getByTestId('logo-uploader')).toBeInTheDocument();
    });

    it('should render ServiceManager component', () => {
      render(<LandingPageForm barbershopId="barbershop-1" />, { wrapper: createWrapper() });

      expect(screen.getByTestId('service-manager')).toBeInTheDocument();
    });

    it('should render action buttons', () => {
      render(<LandingPageForm barbershopId="barbershop-1" />, { wrapper: createWrapper() });

      expect(screen.getByRole('button', { name: /cancelar/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /salvar alterações/i })).toBeInTheDocument();
    });

    it('should not render action buttons in readonly mode', () => {
      render(<LandingPageForm barbershopId="barbershop-1" readonly />, { wrapper: createWrapper() });

      expect(screen.queryByRole('button', { name: /cancelar/i })).not.toBeInTheDocument();
      expect(screen.queryByRole('button', { name: /salvar alterações/i })).not.toBeInTheDocument();
    });
  });

  describe('Form Population', () => {
    it('should populate form with config data', async () => {
      render(<LandingPageForm barbershopId="barbershop-1" />, { wrapper: createWrapper() });

      await waitFor(() => {
        const aboutTextarea = screen.getByLabelText(/sobre a barbearia/i) as HTMLTextAreaElement;
        expect(aboutTextarea.value).toBe(mockConfig.aboutText);

        const hoursTextarea = screen.getByLabelText(/horário de funcionamento/i) as HTMLTextAreaElement;
        expect(hoursTextarea.value).toBe(mockConfig.openingHours);

        const whatsappInput = screen.getByLabelText(/whatsapp/i) as HTMLInputElement;
        expect(whatsappInput.value).toBe(mockConfig.whatsappNumber);

        const instagramInput = screen.getByLabelText(/instagram/i) as HTMLInputElement;
        expect(instagramInput.value).toBe(mockConfig.instagramUrl);

        const facebookInput = screen.getByLabelText(/facebook/i) as HTMLInputElement;
        expect(facebookInput.value).toBe(mockConfig.facebookUrl);
      });
    });
  });

  describe('Validation', () => {
    it('should show aboutText character counter', async () => {
      const user = userEvent.setup();
      render(<LandingPageForm barbershopId="barbershop-1" />, { wrapper: createWrapper() });

      const aboutTextarea = screen.getByLabelText(/sobre a barbearia/i);

      await user.clear(aboutTextarea);
      await user.type(aboutTextarea, 'Test');

      await waitFor(() => {
        expect(screen.getByText('4/1000 caracteres')).toBeInTheDocument();
      });
    });

    it('should show openingHours character counter', async () => {
      const user = userEvent.setup();
      render(<LandingPageForm barbershopId="barbershop-1" />, { wrapper: createWrapper() });

      const hoursTextarea = screen.getByLabelText(/horário de funcionamento/i);

      await user.clear(hoursTextarea);
      await user.type(hoursTextarea, 'Test');

      await waitFor(() => {
        expect(screen.getByText('4/500 caracteres')).toBeInTheDocument();
      });
    });

    it('should validate WhatsApp format', async () => {
      const user = userEvent.setup();
      render(<LandingPageForm barbershopId="barbershop-1" />, { wrapper: createWrapper() });

      const whatsappInput = screen.getByLabelText(/whatsapp/i);

      await user.clear(whatsappInput);
      await user.type(whatsappInput, 'invalid-number');
      await user.click(screen.getByRole('button', { name: /salvar alterações/i }));

      await waitFor(() => {
        expect(screen.getByText(/formato inválido/i)).toBeInTheDocument();
      });
    });

    it('should accept empty optional fields', async () => {
      const user = userEvent.setup();
      render(<LandingPageForm barbershopId="barbershop-1" />, { wrapper: createWrapper() });

      const aboutTextarea = screen.getByLabelText(/sobre a barbearia/i);
      const instagramInput = screen.getByLabelText(/instagram/i);

      await user.clear(aboutTextarea);
      await user.clear(instagramInput);
      await user.click(screen.getByRole('button', { name: /salvar alterações/i }));

      await waitFor(() => {
        expect(updateConfigMock).toHaveBeenCalled();
      });
    });
  });

  describe('Form Submission', () => {
    it('should submit form with valid data', async () => {
      const user = userEvent.setup();
      render(<LandingPageForm barbershopId="barbershop-1" />, { wrapper: createWrapper() });

      const aboutTextarea = screen.getByLabelText(/sobre a barbearia/i);
      await user.clear(aboutTextarea);
      await user.type(aboutTextarea, 'Nova descrição');

      await user.click(screen.getByRole('button', { name: /salvar alterações/i }));

      await waitFor(() => {
        expect(updateConfigMock).toHaveBeenCalledWith(
          expect.objectContaining({
            aboutText: 'Nova descrição',
            openingHours: mockConfig.openingHours,
            whatsappNumber: mockConfig.whatsappNumber,
            instagramUrl: mockConfig.instagramUrl,
            facebookUrl: mockConfig.facebookUrl,
            services: expect.arrayContaining([
              expect.objectContaining({
                serviceId: 'service-1',
                displayOrder: 1,
                isVisible: true,
              }),
              expect.objectContaining({
                serviceId: 'service-2',
                displayOrder: 2,
                isVisible: true,
              }),
            ]),
          })
        );
      });
    });

    it('should include updated services in submission', async () => {
      const user = userEvent.setup();
      render(<LandingPageForm barbershopId="barbershop-1" />, { wrapper: createWrapper() });

      // Simula adicionar serviço através do ServiceManager
      const addServiceButton = screen.getByText('Add Service');
      await user.click(addServiceButton);

      await user.click(screen.getByRole('button', { name: /salvar alterações/i }));

      await waitFor(() => {
        const calls = updateConfigMock.mock.calls;
        const lastCall = calls[calls.length - 1][0];
        
        expect(lastCall.services).toHaveLength(3);
        expect(lastCall.services[2]).toEqual(
          expect.objectContaining({
            serviceId: 'new',
            displayOrder: 3,
          })
        );
      });
    });

    it('should disable form during submission', async () => {
      (useLandingPage as Mock).mockReturnValue(
        mockUseLandingPage({ isUpdating: true, updateConfig: updateConfigMock })
      );

      render(<LandingPageForm barbershopId="barbershop-1" />, { wrapper: createWrapper() });

      const aboutTextarea = screen.getByLabelText(/sobre a barbearia/i) as HTMLTextAreaElement;
      const submitButton = screen.getByRole('button', { name: /salvando/i }) as HTMLButtonElement;

      expect(aboutTextarea.disabled).toBe(true);
      expect(submitButton.disabled).toBe(true);
      expect(screen.getByText(/salvando/i)).toBeInTheDocument();
    });

    it('should track updating state when form is submitted', async () => {
      const user = userEvent.setup();

      render(
        <LandingPageForm barbershopId="barbershop-1" />,
        { wrapper: createWrapper() }
      );

      await user.click(screen.getByRole('button', { name: /salvar alterações/i }));

      await waitFor(() => {
        expect(updateConfigMock).toHaveBeenCalled();
      });
    });
  });

  describe('Cancel Button', () => {
    it('should reset form to original values on cancel', async () => {
      const user = userEvent.setup();
      render(<LandingPageForm barbershopId="barbershop-1" />, { wrapper: createWrapper() });

      const aboutTextarea = screen.getByLabelText(/sobre a barbearia/i) as HTMLTextAreaElement;

      // Modifica campo
      await user.clear(aboutTextarea);
      await user.type(aboutTextarea, 'Texto modificado');

      expect(aboutTextarea.value).toBe('Texto modificado');

      // Clica em cancelar
      await user.click(screen.getByRole('button', { name: /cancelar/i }));

      // Verifica que voltou ao valor original
      await waitFor(() => {
        expect(aboutTextarea.value).toBe(mockConfig.aboutText);
      });
    });

    it('should call onCancel callback when cancel button is clicked', async () => {
      const onCancel = vi.fn();
      const user = userEvent.setup();

      render(<LandingPageForm barbershopId="barbershop-1" onCancel={onCancel} />, {
        wrapper: createWrapper(),
      });

      await user.click(screen.getByRole('button', { name: /cancelar/i }));

      expect(onCancel).toHaveBeenCalled();
    });
  });

  describe('Character Counter', () => {
    it('should display character counter for aboutText', async () => {
      const user = userEvent.setup();
      render(<LandingPageForm barbershopId="barbershop-1" />, { wrapper: createWrapper() });

      const aboutTextarea = screen.getByLabelText(/sobre a barbearia/i);

      await user.clear(aboutTextarea);
      await user.type(aboutTextarea, 'Test');

      await waitFor(() => {
        expect(screen.getByText('4/1000 caracteres')).toBeInTheDocument();
      });
    });

    it('should display character counter for openingHours', async () => {
      const user = userEvent.setup();
      render(<LandingPageForm barbershopId="barbershop-1" />, { wrapper: createWrapper() });

      const hoursTextarea = screen.getByLabelText(/horário de funcionamento/i);

      await user.clear(hoursTextarea);
      await user.type(hoursTextarea, 'Test');

      await waitFor(() => {
        expect(screen.getByText('4/500 caracteres')).toBeInTheDocument();
      });
    });
  });

  describe('Readonly Mode', () => {
    it('should disable all fields in readonly mode', () => {
      render(<LandingPageForm barbershopId="barbershop-1" readonly />, {
        wrapper: createWrapper(),
      });

      const aboutTextarea = screen.getByLabelText(/sobre a barbearia/i) as HTMLTextAreaElement;
      const hoursTextarea = screen.getByLabelText(/horário de funcionamento/i) as HTMLTextAreaElement;
      const whatsappInput = screen.getByLabelText(/whatsapp/i) as HTMLInputElement;

      expect(aboutTextarea.disabled).toBe(true);
      expect(hoursTextarea.disabled).toBe(true);
      expect(whatsappInput.disabled).toBe(true);
    });

    it('should pass disabled prop to child components', () => {
      render(<LandingPageForm barbershopId="barbershop-1" readonly />, {
        wrapper: createWrapper(),
      });

      expect(screen.getByTestId('logo-uploader')).toHaveTextContent('disabled');
      expect(screen.getByTestId('service-manager')).toHaveTextContent('disabled');
    });
  });
});
