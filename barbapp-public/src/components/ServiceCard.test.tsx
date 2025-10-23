import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { ServiceCard } from './ServiceCard';
import type { PublicService } from '@/types/landing-page.types';

const mockService: PublicService = {
  id: '1',
  name: 'Corte Social',
  description: 'Corte tradicional com máquina e tesoura',
  duration: 30,
  price: 35.00,
};

describe('ServiceCard', () => {
  it('should render service information correctly', () => {
    render(
      <ServiceCard
        service={mockService}
        isSelected={false}
        onToggle={() => {}}
      />
    );

    expect(screen.getByText('Corte Social')).toBeInTheDocument();
    expect(screen.getByText('Corte tradicional com máquina e tesoura')).toBeInTheDocument();
    expect(screen.getByText('30min')).toBeInTheDocument();
    expect(screen.getByText('R$ 35.00')).toBeInTheDocument();
  });

  it('should show selected state when isSelected is true', () => {
    const { container } = render(
      <ServiceCard
        service={mockService}
        isSelected={true}
        onToggle={() => {}}
      />
    );

    const card = container.firstChild as HTMLElement;
    expect(card).toHaveClass('border-primary');
    expect(card).toHaveClass('bg-primary/5');
  });

  it('should call onToggle when clicked', () => {
    const mockOnToggle = vi.fn();
    render(
      <ServiceCard
        service={mockService}
        isSelected={false}
        onToggle={mockOnToggle}
      />
    );

    const card = screen.getByText('Corte Social').closest('div')?.parentElement;
    fireEvent.click(card!);

    expect(mockOnToggle).toHaveBeenCalledTimes(1);
  });

  it('should not render description when not provided', () => {
    const serviceWithoutDescription: PublicService = {
      ...mockService,
      description: undefined,
    };

    render(
      <ServiceCard
        service={serviceWithoutDescription}
        isSelected={false}
        onToggle={() => {}}
      />
    );

    expect(screen.queryByText('Corte tradicional com máquina e tesoura')).not.toBeInTheDocument();
  });
});