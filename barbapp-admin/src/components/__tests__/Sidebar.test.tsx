import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { Sidebar } from '../Sidebar';

// Mock useParams
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useParams: () => ({ codigo: 'TEST1234' }),
  };
});

const renderWithRouter = (ui: React.ReactElement) => {
  return render(<BrowserRouter>{ui}</BrowserRouter>);
};

describe('Sidebar', () => {
  it('should render all navigation items', () => {
    renderWithRouter(<Sidebar isOpen={true} />);

    expect(screen.getByText('Dashboard')).toBeInTheDocument();
    expect(screen.getByText('Barbeiros')).toBeInTheDocument();
    expect(screen.getByText('Serviços')).toBeInTheDocument();
    expect(screen.getByText('Agenda')).toBeInTheDocument();
    expect(screen.getByText('Landing Page')).toBeInTheDocument();
  });

  it('should render navigation links with correct paths', () => {
    renderWithRouter(<Sidebar isOpen={true} />);

    const dashboardLink = screen.getByRole('link', { name: /dashboard/i });
    expect(dashboardLink).toHaveAttribute('href', '/TEST1234/dashboard');

    const barbeirosLink = screen.getByRole('link', { name: /barbeiros/i });
    expect(barbeirosLink).toHaveAttribute('href', '/TEST1234/barbeiros');

    const servicosLink = screen.getByRole('link', { name: /serviços/i });
    expect(servicosLink).toHaveAttribute('href', '/TEST1234/servicos');

    const agendaLink = screen.getByRole('link', { name: /agenda/i });
    expect(agendaLink).toHaveAttribute('href', '/TEST1234/agenda');

    const landingPageLink = screen.getByRole('link', { name: /landing page/i });
    expect(landingPageLink).toHaveAttribute('href', '/TEST1234/landing-page');
  });

  it('should apply correct visibility class when isOpen is false', () => {
    const { container } = renderWithRouter(<Sidebar isOpen={false} />);

    const sidebar = container.querySelector('aside');
    expect(sidebar).toHaveClass('-translate-x-full');
  });

  it('should apply correct visibility class when isOpen is true', () => {
    const { container } = renderWithRouter(<Sidebar isOpen={true} />);

    const sidebar = container.querySelector('aside');
    expect(sidebar).toHaveClass('translate-x-0');
  });

  it('should call onClose when close button is clicked', async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    renderWithRouter(<Sidebar isOpen={true} onClose={onClose} />);

    const closeButton = screen.getByRole('button');
    await user.click(closeButton);

    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it('should call onClose when navigation link is clicked', async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    renderWithRouter(<Sidebar isOpen={true} onClose={onClose} />);

    const dashboardLink = screen.getByRole('link', { name: /dashboard/i });
    await user.click(dashboardLink);

    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it('should render mobile overlay when isOpen and onClose is provided', () => {
    const onClose = vi.fn();
    const { container } = renderWithRouter(<Sidebar isOpen={true} onClose={onClose} />);

    const overlay = container.querySelector('.fixed.inset-0.bg-black\\/50');
    expect(overlay).toBeInTheDocument();
  });

  it('should not render mobile overlay when onClose is not provided', () => {
    const { container } = renderWithRouter(<Sidebar isOpen={true} />);

    const overlay = container.querySelector('.fixed.inset-0.bg-black\\/50');
    expect(overlay).not.toBeInTheDocument();
  });
});
