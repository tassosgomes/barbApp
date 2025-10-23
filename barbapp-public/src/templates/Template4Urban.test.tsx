import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { Template4Urban } from './Template4Urban';
import type { PublicLandingPage } from '@/types/landing-page.types';

// Mock react-router-dom
const mockNavigate = vi.fn();
vi.mock('react-router-dom', () => ({
  useNavigate: () => mockNavigate,
}));

const mockData: PublicLandingPage = {
  barbershop: {
    id: '1',
    name: 'Barbearia Urbana',
    code: 'URBAN123',
    address: 'Rua Moderna, 789 - Centro, São Paulo - SP',
  },
  landingPage: {
    templateId: 4,
    logoUrl: 'https://example.com/logo.png',
    aboutText: 'Barbearia com energia urbana e estilo contemporâneo',
    openingHours: 'Segunda a Sexta: 09:00 - 19:00\nSábado: 09:00 - 17:00',
    instagramUrl: 'https://instagram.com/urbanbarber',
    facebookUrl: 'https://facebook.com/urbanbarber',
    whatsappNumber: '+5511987654321',
    services: [
      {
        id: '1',
        name: 'Corte Urbano',
        description: 'Corte moderno e estiloso',
        duration: 30,
        price: 40.0,
      },
      {
        id: '2',
        name: 'Barba Street',
        duration: 20,
        price: 30.0,
      },
    ],
  },
};

describe('Template4Urban', () => {
  beforeEach(() => {
    mockNavigate.mockClear();
  });

  it('renders barbershop information with urban styling', () => {
    render(<Template4Urban data={mockData} />);

    expect(screen.getByRole('heading', { name: 'Barbearia Urbana', level: 1 })).toBeInTheDocument();
    expect(screen.getByText('Estilo Urbano • Energia Inigualável • Cortes de Impacto')).toBeInTheDocument();
    expect(screen.getByText('Nossos Serviços')).toBeInTheDocument();
    expect(screen.getByText('Onde Estamos')).toBeInTheDocument();
  });

  it('renders logo with urban styling', () => {
    render(<Template4Urban data={mockData} />);

    const logo = screen.getByAltText('Barbearia Urbana');
    expect(logo).toBeInTheDocument();
    expect(logo).toHaveAttribute('src', 'https://example.com/logo.png');
    expect(logo).toHaveClass('rounded-full', 'border-2', 'border-urban-red');
  });

  it('renders services in 3-column urban grid', () => {
    render(<Template4Urban data={mockData} />);

    expect(screen.getByText('Corte Urbano')).toBeInTheDocument();
    expect(screen.getByText('Barba Street')).toBeInTheDocument();
    expect(screen.getByText('30min')).toBeInTheDocument();
    expect(screen.getByText('R$ 40.00')).toBeInTheDocument();
    expect(screen.getByText('Corte moderno e estiloso')).toBeInTheDocument();
  });

  it('renders about section with urban styling', () => {
    render(<Template4Urban data={mockData} />);

    expect(screen.getByText('Sobre a Barbearia Urbana')).toBeInTheDocument();
    expect(screen.getByText('Barbearia com energia urbana e estilo contemporâneo')).toBeInTheDocument();
  });

  it('renders contact information with urban cards', () => {
    render(<Template4Urban data={mockData} />);

    expect(screen.getByText('Rua Moderna, 789 - Centro, São Paulo - SP')).toBeInTheDocument();
    expect(screen.getByText(/Segunda a Sexta: 09:00 - 19:00/)).toBeInTheDocument();
    expect(screen.getByText(/Sábado: 09:00 - 17:00/)).toBeInTheDocument();
  });

  it('renders social media links with urban styling', () => {
    render(<Template4Urban data={mockData} />);

    const links = screen.getAllByRole('link');
    const instagramLink = links.find(link => link.getAttribute('href')?.includes('instagram'));
    const facebookLink = links.find(link => link.getAttribute('href')?.includes('facebook'));

    expect(instagramLink).toHaveAttribute('href', 'https://instagram.com/urbanbarber');
    expect(facebookLink).toHaveAttribute('href', 'https://facebook.com/urbanbarber');
  });

  it('navigates to schedule page when "Agendar Agora" is clicked', () => {
    render(<Template4Urban data={mockData} />);

    const scheduleButton = screen.getByText('Agendar Agora');
    fireEvent.click(scheduleButton);

    expect(mockNavigate).toHaveBeenCalledWith('/barbearia/URBAN123/agendar');
  });

  it('shows floating schedule button when services are selected', async () => {
    const user = userEvent.setup();
    render(<Template4Urban data={mockData} />);

    // Find and click the checkbox for the first service
    const checkboxes = screen.getAllByRole('checkbox');
    await user.click(checkboxes[0]);

    await waitFor(() => {
      expect(screen.getByText('Agendar 1 serviço • R$ 40.00')).toBeInTheDocument();
    });
  });

  it('navigates with selected services when floating button is clicked', async () => {
    const user = userEvent.setup();
    render(<Template4Urban data={mockData} />);

    // Select a service by clicking the checkbox
    const checkboxes = screen.getAllByRole('checkbox');
    await user.click(checkboxes[0]);

    await waitFor(() => {
      const floatingButton = screen.getByText('Agendar 1 serviço • R$ 40.00');
      fireEvent.click(floatingButton);
    });

    expect(mockNavigate).toHaveBeenCalledWith('/barbearia/URBAN123/agendar?servicos=1');
  });

  it('renders WhatsApp button with urban styling', () => {
    render(<Template4Urban data={mockData} />);

    const whatsappLink = screen.getByRole('link', { name: 'Contato via WhatsApp' });
    expect(whatsappLink).toHaveAttribute('href', 'https://wa.me/5511987654321?text=Ol%C3%A1!%20Gostaria%20de%20fazer%20um%20agendamento');
  });

  it('renders footer with urban elements', () => {
    render(<Template4Urban data={mockData} />);

    expect(screen.getByText('© 2025 Barbearia Urbana')).toBeInTheDocument();
    expect(screen.getByText('Estilo que marca • Energia urbana')).toBeInTheDocument();
    expect(screen.getByText('Área Administrativa')).toHaveAttribute('href', '/admin/login');
  });

  it('applies urban color scheme', () => {
    render(<Template4Urban data={mockData} />);

    // Check for urban color classes on header
    const header = screen.getByRole('banner');
    expect(header).toHaveClass('bg-urban-black/95');

    const heroSection = screen.getByText('Estilo Urbano • Energia Inigualável • Cortes de Impacto').closest('section');
    expect(heroSection).toHaveClass('from-urban-black');
  });

  it('toggles side menu when hamburger button is clicked', () => {
    render(<Template4Urban data={mockData} />);

    // Initially menu is closed
    expect(screen.queryByText('Serviços')).not.toBeInTheDocument();

    // Find menu button (first button in header)
    const buttons = screen.getAllByRole('button');
    const menuButton = buttons.find(button => button.querySelector('svg.lucide-menu'));
    expect(menuButton).toBeInTheDocument();

    fireEvent.click(menuButton!);

    // Menu should be open
    expect(screen.getByText('Serviços')).toBeInTheDocument();
    expect(screen.getByText('Sobre')).toBeInTheDocument();
    expect(screen.getByText('Contato')).toBeInTheDocument();

    // Close menu
    const closeButton = screen.getAllByRole('button').find(button => button.querySelector('svg.lucide-x'));
    expect(closeButton).toBeInTheDocument();
    fireEvent.click(closeButton!);

    // Menu should be closed
    expect(screen.queryByText('Serviços')).not.toBeInTheDocument();
  });
});