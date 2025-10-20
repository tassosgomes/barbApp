import { useQuery } from '@tanstack/react-query';
import { agendamentoService } from '@/services/agendamento.service';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { formatDateTime } from '@/utils/formatters';
import {
  translateAppointmentStatus,
  getAppointmentStatusClass,
} from '@/types/agendamento';
import { Calendar, User, Scissors } from 'lucide-react';

interface AgendamentoDetailsModalProps {
  agendamentoId: string;
  onClose: () => void;
}

/**
 * AgendamentoDetailsModal - Modal com detalhes completos do agendamento
 * 
 * Features:
 * - Exibe todas as informações do agendamento
 * - Formatação PT-BR de data/hora
 * - Badge colorido por status
 * - Loading state
 */
export function AgendamentoDetailsModal({
  agendamentoId,
  onClose,
}: AgendamentoDetailsModalProps) {
  const { data: agendamento, isLoading } = useQuery({
    queryKey: ['agendamento', agendamentoId],
    queryFn: () => agendamentoService.getById(agendamentoId),
  });

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Detalhes do Agendamento</DialogTitle>
        </DialogHeader>

        {isLoading ? (
          <div className="space-y-4">
            <Skeleton className="h-20 w-full" />
            <Skeleton className="h-20 w-full" />
            <Skeleton className="h-20 w-full" />
          </div>
        ) : (
          <div className="space-y-6">
            {/* Data e Hora */}
            <div className="flex items-start gap-3">
              <div className="mt-1">
                <Calendar className="h-5 w-5 text-gray-500" />
              </div>
              <div>
                <p className="text-sm font-medium text-gray-600">Data e Hora</p>
                <p className="text-lg font-semibold">
                  {formatDateTime(agendamento?.startTime || '')}
                </p>
                {agendamento?.endTime && (
                  <p className="text-sm text-gray-500">
                    até {formatDateTime(agendamento.endTime)}
                  </p>
                )}
              </div>
            </div>

            {/* Cliente */}
            <div className="flex items-start gap-3">
              <div className="mt-1">
                <User className="h-5 w-5 text-gray-500" />
              </div>
              <div className="flex-1">
                <p className="text-sm font-medium text-gray-600">Cliente</p>
                <p className="font-semibold">{agendamento?.customerName}</p>
                {/* Phone would be available if backend provides it */}
              </div>
            </div>

            {/* Barbeiro */}
            <div className="flex items-start gap-3">
              <div className="mt-1">
                <User className="h-5 w-5 text-gray-500" />
              </div>
              <div>
                <p className="text-sm font-medium text-gray-600">Barbeiro</p>
                <p className="font-semibold">{agendamento?.barberName}</p>
              </div>
            </div>

            {/* Serviço */}
            <div className="flex items-start gap-3">
              <div className="mt-1">
                <Scissors className="h-5 w-5 text-gray-500" />
              </div>
              <div>
                <p className="text-sm font-medium text-gray-600">Serviço</p>
                <p className="font-semibold">{agendamento?.serviceTitle}</p>
              </div>
            </div>

            {/* Status */}
            <div className="flex items-start gap-3">
              <div className="mt-1">
                <div className="h-5 w-5" /> {/* Spacer */}
              </div>
              <div>
                <p className="text-sm font-medium text-gray-600 mb-2">Status</p>
                <Badge
                  className={getAppointmentStatusClass(agendamento?.status ?? 0)}
                >
                  {translateAppointmentStatus(agendamento?.status ?? 0)}
                </Badge>
              </div>
            </div>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
