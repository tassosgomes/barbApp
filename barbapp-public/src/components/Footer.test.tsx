import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Footer } from './Footer';

describe('Footer', () => {
  it('should render barbershop name in copyright', () => {
    render(<Footer barbershopName="Barbearia Test" />);

    expect(screen.getByText('© 2025 Barbearia Test - Todos os direitos reservados')).toBeInTheDocument();
  });

  it('should render admin link', () => {
    render(<Footer barbershopName="Barbearia Test" />);

    const adminLink = screen.getByText('Área Admin');
    expect(adminLink).toBeInTheDocument();
    expect(adminLink).toHaveAttribute('href', '/admin/login');
  });

  it('should render children when provided', () => {
    render(
      <Footer barbershopName="Barbearia Test">
        <div>Additional content</div>
      </Footer>
    );

    expect(screen.getByText('Additional content')).toBeInTheDocument();
  });

  it('should apply custom className', () => {
    const { container } = render(
      <Footer barbershopName="Barbearia Test" className="custom-footer" />
    );

    expect(container.firstChild).toHaveClass('custom-footer');
  });

  it('should have default footer classes', () => {
    const { container } = render(<Footer barbershopName="Barbearia Test" />);

    const footer = container.firstChild as HTMLElement;
    expect(footer).toHaveClass('bg-black');
    expect(footer).toHaveClass('text-white');
    expect(footer).toHaveClass('py-8');
    expect(footer).toHaveClass('px-6');
    expect(footer).toHaveClass('text-center');
  });
});