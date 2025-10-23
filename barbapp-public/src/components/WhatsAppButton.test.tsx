import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { WhatsAppButton } from './WhatsAppButton';

// Mock window.open
const mockOpen = vi.fn();
Object.defineProperty(window, 'open', {
  writable: true,
  value: mockOpen,
});

describe('WhatsAppButton', () => {
  beforeEach(() => {
    mockOpen.mockClear();
  });

  it('should render normal button with text', () => {
    render(
      <WhatsAppButton
        phoneNumber="+5511999999999"
        message="Olá!"
      />
    );

    expect(screen.getByText('Falar no WhatsApp')).toBeInTheDocument();
    expect(screen.getByRole('link')).toHaveAttribute('aria-label', 'Contato via WhatsApp');
  });

  it('should render floating button without text', () => {
    render(
      <WhatsAppButton
        phoneNumber="+5511999999999"
        floating={true}
      />
    );

    expect(screen.queryByText('Falar no WhatsApp')).not.toBeInTheDocument();
    const link = screen.getByRole('link');
    expect(link).toHaveClass('fixed');
    expect(link).toHaveClass('bottom-6');
    expect(link).toHaveClass('right-6');
  });

  it('should generate correct WhatsApp URL', () => {
    render(
      <WhatsAppButton
        phoneNumber="+5511999999999"
        message="Olá! Gostaria de agendar"
      />
    );

    const link = screen.getByRole('link');
    const href = link.getAttribute('href');
    expect(href).toBe('https://wa.me/5511999999999?text=Ol%C3%A1!%20Gostaria%20de%20agendar');
  });

  it('should use default message when not provided', () => {
    render(
      <WhatsAppButton
        phoneNumber="+5511999999999"
      />
    );

    const link = screen.getByRole('link');
    const href = link.getAttribute('href');
    expect(href).toBe('https://wa.me/5511999999999?text=Ol%C3%A1!%20Gostaria%20de%20fazer%20um%20agendamento');
  });

  it('should clean phone number formatting', () => {
    render(
      <WhatsAppButton
        phoneNumber="(11) 99999-9999"
      />
    );

    const link = screen.getByRole('link');
    expect(link).toHaveAttribute('href', expect.stringContaining('wa.me/11999999999'));
  });

  it('should open WhatsApp in new tab', () => {
    render(
      <WhatsAppButton
        phoneNumber="+5511999999999"
      />
    );

    const link = screen.getByRole('link');
    expect(link).toHaveAttribute('target', '_blank');
    expect(link).toHaveAttribute('rel', 'noopener noreferrer');
  });

  it('should apply custom className', () => {
    render(
      <WhatsAppButton
        phoneNumber="+5511999999999"
        className="custom-class"
      />
    );

    const link = screen.getByRole('link');
    expect(link).toHaveClass('custom-class');
  });
});