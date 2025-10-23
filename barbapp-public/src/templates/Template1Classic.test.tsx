import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi, describe, it, expect, beforeEach } from 'vitest';
import { Template1Classic } from './Template1Classic';
import type { PublicLandingPage } from '@/types/landing-page.types';

// Mock react-router-dom
const mockNavigate = vi.fn();
vi.mock('react-router-dom', () => ({
  useNavigate: () => mockNavigate,
}));

const mockData: PublicLandingPage = {
  barbershop: {
    id: '1',
    name: 'Barbearia Teste',
    code: 'TEST123',
    address: 'Rua Teste, 123 - Centro, São Paulo - SP',
  },
  landingPage: {
    templateId: 1,
    logoUrl: 'https://example.com/logo.png',
    aboutText: 'Sobre a barbearia de teste',
    openingHours: 'Segunda a Sexta: 09:00 - 19:00',
    instagramUrl: 'https://instagram.com/teste',
    facebookUrl: 'https://facebook.com/teste',
    whatsappNumber: '+5511999999999',
    services: [
      {
        id: '1',
        name: 'Corte Social',
        description: 'Corte completo',
        duration: 30,
        price: 35.0,
      },
      {
        id: '2',
        name: 'Barba',
        duration: 20,
        price: 25.0,
      },
    ],
  },
};

describe('Template1Classic', () => {
  beforeEach(() => {
    mockNavigate.mockClear();
  });

  it('renders barbershop information correctly', () => {
    render(<Template1Classic data={mockData} />);

    expect(screen.getByRole('heading', { name: 'Barbearia Teste', level: 1 })).toBeInTheDocument();
    expect(screen.getByText('Tradição e Elegância desde sempre')).toBeInTheDocument();
    expect(screen.getByText('Nossos Serviços')).toBeInTheDocument();
    expect(screen.getByText('Sobre Nós')).toBeInTheDocument();
    expect(screen.getByText('Onde Estamos')).toBeInTheDocument();
  });

  it('renders logo when provided', () => {
    render(<Template1Classic data={mockData} />);

    const logo = screen.getByAltText('Barbearia Teste');
    expect(logo).toBeInTheDocument();
    expect(logo).toHaveAttribute('src', 'https://example.com/logo.png');
  });

  it('renders services correctly', () => {
    render(<Template1Classic data={mockData} />);

    expect(screen.getByText('Corte Social')).toBeInTheDocument();
    expect(screen.getByText('Barba')).toBeInTheDocument();
    expect(screen.getByText('30min')).toBeInTheDocument();
    expect(screen.getByText('R$ 35.00')).toBeInTheDocument();
  });

  it('renders about section when aboutText is provided', () => {
    render(<Template1Classic data={mockData} />);

    expect(screen.getByText('Sobre a barbearia de teste')).toBeInTheDocument();
  });

  it('renders contact information', () => {
    render(<Template1Classic data={mockData} />);

    expect(screen.getByText('Rua Teste, 123 - Centro, São Paulo - SP')).toBeInTheDocument();
    expect(screen.getByText('Segunda a Sexta: 09:00 - 19:00')).toBeInTheDocument();
  });

  it('renders social media links when provided', () => {
    render(<Template1Classic data={mockData} />);

    const links = screen.getAllByRole('link');
    const instagramLink = links.find(link => link.getAttribute('href')?.includes('instagram'));
    const facebookLink = links.find(link => link.getAttribute('href')?.includes('facebook'));

    expect(instagramLink).toHaveAttribute('href', 'https://instagram.com/teste');
    expect(facebookLink).toHaveAttribute('href', 'https://facebook.com/teste');
  });

  it('navigates to schedule page when "Agendar Agora" is clicked', () => {
    render(<Template1Classic data={mockData} />);

    const scheduleButton = screen.getByText('Agendar Agora');
    fireEvent.click(scheduleButton);

    expect(mockNavigate).toHaveBeenCalledWith('/barbearia/TEST123/agendar');
  });

  it('shows floating schedule button when services are selected', async () => {
    const user = userEvent.setup();
    render(<Template1Classic data={mockData} />);

    // Select a service by clicking the service card (not the checkbox)
    const serviceCard = screen.getByText('Corte Social').closest('div');
    await user.click(serviceCard!);

    await waitFor(() => {
      expect(screen.getByText('Agendar 1 serviço • R$ 35.00')).toBeInTheDocument();
    });
  });

  it('navigates with selected services when floating button is clicked', async () => {
    const user = userEvent.setup();
    render(<Template1Classic data={mockData} />);

    // Select a service by clicking the service card
    const serviceCard = screen.getByText('Corte Social').closest('div');
    await user.click(serviceCard!);

    await waitFor(() => {
      const floatingButton = screen.getByText('Agendar 1 serviço • R$ 35.00');
      fireEvent.click(floatingButton);
    });

    expect(mockNavigate).toHaveBeenCalledWith('/barbearia/TEST123/agendar?servicos=1');
  });

  it('renders WhatsApp button', () => {
    render(<Template1Classic data={mockData} />);

    const whatsappLink = screen.getByRole('link', { name: 'Contato via WhatsApp' });
    expect(whatsappLink).toHaveAttribute('href', 'https://wa.me/5511999999999?text=Ol%C3%A1!%20Gostaria%20de%20fazer%20um%20agendamento');
  });

  it('renders footer with admin link', () => {
    render(<Template1Classic data={mockData} />);

    expect(screen.getByText('© 2025 Barbearia Teste - Todos os direitos reservados')).toBeInTheDocument();
    expect(screen.getByText('Área Admin')).toHaveAttribute('href', '/admin/login');
  });
});