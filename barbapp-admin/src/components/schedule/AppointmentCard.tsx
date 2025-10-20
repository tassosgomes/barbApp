/**
 * AppointmentCard Component
 * 
 * Card de agendamento individual com informações do cliente, serviço, horário e status.
 * Inclui botões de ação condicionais baseados no status do agendamento.
 * Design mobile-first com área de toque adequada (mínimo 44x44px).
 */

import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { Appointment, AppointmentStatus } from '@/types/appointment';
import { StatusBadge } from './StatusBadge';
import { format, parseISO } from 'date-fns';
import { ptBR } from 'date-fns/locale';
import { Clock, User, Scissors, CheckCircle, XCircle, Check } from 'lucide-react';

export interface AppointmentCardProps {
  appointment: Appointment;
  onConfirm?: (id: string) => void;
  onCancel?: (id: string) => void;
  onComplete?: (id: string) => void;
  onClick?: (id: string) => void;
  isLoading?: boolean;
}

// Mapeamento de cores de borda por status
const STATUS_BORDER_COLORS = {
  [AppointmentStatus.Pending]: 'border-l-yellow-500',
  [AppointmentStatus.Confirmed]: 'border-l-green-500',
  [AppointmentStatus.Completed]: 'border-l-gray-400',
  [AppointmentStatus.Cancelled]: 'border-l-red-500',
} as const;

// Mapeamento de cores de fundo por status
const STATUS_BG_COLORS = {
  [AppointmentStatus.Pending]: 'bg-yellow-50/50',
  [AppointmentStatus.Confirmed]: 'bg-green-50/50',
  [AppointmentStatus.Completed]: 'bg-gray-50/50',
  [AppointmentStatus.Cancelled]: 'bg-red-50/50',
} as const;

export function AppointmentCard({
  appointment,
  onConfirm,
  onCancel,
  onComplete,
  onClick,
  isLoading = false,
}: AppointmentCardProps) {
  const startTime = parseISO(appointment.startTime);
  const endTime = parseISO(appointment.endTime);

  const handleCardClick = () => {
    if (onClick && !isLoading) {
      onClick(appointment.id);
    }
  };

  const handleConfirm = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (onConfirm && !isLoading) {
      onConfirm(appointment.id);
    }
  };

  const handleCancel = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (onCancel && !isLoading) {
      onCancel(appointment.id);
    }
  };

  const handleComplete = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (onComplete && !isLoading) {
      onComplete(appointment.id);
    }
  };

  // Determina quais ações estão disponíveis baseado no status
  const showConfirmButton = appointment.status === AppointmentStatus.Pending && onConfirm;
  const showCancelButton =
    (appointment.status === AppointmentStatus.Pending ||
      appointment.status === AppointmentStatus.Confirmed) &&
    onCancel;
  const showCompleteButton = appointment.status === AppointmentStatus.Confirmed && onComplete;

  const hasActions = showConfirmButton || showCancelButton || showCompleteButton;

  return (
    <Card
      role="article"
      className={cn(
        'border-l-4 transition-all duration-200',
        STATUS_BORDER_COLORS[appointment.status],
        STATUS_BG_COLORS[appointment.status],
        onClick && 'cursor-pointer hover:shadow-md',
        isLoading && 'opacity-50 pointer-events-none'
      )}
      onClick={handleCardClick}
    >
      <CardContent className="p-4 space-y-3">
        {/* Header: Horário e Status */}
        <div className="flex items-start justify-between gap-2">
          <div className="flex items-center gap-2 text-sm font-medium">
            <Clock className="h-4 w-4 text-muted-foreground" />
            <span>
              {format(startTime, 'HH:mm', { locale: ptBR })} -{' '}
              {format(endTime, 'HH:mm', { locale: ptBR })}
            </span>
          </div>
          <StatusBadge status={appointment.status} />
        </div>

        {/* Cliente */}
        <div className="flex items-center gap-2">
          <User className="h-4 w-4 text-muted-foreground" />
          <span className="font-medium text-base">{appointment.customerName}</span>
        </div>

        {/* Serviço */}
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          <Scissors className="h-4 w-4" />
          <span>{appointment.serviceTitle}</span>
        </div>

        {/* Botões de Ação */}
        {hasActions && (
          <div className="flex gap-2 pt-2 border-t">
            {showConfirmButton && (
              <Button
                size="sm"
                variant="default"
                className="flex-1 min-h-[44px] gap-2"
                onClick={handleConfirm}
                disabled={isLoading}
              >
                <CheckCircle className="h-4 w-4" />
                Confirmar
              </Button>
            )}

            {showCompleteButton && (
              <Button
                size="sm"
                variant="default"
                className="flex-1 min-h-[44px] gap-2"
                onClick={handleComplete}
                disabled={isLoading}
              >
                <Check className="h-4 w-4" />
                Concluir
              </Button>
            )}

            {showCancelButton && (
              <Button
                size="sm"
                variant="outline"
                className={cn(
                  'min-h-[44px] gap-2',
                  showConfirmButton || showCompleteButton ? 'flex-1' : 'w-full'
                )}
                onClick={handleCancel}
                disabled={isLoading}
              >
                <XCircle className="h-4 w-4" />
                Cancelar
              </Button>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
