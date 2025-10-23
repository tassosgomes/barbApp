import { render, screen } from '@testing-library/react';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { LandingPage } from './LandingPage';
import type { PublicLandingPage } from '@/types/landing-page.types';

// Mock the hook
const mockUseLandingPageData = vi.fn();
vi.mock('@/hooks/useLandingPageData', () => ({
  useLandingPageData: (...args: any[]) => mockUseLandingPageData(...args),
}));

// Mock templates
vi.mock('@/templates', () => ({
  Template1Classic: ({ data }: { data: PublicLandingPage }) => (
    <div data-testid="template-1">Template 1 Classic - {data.barbershop.name}</div>
  ),
  Template2Modern: ({ data }: { data: PublicLandingPage }) => (
    <div data-testid="template-2">Template 2 Modern - {data.barbershop.name}</div>
  ),
  Template3Vintage: ({ data }: { data: PublicLandingPage }) => (
    <div data-testid="template-3">Template 3 Vintage - {data.barbershop.name}</div>
  ),
  Template4Urban: ({ data }: { data: PublicLandingPage }) => (
    <div data-testid="template-4">Template 4 Urban - {data.barbershop.name}</div>
  ),
  Template5Premium: ({ data }: { data: PublicLandingPage }) => (
    <div data-testid="template-5">Template 5 Premium - {data.barbershop.name}</div>
  ),
}));

// Mock components
vi.mock('@/components', () => ({
  LoadingState: () => <div data-testid="loading">Loading...</div>,
  ErrorState: ({ title, message }: { title: string; message: string }) => (
    <div data-testid="error">
      <h1>{title}</h1>
      <p>{message}</p>
    </div>
  ),
}));

const mockData: PublicLandingPage = {
  barbershop: {
    id: '1',
    name: 'Barbearia Teste',
    code: 'TEST123',
    address: 'Rua Teste, 123',
  },
  landingPage: {
    templateId: 1,
    logoUrl: 'https://example.com/logo.png',
    aboutText: 'Sobre a barbearia',
    openingHours: '09:00 - 19:00',
    instagramUrl: 'https://instagram.com/test',
    facebookUrl: 'https://facebook.com/test',
    whatsappNumber: '+5511999999999',
    services: [
      {
        id: '1',
        name: 'Corte',
        description: 'Corte de cabelo',
        duration: 30,
        price: 35.0,
      },
    ],
  },
};

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  });

  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        {children}
      </BrowserRouter>
    </QueryClientProvider>
  );
};

describe('LandingPage', () => {
  const Wrapper = createWrapper();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('shows loading state while fetching data', () => {
    mockUseLandingPageData.mockReturnValue({
      data: undefined,
      isLoading: true,
      error: null,
    });

    render(
      <Wrapper>
        <LandingPage />
      </Wrapper>
    );

    expect(screen.getByTestId('loading')).toBeInTheDocument();
  });

  it('shows error state when API fails', () => {
    mockUseLandingPageData.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: new Error('API Error'),
    });

    render(
      <Wrapper>
        <LandingPage />
      </Wrapper>
    );

    expect(screen.getByTestId('error')).toBeInTheDocument();
    expect(screen.getByText('Landing page não encontrada')).toBeInTheDocument();
    expect(screen.getByText('Verifique o código e tente novamente.')).toBeInTheDocument();
  });

  it('renders Template1Classic for templateId 1', () => {
    mockUseLandingPageData.mockReturnValue({
      data: mockData,
      isLoading: false,
      error: null,
    });

    render(
      <Wrapper>
        <LandingPage />
      </Wrapper>
    );

    expect(screen.getByTestId('template-1')).toBeInTheDocument();
    expect(screen.getByText('Template 1 Classic - Barbearia Teste')).toBeInTheDocument();
  });

  it('renders Template2Modern for templateId 2', () => {
    const dataWithTemplate2 = { ...mockData, landingPage: { ...mockData.landingPage, templateId: 2 } };

    mockUseLandingPageData.mockReturnValue({
      data: dataWithTemplate2,
      isLoading: false,
      error: null,
    });

    render(
      <Wrapper>
        <LandingPage />
      </Wrapper>
    );

    expect(screen.getByTestId('template-2')).toBeInTheDocument();
  });

  it('renders Template3Vintage for templateId 3', () => {
    const dataWithTemplate3 = { ...mockData, landingPage: { ...mockData.landingPage, templateId: 3 } };

    mockUseLandingPageData.mockReturnValue({
      data: dataWithTemplate3,
      isLoading: false,
      error: null,
    });

    render(
      <Wrapper>
        <LandingPage />
      </Wrapper>
    );

    expect(screen.getByTestId('template-3')).toBeInTheDocument();
  });

  it('renders Template4Urban for templateId 4', () => {
    const dataWithTemplate4 = { ...mockData, landingPage: { ...mockData.landingPage, templateId: 4 } };

    mockUseLandingPageData.mockReturnValue({
      data: dataWithTemplate4,
      isLoading: false,
      error: null,
    });

    render(
      <Wrapper>
        <LandingPage />
      </Wrapper>
    );

    expect(screen.getByTestId('template-4')).toBeInTheDocument();
  });

  it('renders Template5Premium for templateId 5', () => {
    const dataWithTemplate5 = { ...mockData, landingPage: { ...mockData.landingPage, templateId: 5 } };

    mockUseLandingPageData.mockReturnValue({
      data: dataWithTemplate5,
      isLoading: false,
      error: null,
    });

    render(
      <Wrapper>
        <LandingPage />
      </Wrapper>
    );

    expect(screen.getByTestId('template-5')).toBeInTheDocument();
  });

  it('falls back to Template1Classic for invalid templateId', () => {
    const dataWithInvalidTemplate = { ...mockData, landingPage: { ...mockData.landingPage, templateId: 99 } };

    mockUseLandingPageData.mockReturnValue({
      data: dataWithInvalidTemplate,
      isLoading: false,
      error: null,
    });

    render(
      <Wrapper>
        <LandingPage />
      </Wrapper>
    );

    expect(screen.getByTestId('template-1')).toBeInTheDocument();
  });
});