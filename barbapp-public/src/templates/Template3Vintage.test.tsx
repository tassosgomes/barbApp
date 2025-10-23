import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { Template3Vintage } from './Template3Vintage';
import type { PublicLandingPage } from '@/types/landing-page.types';

// Mock react-router-dom
const mockNavigate = vi.fn();
vi.mock('react-router-dom', () => ({
  useNavigate: () => mockNavigate,
}));

const mockData: PublicLandingPage = {
  barbershop: {
    id: '1',
    name: 'Barbearia Vintage',
    code: 'VINTAGE123',
    address: 'Rua Antiga, 456 - Centro, São Paulo - SP',
  },
  landingPage: {
    templateId: 3,
    logoUrl: 'https://example.com/logo.png',
    aboutText: 'Barbearia com o charme dos anos 50',
    openingHours: 'Segunda a Sexta: 09:00 - 19:00\nSábado: 09:00 - 17:00',
    instagramUrl: 'https://instagram.com/vintagebarber',
    facebookUrl: 'https://facebook.com/vintagebarber',
    whatsappNumber: '+5511987654321',
    services: [
      {
        id: '1',
        name: 'Corte Social',
        description: 'Corte clássico completo',
        duration: 30,
        price: 35.0,
      },
      {
        id: '2',
        name: 'Barba Vintage',
        duration: 20,
        price: 25.0,
      },
    ],
  },
};

describe('Template3Vintage', () => {
  beforeEach(() => {
    mockNavigate.mockClear();
  });

  it('renders barbershop information with vintage styling', () => {
    render(<Template3Vintage data={mockData} />);

    expect(screen.getByRole('heading', { name: 'Barbearia Vintage', level: 1 })).toBeInTheDocument();
    expect(screen.getByText('Experiência autêntica dos anos 50 • Cortes clássicos • Ambiente acolhedor')).toBeInTheDocument();
    expect(screen.getByText('Nossos Serviços')).toBeInTheDocument();
    expect(screen.getByText('Onde Estamos')).toBeInTheDocument();
  });

  it('renders logo with vintage styling', () => {
    render(<Template3Vintage data={mockData} />);

    const logo = screen.getByAltText('Barbearia Vintage');
    expect(logo).toBeInTheDocument();
    expect(logo).toHaveAttribute('src', 'https://example.com/logo.png');
    expect(logo).toHaveClass('rounded-full', 'border-4', 'border-vintage-cream');
  });

  it('renders services in vintage list format', () => {
    render(<Template3Vintage data={mockData} />);

    expect(screen.getByText('Corte Social')).toBeInTheDocument();
    expect(screen.getByText('Barba Vintage')).toBeInTheDocument();
    expect(screen.getByText('30min')).toBeInTheDocument();
    expect(screen.getByText('R$ 35.00')).toBeInTheDocument();
    expect(screen.getByText('Corte clássico completo')).toBeInTheDocument();
  });

  it('renders about section with vintage styling', () => {
    render(<Template3Vintage data={mockData} />);

    expect(screen.getByText('Sobre a Barbearia Vintage')).toBeInTheDocument();
    expect(screen.getByText('Barbearia com o charme dos anos 50')).toBeInTheDocument();
  });

  it('renders contact information with vintage cards', () => {
    render(<Template3Vintage data={mockData} />);

    expect(screen.getByText('Rua Antiga, 456 - Centro, São Paulo - SP')).toBeInTheDocument();
    expect(screen.getByText(/Segunda a Sexta: 09:00 - 19:00/)).toBeInTheDocument();
    expect(screen.getByText(/Sábado: 09:00 - 17:00/)).toBeInTheDocument();
  });

  it('renders social media links with vintage styling', () => {
    render(<Template3Vintage data={mockData} />);

    const links = screen.getAllByRole('link');
    const instagramLink = links.find(link => link.getAttribute('href')?.includes('instagram'));
    const facebookLink = links.find(link => link.getAttribute('href')?.includes('facebook'));

    expect(instagramLink).toHaveAttribute('href', 'https://instagram.com/vintagebarber');
    expect(facebookLink).toHaveAttribute('href', 'https://facebook.com/vintagebarber');
  });

  it('navigates to schedule page when "Agendar Agora" is clicked', () => {
    render(<Template3Vintage data={mockData} />);

    const scheduleButton = screen.getByText('Agendar Agora');
    fireEvent.click(scheduleButton);

    expect(mockNavigate).toHaveBeenCalledWith('/barbearia/VINTAGE123/agendar');
  });

  it('navigates to schedule page when "Agendar Seu Corte" is clicked', () => {
    render(<Template3Vintage data={mockData} />);

    const scheduleButton = screen.getByText('Agendar Seu Corte');
    fireEvent.click(scheduleButton);

    expect(mockNavigate).toHaveBeenCalledWith('/barbearia/VINTAGE123/agendar');
  });

  it('shows floating schedule button when services are selected', async () => {
    const user = userEvent.setup();
    render(<Template3Vintage data={mockData} />);

    // Find and click the checkbox for the first service
    const checkboxes = screen.getAllByRole('checkbox');
    await user.click(checkboxes[0]);

    await waitFor(() => {
      expect(screen.getByText('Agendar 1 serviço • R$ 35.00')).toBeInTheDocument();
    });
  });

  it('navigates with selected services when floating button is clicked', async () => {
    const user = userEvent.setup();
    render(<Template3Vintage data={mockData} />);

    // Select a service by clicking the checkbox
    const checkboxes = screen.getAllByRole('checkbox');
    await user.click(checkboxes[0]);

    await waitFor(() => {
      const floatingButton = screen.getByText('Agendar 1 serviço • R$ 35.00');
      fireEvent.click(floatingButton);
    });

    expect(mockNavigate).toHaveBeenCalledWith('/barbearia/VINTAGE123/agendar?servicos=1');
  });

  it('renders WhatsApp button with vintage styling', () => {
    render(<Template3Vintage data={mockData} />);

    const whatsappLink = screen.getByRole('link', { name: 'Contato via WhatsApp' });
    expect(whatsappLink).toHaveAttribute('href', 'https://wa.me/5511987654321?text=Ol%C3%A1!%20Gostaria%20de%20fazer%20um%20agendamento');
  });

  it('renders footer with vintage elements', () => {
    render(<Template3Vintage data={mockData} />);

    expect(screen.getByText('© 2025 Barbearia Vintage')).toBeInTheDocument();
    expect(screen.getByText('Tradição e estilo desde sempre')).toBeInTheDocument();
    expect(screen.getByText('Área Administrativa')).toHaveAttribute('href', '/admin/login');
  });

  it('applies vintage color scheme', () => {
    render(<Template3Vintage data={mockData} />);

    // Check for vintage color classes
    const header = screen.getByRole('banner');
    expect(header).toHaveClass('bg-vintage-brown');

    const heroSection = screen.getByText('Experiência autêntica dos anos 50 • Cortes clássicos • Ambiente acolhedor').closest('section');
    expect(heroSection).toHaveClass('from-vintage-brown/10');
  });
});