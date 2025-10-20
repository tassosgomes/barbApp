/**
 * AppointmentDetailsModal Component
 * 
 * Modal com detalhes completos do agendamento:
 * - Informações do cliente (nome, telefone)
 * - Detalhes do serviço (título, duração, valor)
 * - Horários (início, fim)
 * - Status atual
 * - Timestamps (criado, confirmado, concluído, cancelado)
 * - Botões de ação baseados no status
 */

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { StatusBadge } from './StatusBadge';
import { useAppointmentDetails } from '@/hooks/useAppointmentDetails';
import { AppointmentStatus } from '@/types/appointment';
import { format, parseISO, formatDistanceToNow } from 'date-fns';
import { ptBR } from 'date-fns/locale';
import {
  User,
  Phone,
  Scissors,
  Clock,
  DollarSign,
  Calendar,
  CheckCircle,
  XCircle,
  Check,
  AlertCircle,
} from 'lucide-react';
import { cn } from '@/lib/utils';

export interface AppointmentDetailsModalProps {
  /** ID do agendamento */
  appointmentId: string;
  /** Callback ao fechar o modal */
  onClose: () => void;
  /** Callback ao confirmar */
  onConfirm?: (id: string) => void;
  /** Callback ao cancelar */
  onCancel?: (id: string) => void;
  /** Callback ao concluir */
  onComplete?: (id: string) => void;
  /** Estado de loading das ações */
  isActionLoading?: boolean;
}

export function AppointmentDetailsModal({
  appointmentId,
  onClose,
  onConfirm,
  onCancel,
  onComplete,
  isActionLoading = false,
}: AppointmentDetailsModalProps) {
  const { data: appointment, isLoading, error } = useAppointmentDetails(appointmentId);

  // Determina quais ações estão disponíveis
  const showConfirmButton =
    appointment?.status === AppointmentStatus.Pending && onConfirm;
  const showCancelButton =
    (appointment?.status === AppointmentStatus.Pending ||
      appointment?.status === AppointmentStatus.Confirmed) &&
    onCancel;
  const showCompleteButton =
    appointment?.status === AppointmentStatus.Confirmed && onComplete;

  const hasActions = showConfirmButton || showCancelButton || showCompleteButton;

  // Formata timestamp relativo
  const formatTimestamp = (timestamp?: string) => {
    if (!timestamp) return null;
    const date = parseISO(timestamp);
    return {
      formatted: format(date, "dd/MM/yyyy 'às' HH:mm", { locale: ptBR }),
      relative: formatDistanceToNow(date, { locale: ptBR, addSuffix: true }),
    };
  };

  return (
    <Dialog open={true} onOpenChange={onClose}>
      <DialogContent className="max-w-md max-h-[90vh] overflow-y-auto" data-testid="appointment-details-modal">
        <DialogHeader>
          <DialogTitle>Detalhes do Agendamento</DialogTitle>
          <DialogDescription>
            Informações completas do agendamento
          </DialogDescription>
        </DialogHeader>

        {isLoading && <DetailsLoadingSkeleton />}

        {error && (
          <div className="flex flex-col items-center justify-center py-8 text-center">
            <AlertCircle className="h-12 w-12 text-destructive mb-4" />
            <p className="text-sm text-muted-foreground">
              Erro ao carregar detalhes do agendamento
            </p>
          </div>
        )}

        {appointment && (
          <div className="space-y-6">
            {/* Status */}
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium">Status</span>
              <StatusBadge status={appointment.status} />
            </div>

            {/* Informações do Cliente */}
            <div className="space-y-3">
              <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
                Cliente
              </h3>
              
              <div className="flex items-center gap-3">
                <User className="h-5 w-5 text-muted-foreground" />
                <div className="flex-1">
                  <p className="font-medium">{appointment.customerName}</p>
                </div>
              </div>

              {appointment.customerPhone && (
                <div className="flex items-center gap-3">
                  <Phone className="h-5 w-5 text-muted-foreground" />
                  <a
                    href={`tel:${appointment.customerPhone}`}
                    className="text-primary hover:underline"
                  >
                    {appointment.customerPhone}
                  </a>
                </div>
              )}
            </div>

            {/* Informações do Serviço */}
            <div className="space-y-3">
              <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
                Serviço
              </h3>

              <div className="flex items-center gap-3">
                <Scissors className="h-5 w-5 text-muted-foreground" />
                <div className="flex-1">
                  <p className="font-medium">{appointment.serviceTitle}</p>
                  {appointment.serviceDurationMinutes && (
                    <p className="text-sm text-muted-foreground">
                      Duração: {appointment.serviceDurationMinutes} min
                    </p>
                  )}
                </div>
              </div>

              {appointment.servicePrice && (
                <div className="flex items-center gap-3">
                  <DollarSign className="h-5 w-5 text-muted-foreground" />
                  <p className="font-medium">
                    R$ {appointment.servicePrice.toFixed(2)}
                  </p>
                </div>
              )}
            </div>

            {/* Horários */}
            <div className="space-y-3">
              <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
                Horário
              </h3>

              <div className="flex items-center gap-3">
                <Clock className="h-5 w-5 text-muted-foreground" />
                <div>
                  <p className="font-medium">
                    {format(parseISO(appointment.startTime), 'HH:mm', { locale: ptBR })}
                    {' - '}
                    {format(parseISO(appointment.endTime), 'HH:mm', { locale: ptBR })}
                  </p>
                  <p className="text-sm text-muted-foreground">
                    {format(parseISO(appointment.startTime), "dd 'de' MMMM 'de' yyyy", {
                      locale: ptBR,
                    })}
                  </p>
                </div>
              </div>
            </div>

            {/* Timestamps */}
            <div className="space-y-3">
              <h3 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
                Histórico
              </h3>

              {appointment.createdAt && (
                <div className="flex items-start gap-3">
                  <Calendar className="h-5 w-5 text-muted-foreground mt-0.5" />
                  <div>
                    <p className="text-sm font-medium">Criado</p>
                    <p className="text-xs text-muted-foreground">
                      {formatTimestamp(appointment.createdAt)?.relative}
                    </p>
                  </div>
                </div>
              )}

              {appointment.confirmedAt && (
                <div className="flex items-start gap-3">
                  <CheckCircle className="h-5 w-5 text-green-600 mt-0.5" />
                  <div>
                    <p className="text-sm font-medium">Confirmado</p>
                    <p className="text-xs text-muted-foreground">
                      {formatTimestamp(appointment.confirmedAt)?.relative}
                    </p>
                  </div>
                </div>
              )}

              {appointment.completedAt && (
                <div className="flex items-start gap-3">
                  <Check className="h-5 w-5 text-gray-600 mt-0.5" />
                  <div>
                    <p className="text-sm font-medium">Concluído</p>
                    <p className="text-xs text-muted-foreground">
                      {formatTimestamp(appointment.completedAt)?.relative}
                    </p>
                  </div>
                </div>
              )}

              {appointment.cancelledAt && (
                <div className="flex items-start gap-3">
                  <XCircle className="h-5 w-5 text-red-600 mt-0.5" />
                  <div>
                    <p className="text-sm font-medium">Cancelado</p>
                    <p className="text-xs text-muted-foreground">
                      {formatTimestamp(appointment.cancelledAt)?.relative}
                    </p>
                  </div>
                </div>
              )}
            </div>

            {/* Botões de Ação */}
            {hasActions && (
              <div className="flex flex-col gap-2 pt-4 border-t">
                {showConfirmButton && (
                  <Button
                    size="lg"
                    className="w-full min-h-[44px] gap-2"
                    onClick={() => onConfirm(appointment.id)}
                    disabled={isActionLoading}
                  >
                    <CheckCircle className="h-5 w-5" />
                    Confirmar Agendamento
                  </Button>
                )}

                {showCompleteButton && (
                  <Button
                    size="lg"
                    className="w-full min-h-[44px] gap-2"
                    onClick={() => onComplete(appointment.id)}
                    disabled={isActionLoading}
                  >
                    <Check className="h-5 w-5" />
                    Concluir Agendamento
                  </Button>
                )}

                {showCancelButton && (
                  <Button
                    size="lg"
                    variant="outline"
                    className={cn(
                      'w-full min-h-[44px] gap-2',
                      'text-destructive hover:bg-destructive hover:text-destructive-foreground'
                    )}
                    onClick={() => onCancel(appointment.id)}
                    disabled={isActionLoading}
                  >
                    <XCircle className="h-5 w-5" />
                    Cancelar Agendamento
                  </Button>
                )}
              </div>
            )}
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}

// Skeleton para estado de loading
function DetailsLoadingSkeleton() {
  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <Skeleton className="h-4 w-20" />
        <Skeleton className="h-8 w-32" />
      </div>
      <div className="space-y-2">
        <Skeleton className="h-4 w-24" />
        <Skeleton className="h-6 w-full" />
        <Skeleton className="h-6 w-3/4" />
      </div>
      <div className="space-y-2">
        <Skeleton className="h-4 w-24" />
        <Skeleton className="h-6 w-full" />
        <Skeleton className="h-4 w-2/3" />
      </div>
    </div>
  );
}
