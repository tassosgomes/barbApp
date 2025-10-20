/**
 * ScheduleHeader Component
 * 
 * Header da página de agenda com:
 * - Data atual formatada
 * - Contador de agendamentos
 * - Navegação entre dias (anterior/próximo)
 * - Botão "Hoje" para voltar ao dia atual
 * - Date picker para seleção direta de data
 */

import { Button } from '@/components/ui/button';
import { DatePicker } from '@/components/ui/date-picker';
import { format, isToday } from 'date-fns';
import { ptBR } from 'date-fns/locale';
import { ChevronLeft, ChevronRight, Calendar as CalendarIcon } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface ScheduleHeaderProps {
  /** Data selecionada */
  date: Date;
  /** Número de agendamentos no dia */
  appointmentsCount: number;
  /** Callback para ir ao dia anterior */
  onPrevious: () => void;
  /** Callback para ir ao próximo dia */
  onNext: () => void;
  /** Callback para ir ao dia atual */
  onToday: () => void;
  /** Callback para selecionar uma data específica */
  onDateSelect: (date: Date) => void;
}

export function ScheduleHeader({
  date,
  appointmentsCount,
  onPrevious,
  onNext,
  onToday,
  onDateSelect,
}: ScheduleHeaderProps) {
  const isCurrentDay = isToday(date);

  // Formata a data no formato: "Terça-feira, 15 de Outubro"
  const formattedDate = format(date, "EEEE, dd 'de' MMMM", { locale: ptBR });
  const capitalizedDate = formattedDate.charAt(0).toUpperCase() + formattedDate.slice(1);

  // Formata a data para o input type="date" (yyyy-MM-dd)
  const inputDateValue = format(date, 'yyyy-MM-dd');

  const handleDateChange = (value: string) => {
    if (value) {
      const newDate = new Date(value + 'T00:00:00');
      onDateSelect(newDate);
    }
  };

  return (
    <div className="space-y-4 pb-4 border-b" data-testid="schedule-header">
      {/* Linha 1: Data e Contador */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground" data-testid="current-date">
            {capitalizedDate}
          </h1>
          <p className="text-sm text-muted-foreground mt-1" data-testid="appointments-count">
            {appointmentsCount === 0 && 'Nenhum agendamento'}
            {appointmentsCount === 1 && '1 agendamento'}
            {appointmentsCount > 1 && `${appointmentsCount} agendamentos`}
          </p>
        </div>

        {/* Botão "Hoje" - apenas se não estiver no dia atual */}
        {!isCurrentDay && (
          <Button
            variant="outline"
            size="sm"
            onClick={onToday}
            className="gap-2"
            data-testid="today-btn"
          >
            <CalendarIcon className="h-4 w-4" />
            Hoje
          </Button>
        )}
      </div>

      {/* Linha 2: Navegação e Date Picker */}
      <div className="flex items-center gap-2">
        {/* Botão Dia Anterior */}
        <Button
          variant="outline"
          size="icon"
          onClick={onPrevious}
          aria-label="Dia anterior"
          className="min-h-[44px] min-w-[44px]"
          data-testid="prev-day-btn"
        >
          <ChevronLeft className="h-5 w-5" />
        </Button>

        {/* Date Picker */}
        <div className="flex-1">
          <DatePicker
            value={inputDateValue}
            onChange={handleDateChange}
            placeholder="Selecione uma data"
            className={cn(
              'text-center font-medium',
              isCurrentDay && 'border-primary'
            )}
          />
        </div>

        {/* Botão Próximo Dia */}
        <Button
          variant="outline"
          size="icon"
          onClick={onNext}
          aria-label="Próximo dia"
          className="min-h-[44px] min-w-[44px]"
          data-testid="next-day-btn"
        >
          <ChevronRight className="h-5 w-5" />
        </Button>
      </div>
    </div>
  );
}
