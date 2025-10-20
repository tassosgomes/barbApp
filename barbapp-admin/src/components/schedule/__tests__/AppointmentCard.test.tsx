import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { AppointmentCard } from '../AppointmentCard';
import { AppointmentStatus, Appointment } from '@/types/appointment';

const mockAppointment: Appointment = {
  id: 'apt-123',
  customerName: 'João Silva',
  serviceTitle: 'Corte de Cabelo',
  startTime: '2025-10-20T10:00:00Z',
  endTime: '2025-10-20T10:30:00Z',
  status: AppointmentStatus.Pending,
};

describe('AppointmentCard', () => {
  describe('Renderização', () => {
    it('deve renderizar informações básicas do agendamento', () => {
      render(<AppointmentCard appointment={mockAppointment} />);

      expect(screen.getByText('João Silva')).toBeInTheDocument();
      expect(screen.getByText('Corte de Cabelo')).toBeInTheDocument();
      expect(screen.getByText(/07:00/)).toBeInTheDocument(); // Horário pode variar por timezone
    });

    it('deve renderizar badge de status', () => {
      render(<AppointmentCard appointment={mockAppointment} />);

      expect(screen.getByText('Pendente')).toBeInTheDocument();
    });

    it('deve aplicar cor de borda baseada no status', () => {
      const { container, rerender } = render(
        <AppointmentCard appointment={mockAppointment} />
      );

      // Pending - amarelo
      expect(container.querySelector('.border-l-yellow-500')).toBeInTheDocument();

      // Confirmed - verde
      rerender(
        <AppointmentCard
          appointment={{ ...mockAppointment, status: AppointmentStatus.Confirmed }}
        />
      );
      expect(container.querySelector('.border-l-green-500')).toBeInTheDocument();

      // Completed - cinza
      rerender(
        <AppointmentCard
          appointment={{ ...mockAppointment, status: AppointmentStatus.Completed }}
        />
      );
      expect(container.querySelector('.border-l-gray-400')).toBeInTheDocument();

      // Cancelled - vermelho
      rerender(
        <AppointmentCard
          appointment={{ ...mockAppointment, status: AppointmentStatus.Cancelled }}
        />
      );
      expect(container.querySelector('.border-l-red-500')).toBeInTheDocument();
    });
  });

  describe('Botões de Ação - Status Pendente', () => {
    it('deve mostrar botões Confirmar e Cancelar para status Pendente', () => {
      const onConfirm = vi.fn();
      const onCancel = vi.fn();

      render(
        <AppointmentCard
          appointment={mockAppointment}
          onConfirm={onConfirm}
          onCancel={onCancel}
        />
      );

      expect(screen.getByRole('button', { name: /confirmar/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /cancelar/i })).toBeInTheDocument();
    });

    it('deve chamar onConfirm ao clicar em Confirmar', async () => {
      const user = userEvent.setup();
      const onConfirm = vi.fn();

      render(<AppointmentCard appointment={mockAppointment} onConfirm={onConfirm} />);

      const confirmButton = screen.getByRole('button', { name: /confirmar/i });
      await user.click(confirmButton);

      expect(onConfirm).toHaveBeenCalledWith('apt-123');
    });

    it('deve chamar onCancel ao clicar em Cancelar', async () => {
      const user = userEvent.setup();
      const onCancel = vi.fn();

      render(<AppointmentCard appointment={mockAppointment} onCancel={onCancel} />);

      const cancelButton = screen.getByRole('button', { name: /cancelar/i });
      await user.click(cancelButton);

      expect(onCancel).toHaveBeenCalledWith('apt-123');
    });
  });

  describe('Botões de Ação - Status Confirmado', () => {
    it('deve mostrar botões Concluir e Cancelar para status Confirmado', () => {
      const onComplete = vi.fn();
      const onCancel = vi.fn();

      render(
        <AppointmentCard
          appointment={{ ...mockAppointment, status: AppointmentStatus.Confirmed }}
          onComplete={onComplete}
          onCancel={onCancel}
        />
      );

      expect(screen.getByRole('button', { name: /concluir/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /cancelar/i })).toBeInTheDocument();
    });

    it('deve chamar onComplete ao clicar em Concluir', async () => {
      const user = userEvent.setup();
      const onComplete = vi.fn();

      render(
        <AppointmentCard
          appointment={{ ...mockAppointment, status: AppointmentStatus.Confirmed }}
          onComplete={onComplete}
        />
      );

      const completeButton = screen.getByRole('button', { name: /concluir/i });
      await user.click(completeButton);

      expect(onComplete).toHaveBeenCalledWith('apt-123');
    });
  });

  describe('Botões de Ação - Status Concluído/Cancelado', () => {
    it('não deve mostrar botões para status Concluído', () => {
      render(
        <AppointmentCard
          appointment={{ ...mockAppointment, status: AppointmentStatus.Completed }}
        />
      );

      expect(screen.queryByRole('button', { name: /confirmar/i })).not.toBeInTheDocument();
      expect(screen.queryByRole('button', { name: /cancelar/i })).not.toBeInTheDocument();
      expect(screen.queryByRole('button', { name: /concluir/i })).not.toBeInTheDocument();
    });

    it('não deve mostrar botões para status Cancelado', () => {
      render(
        <AppointmentCard
          appointment={{ ...mockAppointment, status: AppointmentStatus.Cancelled }}
        />
      );

      expect(screen.queryByRole('button', { name: /confirmar/i })).not.toBeInTheDocument();
      expect(screen.queryByRole('button', { name: /cancelar/i })).not.toBeInTheDocument();
      expect(screen.queryByRole('button', { name: /concluir/i })).not.toBeInTheDocument();
    });
  });

  describe('Interação com Card', () => {
    it('deve chamar onClick ao clicar no card', async () => {
      const user = userEvent.setup();
      const onClick = vi.fn();

      render(<AppointmentCard appointment={mockAppointment} onClick={onClick} />);

      const card = screen.getByText('João Silva').closest('div')?.parentElement?.parentElement;
      if (card) {
        await user.click(card);
        expect(onClick).toHaveBeenCalledWith('apt-123');
      }
    });

    it('não deve chamar onClick do card ao clicar em botões de ação', async () => {
      const user = userEvent.setup();
      const onClick = vi.fn();
      const onConfirm = vi.fn();

      render(
        <AppointmentCard
          appointment={mockAppointment}
          onClick={onClick}
          onConfirm={onConfirm}
        />
      );

      const confirmButton = screen.getByRole('button', { name: /confirmar/i });
      await user.click(confirmButton);

      expect(onConfirm).toHaveBeenCalled();
      expect(onClick).not.toHaveBeenCalled();
    });
  });

  describe('Estado de Loading', () => {
    it('deve desabilitar interações quando isLoading é true', () => {
      const onClick = vi.fn();
      const onConfirm = vi.fn();

      const { container } = render(
        <AppointmentCard
          appointment={mockAppointment}
          onClick={onClick}
          onConfirm={onConfirm}
          isLoading={true}
        />
      );

      // Card deve ter pointer-events-none
      expect(container.querySelector('.pointer-events-none')).toBeInTheDocument();

      // Botões devem estar desabilitados
      const confirmButton = screen.getByRole('button', { name: /confirmar/i });
      expect(confirmButton).toBeDisabled();
    });

    it('deve aplicar opacity reduzida quando isLoading é true', () => {
      const { container } = render(
        <AppointmentCard appointment={mockAppointment} isLoading={true} />
      );

      expect(container.querySelector('.opacity-50')).toBeInTheDocument();
    });
  });

  describe('Acessibilidade', () => {
    it('botões devem ter área de toque adequada (min 44px)', () => {
      render(
        <AppointmentCard
          appointment={mockAppointment}
          onConfirm={vi.fn()}
          onCancel={vi.fn()}
        />
      );

      const confirmButton = screen.getByRole('button', { name: /confirmar/i });
      const cancelButton = screen.getByRole('button', { name: /cancelar/i });

      // Verifica se tem classe min-h-[44px]
      expect(confirmButton.className).toContain('min-h-[44px]');
      expect(cancelButton.className).toContain('min-h-[44px]');
    });
  });
});
