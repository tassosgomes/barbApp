import { Badge } from '@/components/ui/badge';
import { Clock, User, Scissors } from 'lucide-react';
import type { Appointment } from '@/types/schedule';
import { AppointmentStatus } from '@/types/schedule';
import { cn } from '@/lib/utils';

interface AppointmentCardProps {
  appointment: Appointment;
  currentDate: string;
}

export function AppointmentCard({ appointment }: AppointmentCardProps) {
  const startTime = new Date(appointment.startTime);
  const endTime = new Date(appointment.endTime);
  const now = new Date();

  // Check if this appointment is happening right now
  const isHappeningNow = now >= startTime && now <= endTime;

  // Format time in pt-BR
  const formatTime = (date: Date) => {
    return new Intl.DateTimeFormat('pt-BR', {
      hour: '2-digit',
      minute: '2-digit',
    }).format(date);
  };

  const startTimeStr = formatTime(startTime);
  const endTimeStr = formatTime(endTime);

  return (
    <div
      className={cn(
        'border rounded-lg p-4 hover:shadow-md transition-shadow bg-white',
        isHappeningNow && 'ring-2 ring-blue-500 bg-blue-50'
      )}
    >
      <div className="flex items-start justify-between gap-4">
        <div className="flex-1 space-y-2">
          {/* Time */}
          <div className="flex items-center gap-2 text-sm font-medium">
            <Clock className="h-4 w-4 text-gray-500" />
            <span className={cn(
              isHappeningNow && 'text-blue-700 font-semibold'
            )}>
              {startTimeStr} - {endTimeStr}
            </span>
            {isHappeningNow && (
              <Badge variant="default" className="ml-2 bg-blue-600">
                Em andamento
              </Badge>
            )}
          </div>

          {/* Customer */}
          <div className="flex items-center gap-2 text-sm">
            <User className="h-4 w-4 text-gray-500" />
            <span className="font-medium">{appointment.customerName}</span>
          </div>

          {/* Service */}
          <div className="flex items-center gap-2 text-sm text-gray-600">
            <Scissors className="h-4 w-4 text-gray-500" />
            <span>{appointment.serviceTitle}</span>
          </div>
        </div>

        {/* Status Badge */}
        <div>
          <StatusBadge status={appointment.status} />
        </div>
      </div>
    </div>
  );
}

// Status Badge Component
function StatusBadge({ status }: { status: AppointmentStatus }) {
  const statusConfig: Record<AppointmentStatus, { label: string; variant: 'default' | 'secondary' | 'destructive' | 'outline' }> = {
    [AppointmentStatus.Pending]: {
      label: 'Pendente',
      variant: 'outline',
    },
    [AppointmentStatus.Confirmed]: {
      label: 'Confirmado',
      variant: 'default',
    },
    [AppointmentStatus.Cancelled]: {
      label: 'Cancelado',
      variant: 'destructive',
    },
    [AppointmentStatus.Completed]: {
      label: 'Concluído',
      variant: 'secondary',
    },
  };

  const config = statusConfig[status];

  return (
    <Badge variant={config.variant} className={cn(
      config.variant === 'default' && 'bg-green-600 hover:bg-green-700',
    )}>
      {config.label}
    </Badge>
  );
}
