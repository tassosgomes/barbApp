import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ScheduleHeader } from '../ScheduleHeader';

describe('ScheduleHeader', () => {
  const defaultProps = {
    date: new Date('2025-10-20T12:00:00'),
    appointmentsCount: 0,
    onPrevious: vi.fn(),
    onNext: vi.fn(),
    onToday: vi.fn(),
    onDateSelect: vi.fn(),
  };

  describe('Renderização', () => {
    it('deve renderizar a data formatada corretamente', () => {
      render(<ScheduleHeader {...defaultProps} />);

      // Deve exibir: "Segunda-feira, 20 de Outubro"
      expect(screen.getByText(/segunda-feira, 20 de outubro/i)).toBeInTheDocument();
    });

    it('deve mostrar "Nenhum agendamento" quando count é 0', () => {
      render(<ScheduleHeader {...defaultProps} appointmentsCount={0} />);

      expect(screen.getByText('Nenhum agendamento')).toBeInTheDocument();
    });

    it('deve mostrar "1 agendamento" quando count é 1', () => {
      render(<ScheduleHeader {...defaultProps} appointmentsCount={1} />);

      expect(screen.getByText('1 agendamento')).toBeInTheDocument();
    });

    it('deve mostrar "5 agendamentos" quando count é maior que 1', () => {
      render(<ScheduleHeader {...defaultProps} appointmentsCount={5} />);

      expect(screen.getByText('5 agendamentos')).toBeInTheDocument();
    });
  });

  describe('Botão "Hoje"', () => {
    it('deve mostrar botão "Hoje" quando não está no dia atual', () => {
      const pastDate = new Date('2025-01-15T12:00:00');
      render(<ScheduleHeader {...defaultProps} date={pastDate} />);

      expect(screen.getByRole('button', { name: /hoje/i })).toBeInTheDocument();
    });

    it('não deve mostrar botão "Hoje" quando já está no dia atual', () => {
      const today = new Date();
      render(<ScheduleHeader {...defaultProps} date={today} />);

      expect(screen.queryByRole('button', { name: /hoje/i })).not.toBeInTheDocument();
    });

    it('deve chamar onToday quando botão "Hoje" é clicado', async () => {
      const user = userEvent.setup();
      const pastDate = new Date('2025-01-15T12:00:00');
      const onToday = vi.fn();

      render(<ScheduleHeader {...defaultProps} date={pastDate} onToday={onToday} />);

      await user.click(screen.getByRole('button', { name: /hoje/i }));
      expect(onToday).toHaveBeenCalledTimes(1);
    });
  });

  describe('Navegação', () => {
    it('deve chamar onPrevious quando botão de dia anterior é clicado', async () => {
      const user = userEvent.setup();
      const onPrevious = vi.fn();

      render(<ScheduleHeader {...defaultProps} onPrevious={onPrevious} />);

      await user.click(screen.getByLabelText('Dia anterior'));
      expect(onPrevious).toHaveBeenCalledTimes(1);
    });

    it('deve chamar onNext quando botão de próximo dia é clicado', async () => {
      const user = userEvent.setup();
      const onNext = vi.fn();

      render(<ScheduleHeader {...defaultProps} onNext={onNext} />);

      await user.click(screen.getByLabelText('Próximo dia'));
      expect(onNext).toHaveBeenCalledTimes(1);
    });
  });

  describe('Date Picker', () => {
    it('deve exibir date picker com valor correto', () => {
      render(<ScheduleHeader {...defaultProps} date={new Date('2025-10-20T12:00:00')} />);

      const datePicker = screen.getByPlaceholderText('Selecione uma data') as HTMLInputElement;
      expect(datePicker).toBeInTheDocument();
      expect(datePicker.value).toBe('2025-10-20');
    });

    it('date picker deve ter valor correto quando renderizado', () => {
      render(<ScheduleHeader {...defaultProps} date={new Date('2025-10-20T12:00:00')} />);

      const datePicker = screen.getByPlaceholderText('Selecione uma data') as HTMLInputElement;
      expect(datePicker).toBeInTheDocument();
      expect(datePicker.value).toBe('2025-10-20');
      expect(datePicker.type).toBe('date');
    });
  });

  describe('Acessibilidade', () => {
    it('deve ter botões de navegação com áreas de toque adequadas', () => {
      render(<ScheduleHeader {...defaultProps} />);

      const prevButton = screen.getByLabelText('Dia anterior');
      const nextButton = screen.getByLabelText('Próximo dia');

      expect(prevButton).toHaveClass('min-h-[44px]');
      expect(prevButton).toHaveClass('min-w-[44px]');
      expect(nextButton).toHaveClass('min-h-[44px]');
      expect(nextButton).toHaveClass('min-w-[44px]');
    });
  });
});
