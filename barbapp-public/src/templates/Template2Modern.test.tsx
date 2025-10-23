import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { Template2Modern } from './Template2Modern';
import type { PublicLandingPage } from '@/types/landing-page.types';

// Mock useNavigate
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

const mockData: PublicLandingPage = {
  barbershop: {
    id: '1',
    name: 'Barbearia Moderna',
    code: 'MOD123',
    address: 'Rua das Flores, 123 - Centro, São Paulo - SP',
  },
  landingPage: {
    templateId: 2,
    logoUrl: 'https://example.com/logo.png',
    aboutText: 'Sobre a barbearia moderna...',
    openingHours: 'Segunda a Sexta: 09:00 - 19:00',
    instagramUrl: 'https://instagram.com/barbearia',
    facebookUrl: 'https://facebook.com/barbearia',
    whatsappNumber: '+5511999999999',
    services: [
      {
        id: '1',
        name: 'Corte Moderno',
        description: 'Corte contemporâneo',
        duration: 30,
        price: 35.0,
      },
      {
        id: '2',
        name: 'Barba Estilosa',
        description: 'Barba moderna',
        duration: 20,
        price: 25.0,
      },
    ],
  },
};

const renderWithRouter = (component: React.ReactElement) => {
  return render(<BrowserRouter>{component}</BrowserRouter>);
};

describe('Template2Modern', () => {
  beforeEach(() => {
    mockNavigate.mockClear();
  });

  it('should render barbershop name and logo', () => {
    renderWithRouter(<Template2Modern data={mockData} />);

    // Check header name
    expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Barbearia Moderna');
    const logo = screen.getByAltText('Barbearia Moderna');
    expect(logo).toBeInTheDocument();
    expect(logo).toHaveAttribute('src', 'https://example.com/logo.png');
  });

  it('should render hero section with CTA button', () => {
    renderWithRouter(<Template2Modern data={mockData} />);

    // Check hero heading (h2)
    const heroHeading = screen.getByRole('heading', { level: 2 });
    expect(heroHeading).toHaveTextContent('Barbearia Moderna');
    expect(screen.getByText('Experiência moderna e profissional para o seu cuidado pessoal')).toBeInTheDocument();
    expect(screen.getByText('Agendar Serviço')).toBeInTheDocument();
  });

  it('should render services section', () => {
    renderWithRouter(<Template2Modern data={mockData} />);

    expect(screen.getByText('Nossos Serviços')).toBeInTheDocument();
    expect(screen.getByText('Corte Moderno')).toBeInTheDocument();
    expect(screen.getByText('Barba Estilosa')).toBeInTheDocument();
  });

  it('should render about section when aboutText exists', () => {
    renderWithRouter(<Template2Modern data={mockData} />);

    expect(screen.getByText('Sobre Nós')).toBeInTheDocument();
    expect(screen.getByText('Sobre a barbearia moderna...')).toBeInTheDocument();
  });

  it('should render contact section with address and hours', () => {
    renderWithRouter(<Template2Modern data={mockData} />);

    expect(screen.getByText('Onde Estamos')).toBeInTheDocument();
    expect(screen.getByText('Rua das Flores, 123 - Centro, São Paulo - SP')).toBeInTheDocument();
    expect(screen.getByText('Segunda a Sexta: 09:00 - 19:00')).toBeInTheDocument();
  });

  it('should render social media links', () => {
    renderWithRouter(<Template2Modern data={mockData} />);

    expect(screen.getByText('Nos Acompanhe')).toBeInTheDocument();
    const links = screen.getAllByRole('link');
    const instagramLink = links.find(link => link.getAttribute('href')?.includes('instagram'));
    const facebookLink = links.find(link => link.getAttribute('href')?.includes('facebook'));
    expect(instagramLink).toHaveAttribute('href', 'https://instagram.com/barbearia');
    expect(facebookLink).toHaveAttribute('href', 'https://facebook.com/barbearia');
  });

  it('should render footer with copyright and admin link', () => {
    renderWithRouter(<Template2Modern data={mockData} />);

    expect(screen.getByText('© 2025 Barbearia Moderna - Todos os direitos reservados')).toBeInTheDocument();
    expect(screen.getByText('Área Admin')).toBeInTheDocument();
  });

  it('should navigate to scheduling page when CTA clicked', () => {
    renderWithRouter(<Template2Modern data={mockData} />);

    const ctaButton = screen.getByText('Agendar Serviço');
    fireEvent.click(ctaButton);

    expect(mockNavigate).toHaveBeenCalledWith('/barbearia/MOD123/agendar');
  });

  it('should show floating schedule button when services selected', () => {
    renderWithRouter(<Template2Modern data={mockData} />);

    // Select a service by clicking the service card div
    const serviceCard = screen.getByText('Corte Moderno').closest('div');
    if (serviceCard) {
      fireEvent.click(serviceCard);
    }

    // Check if floating button appears
    const floatingButton = screen.getByText('Agendar 1 serviço • R$ 35.00');
    expect(floatingButton).toBeInTheDocument();
  });

  it('should apply modern theme classes', () => {
    const { container } = renderWithRouter(<Template2Modern data={mockData} />);

    // Check for modern color classes
    expect(container.querySelector('.bg-modern-dark')).toBeInTheDocument();
    expect(container.querySelector('.bg-modern-blue')).toBeInTheDocument();
    expect(container.querySelector('.bg-modern-light')).toBeInTheDocument();
  });

  it('should have fixed header', () => {
    const { container } = renderWithRouter(<Template2Modern data={mockData} />);

    const header = container.querySelector('header');
    expect(header).toHaveClass('fixed', 'top-0', 'left-0', 'right-0', 'z-50');
  });
});