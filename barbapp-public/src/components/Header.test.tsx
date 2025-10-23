import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Header } from './Header';

describe('Header', () => {
  it('should render barbershop name', () => {
    render(<Header barbershopName="Barbearia Test" />);

    expect(screen.getByText('Barbearia Test')).toBeInTheDocument();
  });

  it('should render logo when provided', () => {
    render(
      <Header
        barbershopName="Barbearia Test"
        logoUrl="https://example.com/logo.png"
      />
    );

    const logo = screen.getByAltText('Barbearia Test');
    expect(logo).toBeInTheDocument();
    expect(logo).toHaveAttribute('src', 'https://example.com/logo.png');
  });

  it('should not render logo when not provided', () => {
    render(<Header barbershopName="Barbearia Test" />);

    expect(screen.queryByAltText('Barbearia Test')).not.toBeInTheDocument();
  });

  it('should render children when provided', () => {
    render(
      <Header barbershopName="Barbearia Test">
        <button>Custom Button</button>
      </Header>
    );

    expect(screen.getByText('Custom Button')).toBeInTheDocument();
  });

  it('should apply custom className', () => {
    const { container } = render(
      <Header barbershopName="Barbearia Test" className="custom-header" />
    );

    expect(container.firstChild).toHaveClass('custom-header');
  });

  it('should have default header classes', () => {
    const { container } = render(<Header barbershopName="Barbearia Test" />);

    const header = container.firstChild as HTMLElement;
    expect(header).toHaveClass('bg-black');
    expect(header).toHaveClass('text-white');
    expect(header).toHaveClass('py-4');
    expect(header).toHaveClass('px-6');
    expect(header).toHaveClass('sticky');
    expect(header).toHaveClass('top-0');
    expect(header).toHaveClass('z-40');
  });
});