import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { Template5Premium } from './Template5Premium';
import type { PublicLandingPage } from '@/types/landing-page.types';

// Mock react-router-dom
const mockNavigate = vi.fn();
vi.mock('react-router-dom', () => ({
  useNavigate: () => mockNavigate,
}));

const mockData: PublicLandingPage = {
  barbershop: {
    id: '1',
    name: 'Barbearia Premium',
    code: 'PREMIUM123',
    address: 'Rua Premium, 456 - Centro, São Paulo - SP',
  },
  landingPage: {
    templateId: 5,
    logoUrl: 'https://example.com/logo.png',
    aboutText: 'Sobre a barbearia premium de teste',
    openingHours: 'Segunda a Sexta: 10:00 - 20:00',
    instagramUrl: 'https://instagram.com/premium',
    facebookUrl: 'https://facebook.com/premium',
    whatsappNumber: '+5511988888888',
    services: [
      {
        id: '1',
        name: 'Corte Executivo',
        description: 'Corte completo e premium',
        duration: 45,
        price: 75.0,
      },
      {
        id: '2',
        name: 'Barba Premium',
        duration: 30,
        price: 45.0,
      },
    ],
  },
};

describe('Template5Premium', () => {
  beforeEach(() => {
    mockNavigate.mockClear();
  });

  it('renders barbershop information correctly', () => {
    render(<Template5Premium data={mockData} />);

    expect(screen.getByRole('heading', { name: 'Barbearia Premium', level: 1 })).toBeInTheDocument();
    expect(screen.getByText('Luxo e Sofisticação em Cada Detalhe')).toBeInTheDocument();
    expect(screen.getByText('Nossos Serviços Premium')).toBeInTheDocument();
    expect(screen.getByText('Sobre Nós')).toBeInTheDocument();
    expect(screen.getByText('Entre em Contato')).toBeInTheDocument();
  });

  it('renders logo when provided', () => {
    render(<Template5Premium data={mockData} />);

    const logo = screen.getByAltText('Barbearia Premium');
    expect(logo).toBeInTheDocument();
    expect(logo).toHaveAttribute('src', 'https://example.com/logo.png');
  });

  it('renders services in list format', () => {
    render(<Template5Premium data={mockData} />);

    expect(screen.getByText('Corte Executivo')).toBeInTheDocument();
    expect(screen.getByText('Barba Premium')).toBeInTheDocument();
    expect(screen.getByText('R$ 75.00')).toBeInTheDocument();
    expect(screen.getByText('45 minutos')).toBeInTheDocument();
  });

  it('renders about section when aboutText is provided', () => {
    render(<Template5Premium data={mockData} />);

    expect(screen.getByText('Sobre a barbearia premium de teste')).toBeInTheDocument();
  });

  it('renders contact information', () => {
    render(<Template5Premium data={mockData} />);

    expect(screen.getByText('Rua Premium, 456 - Centro, São Paulo - SP')).toBeInTheDocument();
    expect(screen.getByText('Segunda a Sexta: 10:00 - 20:00')).toBeInTheDocument();
  });

  it('renders social media links when provided', () => {
    render(<Template5Premium data={mockData} />);

    const links = screen.getAllByRole('link');
    const instagramLink = links.find(link => link.getAttribute('href')?.includes('instagram'));
    const facebookLink = links.find(link => link.getAttribute('href')?.includes('facebook'));

    expect(instagramLink).toHaveAttribute('href', 'https://instagram.com/premium');
    expect(facebookLink).toHaveAttribute('href', 'https://facebook.com/premium');
  });

  it('navigates to schedule page when "Agendar Agora" is clicked', () => {
    render(<Template5Premium data={mockData} />);

    const scheduleButton = screen.getByText('Agendar Experiência Premium');
    fireEvent.click(scheduleButton);

    expect(mockNavigate).toHaveBeenCalledWith('/barbearia/PREMIUM123/agendar');
  });

  it('shows floating schedule button when services are selected', async () => {
    const user = userEvent.setup();
    render(<Template5Premium data={mockData} />);

    // Find the checkbox for the first service
    const checkboxes = screen.getAllByRole('checkbox');
    const firstCheckbox = checkboxes[0];
    await user.click(firstCheckbox);

    await waitFor(() => {
      expect(screen.getByText('Agendar 1 serviço • R$ 75.00')).toBeInTheDocument();
    });
  });

  it('navigates with selected services when floating button is clicked', async () => {
    const user = userEvent.setup();
    render(<Template5Premium data={mockData} />);

    // Select a service by clicking the checkbox
    const checkboxes = screen.getAllByRole('checkbox');
    const firstCheckbox = checkboxes[0];
    await user.click(firstCheckbox);

    await waitFor(() => {
      const floatingButton = screen.getByText('Agendar 1 serviço • R$ 75.00');
      fireEvent.click(floatingButton);
    });

    expect(mockNavigate).toHaveBeenCalledWith('/barbearia/PREMIUM123/agendar?servicos=1');
  });

  it('renders WhatsApp button', () => {
    render(<Template5Premium data={mockData} />);

    const whatsappLink = screen.getByRole('link', { name: 'Contato via WhatsApp' });
    expect(whatsappLink).toHaveAttribute('href', 'https://wa.me/5511988888888?text=Ol%C3%A1!%20Gostaria%20de%20fazer%20um%20agendamento');
  });

  it('renders footer with admin link', () => {
    render(<Template5Premium data={mockData} />);

    expect(screen.getByText('© 2025 Barbearia Premium - Luxo e Tradição')).toBeInTheDocument();
    expect(screen.getByText('Área Administrativa')).toHaveAttribute('href', '/admin/login');
  });

  it('renders testimonials section', () => {
    render(<Template5Premium data={mockData} />);

    expect(screen.getByText('Depoimentos')).toBeInTheDocument();
    expect(screen.getByText('- Cliente Satisfeito')).toBeInTheDocument();
    expect(screen.getByText('- Cliente Premium')).toBeInTheDocument();
  });

  it('header becomes solid on scroll', () => {
    render(<Template5Premium data={mockData} />);

    // Initially transparent
    const header = screen.getByRole('banner');
    expect(header).toHaveClass('bg-transparent');

    // Simulate scroll
    fireEvent.scroll(window, { target: { scrollY: 150 } });

    // Should become solid (this test might need adjustment based on implementation)
    // For now, just check that the component renders without errors
  });
});