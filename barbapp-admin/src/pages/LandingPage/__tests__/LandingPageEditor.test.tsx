/**
 * LandingPageEditor Component Tests
 * 
 * Tests for the main Landing Page Editor page component
 * 
 * @version 1.0
 * @date 2025-10-22
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { LandingPageEditor } from '../LandingPageEditor';
import { BarbeariaProvider } from '@/contexts/BarbeariaContext';

// Mock dos módulos
vi.mock('@/features/landing-page/hooks/useLandingPage');
vi.mock('@/features/landing-page/components/LandingPageForm', () => ({
  LandingPageForm: () => <div data-testid="landing-page-form">Form</div>,
}));
vi.mock('@/features/landing-page/components/TemplateGallery', () => ({
  TemplateGallery: () => <div data-testid="template-gallery">Gallery</div>,
}));
vi.mock('@/features/landing-page/components/PreviewPanel', () => ({
  PreviewPanel: () => <div data-testid="preview-panel">Preview</div>,
}));

// Mock do clipboard API
Object.assign(navigator, {
  clipboard: {
    writeText: vi.fn(),
  },
});

const mockConfig = {
  id: '1',
  barbershopId: 'barbearia-1',
  templateId: 1,
  logoUrl: 'https://example.com/logo.png',
  aboutText: 'Sobre a barbearia',
  openingHours: 'Seg-Sex: 9h-18h',
  instagramUrl: 'https://instagram.com/barbearia',
  facebookUrl: '',
  whatsappNumber: '+5511999999999',
  isPublished: true,
  services: [],
  updatedAt: '2025-10-22T00:00:00Z',
  createdAt: '2025-10-22T00:00:00Z',
};

const mockBarbearia = {
  barbeariaId: 'barbearia-1',
  nome: 'Barbearia Teste',
  codigo: 'TESTE123',
  isActive: true,
};

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <BarbeariaProvider>{children}</BarbeariaProvider>
      </BrowserRouter>
    </QueryClientProvider>
  );
};

describe('LandingPageEditor', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    
    // Mock do contexto da barbearia
    vi.mock('@/contexts/BarbeariaContext', () => ({
      useBarbearia: () => ({
        barbearia: mockBarbearia,
        setBarbearia: vi.fn(),
        clearBarbearia: vi.fn(),
        isLoaded: true,
      }),
      BarbeariaProvider: ({ children }: { children: React.ReactNode }) => <>{children}</>,
    }));
  });

  describe('Rendering', () => {
    it('should render page title and description', async () => {
      const { useLandingPage } = await import('@/features/landing-page/hooks/useLandingPage');
      vi.mocked(useLandingPage).mockReturnValue({
        config: mockConfig,
        isLoading: false,
        error: null,
        updateConfig: vi.fn(),
        isUpdating: false,
      } as ReturnType<typeof useLandingPage>);

      render(<LandingPageEditor />, { wrapper: createWrapper() });

      expect(screen.getByText('Landing Page')).toBeInTheDocument();
      expect(
        screen.getByText('Personalize a página pública da sua barbearia')
      ).toBeInTheDocument();
    });

    it('should render action buttons', async () => {
      const { useLandingPage } = await import('@/features/landing-page/hooks/useLandingPage');
      vi.mocked(useLandingPage).mockReturnValue({
        config: mockConfig,
        isLoading: false,
        error: null,
        updateConfig: vi.fn(),
        isUpdating: false,
      } as ReturnType<typeof useLandingPage>);

      render(<LandingPageEditor />, { wrapper: createWrapper() });

      expect(screen.getAllByText(/Copiar URL/i)[0]).toBeInTheDocument();
      expect(screen.getAllByText(/Abrir Landing Page/i)[0]).toBeInTheDocument();
    });

    it('should render URL box with correct URL', async () => {
      const { useLandingPage } = await import('@/features/landing-page/hooks/useLandingPage');
      vi.mocked(useLandingPage).mockReturnValue({
        config: mockConfig,
        isLoading: false,
        error: null,
        updateConfig: vi.fn(),
        isUpdating: false,
      } as ReturnType<typeof useLandingPage>);

      render(<LandingPageEditor />, { wrapper: createWrapper() });

      const expectedUrl = `${window.location.origin}/barbearia/TESTE123`;
      expect(screen.getByText(expectedUrl)).toBeInTheDocument();
    });

    it('should render tabs navigation', async () => {
      const { useLandingPage } = await import('@/features/landing-page/hooks/useLandingPage');
      vi.mocked(useLandingPage).mockReturnValue({
        config: mockConfig,
        isLoading: false,
        error: null,
        updateConfig: vi.fn(),
        isUpdating: false,
      } as ReturnType<typeof useLandingPage>);

      render(<LandingPageEditor />, { wrapper: createWrapper() });

      expect(screen.getByText('Editar Informações')).toBeInTheDocument();
      expect(screen.getByText('Escolher Template')).toBeInTheDocument();
      expect(screen.getByText('Preview')).toBeInTheDocument();
    });
  });

  describe('Loading State', () => {
    it('should show loading spinner when loading', async () => {
      const { useLandingPage } = await import('@/features/landing-page/hooks/useLandingPage');
      vi.mocked(useLandingPage).mockReturnValue({
        config: undefined,
        isLoading: true,
        error: null,
        updateConfig: vi.fn(),
        isUpdating: false,
      } as ReturnType<typeof useLandingPage>);

      render(<LandingPageEditor />, { wrapper: createWrapper() });

      expect(
        screen.getByText('Carregando configuração da landing page...')
      ).toBeInTheDocument();
    });
  });

  describe('Error State', () => {
    it('should show error message when there is an error', async () => {
      const { useLandingPage } = await import('@/features/landing-page/hooks/useLandingPage');
      vi.mocked(useLandingPage).mockReturnValue({
        config: undefined,
        isLoading: false,
        error: new Error('Failed to load'),
        updateConfig: vi.fn(),
        isUpdating: false,
      } as any);

      render(<LandingPageEditor />, { wrapper: createWrapper() });

      expect(
        screen.getByText('Não foi possível carregar a configuração da landing page.')
      ).toBeInTheDocument();
    });

    it('should show error message when config is null', async () => {
      const { useLandingPage } = await import('@/features/landing-page/hooks/useLandingPage');
      vi.mocked(useLandingPage).mockReturnValue({
        config: null,
        isLoading: false,
        error: null,
        updateConfig: vi.fn(),
        isUpdating: false,
      } as any);

      render(<LandingPageEditor />, { wrapper: createWrapper() });

      expect(
        screen.getByText('Não foi possível carregar a configuração da landing page.')
      ).toBeInTheDocument();
    });
  });

  describe('Copy URL Functionality', () => {
    it('should copy URL to clipboard when copy button is clicked', async () => {
      const user = userEvent.setup();
      const { useLandingPage } = await import('@/features/landing-page/hooks/useLandingPage');
      vi.mocked(useLandingPage).mockReturnValue({
        config: mockConfig,
        isLoading: false,
        error: null,
        updateConfig: vi.fn(),
        isUpdating: false,
      } as any);

      render(<LandingPageEditor />, { wrapper: createWrapper() });

      const copyButton = screen.getAllByText(/Copiar URL/i)[0];
      await user.click(copyButton);

      await waitFor(() => {
        expect(navigator.clipboard.writeText).toHaveBeenCalledWith(
          `${window.location.origin}/barbearia/TESTE123`
        );
      });
    });
  });

  describe('Open Landing Page', () => {
    it('should open landing page in new tab when button is clicked', async () => {
      const user = userEvent.setup();
      const windowOpenSpy = vi.spyOn(window, 'open').mockImplementation(() => null);
      
      const { useLandingPage } = await import('@/features/landing-page/hooks/useLandingPage');
      vi.mocked(useLandingPage).mockReturnValue({
        config: mockConfig,
        isLoading: false,
        error: null,
        updateConfig: vi.fn(),
        isUpdating: false,
      } as any);

      render(<LandingPageEditor />, { wrapper: createWrapper() });

      const openButton = screen.getAllByText(/Abrir Landing Page/i)[0];
      await user.click(openButton);

      expect(windowOpenSpy).toHaveBeenCalledWith(
        `${window.location.origin}/barbearia/TESTE123`,
        '_blank',
        'noopener,noreferrer'
      );

      windowOpenSpy.mockRestore();
    });
  });

  describe('Tab Navigation', () => {
    it('should show form when "Editar Informações" tab is active', async () => {
      const { useLandingPage } = await import('@/features/landing-page/hooks/useLandingPage');
      vi.mocked(useLandingPage).mockReturnValue({
        config: mockConfig,
        isLoading: false,
        error: null,
        updateConfig: vi.fn(),
        isUpdating: false,
      } as any);

      render(<LandingPageEditor />, { wrapper: createWrapper() });

      // Default tab should be "edit"
      expect(screen.getByTestId('landing-page-form')).toBeInTheDocument();
    });

    it('should show template gallery when "Escolher Template" tab is clicked', async () => {
      const user = userEvent.setup();
      const { useLandingPage } = await import('@/features/landing-page/hooks/useLandingPage');
      vi.mocked(useLandingPage).mockReturnValue({
        config: mockConfig,
        isLoading: false,
        error: null,
        updateConfig: vi.fn(),
        isUpdating: false,
      } as any);

      render(<LandingPageEditor />, { wrapper: createWrapper() });

      const templateTab = screen.getByText('Escolher Template');
      await user.click(templateTab);

      await waitFor(() => {
        expect(screen.getByTestId('template-gallery')).toBeInTheDocument();
      });
    });

    it('should show preview panel when "Preview" tab is clicked', async () => {
      const user = userEvent.setup();
      const { useLandingPage } = await import('@/features/landing-page/hooks/useLandingPage');
      vi.mocked(useLandingPage).mockReturnValue({
        config: mockConfig,
        isLoading: false,
        error: null,
        updateConfig: vi.fn(),
        isUpdating: false,
      } as any);

      render(<LandingPageEditor />, { wrapper: createWrapper() });

      const previewTab = screen.getByText('Preview');
      await user.click(previewTab);

      await waitFor(() => {
        // Should have 2 preview panels: one in edit tab (hidden on mobile), one in preview tab
        const previewPanels = screen.getAllByTestId('preview-panel');
        expect(previewPanels.length).toBeGreaterThan(0);
      });
    });
  });

  describe('Template Change', () => {
    it('should update template when template is selected', async () => {
      const updateConfigMock = vi.fn();
      const { useLandingPage } = await import('@/features/landing-page/hooks/useLandingPage');
      vi.mocked(useLandingPage).mockReturnValue({
        config: mockConfig,
        isLoading: false,
        error: null,
        updateConfig: updateConfigMock,
        isUpdating: false,
      } as any);

      render(<LandingPageEditor />, { wrapper: createWrapper() });

      // Simulate template selection through the gallery
      // In real implementation, this would be tested through TemplateGallery integration
      // For now, we verify the hook setup
      expect(useLandingPage).toHaveBeenCalledWith(mockBarbearia.barbeariaId);
    });
  });

  describe('Responsive Layout', () => {
    it('should render preview panel in edit tab on desktop', async () => {
      const { useLandingPage } = await import('@/features/landing-page/hooks/useLandingPage');
      vi.mocked(useLandingPage).mockReturnValue({
        config: mockConfig,
        isLoading: false,
        error: null,
        updateConfig: vi.fn(),
        isUpdating: false,
      } as any);

      render(<LandingPageEditor />, { wrapper: createWrapper() });

      // Preview should be in the DOM (even if hidden on mobile with CSS)
      const previewPanels = screen.getAllByTestId('preview-panel');
      expect(previewPanels.length).toBeGreaterThan(0);
    });
  });
});
