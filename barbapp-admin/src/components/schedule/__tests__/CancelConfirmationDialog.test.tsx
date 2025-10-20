import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CancelConfirmationDialog } from '../CancelConfirmationDialog';

describe('CancelConfirmationDialog', () => {
  const defaultProps = {
    open: true,
    onOpenChange: vi.fn(),
    onConfirm: vi.fn(),
  };

  describe('Renderização', () => {
    it('deve renderizar título e descrição padrão', () => {
      render(<CancelConfirmationDialog {...defaultProps} />);

      expect(screen.getByText('Cancelar Agendamento')).toBeInTheDocument();
      expect(
        screen.getByText(/tem certeza que deseja cancelar este agendamento/i)
      ).toBeInTheDocument();
    });

    it('deve renderizar descrição personalizada com nome do cliente', () => {
      render(<CancelConfirmationDialog {...defaultProps} customerName="João Silva" />);

      expect(screen.getByText(/joão silva/i)).toBeInTheDocument();
    });

    it('deve exibir aviso sobre ação irreversível', () => {
      render(<CancelConfirmationDialog {...defaultProps} />);

      expect(screen.getByText(/esta ação não pode ser desfeita/i)).toBeInTheDocument();
    });

    it('deve renderizar ícone na interface', () => {
      render(<CancelConfirmationDialog {...defaultProps} />);

      // Verificar que o título com ícone está presente
      expect(screen.getByText('Cancelar Agendamento')).toBeInTheDocument();
    });
  });

  describe('Botões', () => {
    it('deve renderizar botões "Voltar" e "Sim, Cancelar"', () => {
      render(<CancelConfirmationDialog {...defaultProps} />);

      expect(screen.getByRole('button', { name: /voltar/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /sim, cancelar/i })).toBeInTheDocument();
    });

    it('deve chamar onOpenChange(false) quando botão "Voltar" é clicado', async () => {
      const user = userEvent.setup();
      const onOpenChange = vi.fn();

      render(<CancelConfirmationDialog {...defaultProps} onOpenChange={onOpenChange} />);

      await user.click(screen.getByRole('button', { name: /voltar/i }));
      expect(onOpenChange).toHaveBeenCalledWith(false);
    });

    it('deve chamar onConfirm quando botão "Sim, Cancelar" é clicado', async () => {
      const user = userEvent.setup();
      const onConfirm = vi.fn();

      render(<CancelConfirmationDialog {...defaultProps} onConfirm={onConfirm} />);

      await user.click(screen.getByRole('button', { name: /sim, cancelar/i }));
      expect(onConfirm).toHaveBeenCalledTimes(1);
    });
  });

  describe('Estado de Loading', () => {
    it('deve desabilitar botões quando isLoading é true', () => {
      render(<CancelConfirmationDialog {...defaultProps} isLoading={true} />);

      expect(screen.getByRole('button', { name: /voltar/i })).toBeDisabled();
      expect(screen.getByRole('button', { name: /cancelando/i })).toBeDisabled();
    });

    it('deve mostrar "Cancelando..." quando isLoading é true', () => {
      render(<CancelConfirmationDialog {...defaultProps} isLoading={true} />);

      expect(screen.getByText('Cancelando...')).toBeInTheDocument();
    });

    it('não deve desabilitar botões quando isLoading é false', () => {
      render(<CancelConfirmationDialog {...defaultProps} isLoading={false} />);

      expect(screen.getByRole('button', { name: /voltar/i })).not.toBeDisabled();
      expect(screen.getByRole('button', { name: /sim, cancelar/i })).not.toBeDisabled();
    });
  });

  describe('Controle de Visibilidade', () => {
    it('não deve renderizar quando open é false', () => {
      render(<CancelConfirmationDialog {...defaultProps} open={false} />);

      expect(screen.queryByText('Cancelar Agendamento')).not.toBeInTheDocument();
    });

    it('deve renderizar quando open é true', () => {
      render(<CancelConfirmationDialog {...defaultProps} open={true} />);

      expect(screen.getByText('Cancelar Agendamento')).toBeInTheDocument();
    });
  });

  describe('Estilo', () => {
    it('botão de confirmação deve ter estilo destrutivo', () => {
      render(<CancelConfirmationDialog {...defaultProps} />);

      const confirmButton = screen.getByRole('button', { name: /sim, cancelar/i });
      expect(confirmButton).toHaveClass('bg-destructive');
    });
  });
});
