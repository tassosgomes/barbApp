/**
 * StatusBadge Component
 * 
 * Badge visual para status de agendamento com cores e ícones consistentes.
 * Segue o design system do projeto com cores específicas para cada status.
 */

import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';
import { AppointmentStatus } from '@/types/appointment';
import { CheckCircle2, Clock, XCircle, CheckCheck } from 'lucide-react';

interface StatusBadgeProps {
  status: AppointmentStatus;
  className?: string;
}

const STATUS_CONFIG = {
  [AppointmentStatus.Pending]: {
    label: 'Pendente',
    icon: Clock,
    className: 'bg-yellow-100 text-yellow-800 border-yellow-300 hover:bg-yellow-100',
  },
  [AppointmentStatus.Confirmed]: {
    label: 'Confirmado',
    icon: CheckCircle2,
    className: 'bg-green-100 text-green-800 border-green-300 hover:bg-green-100',
  },
  [AppointmentStatus.Completed]: {
    label: 'Concluído',
    icon: CheckCheck,
    className: 'bg-gray-100 text-gray-800 border-gray-300 hover:bg-gray-100',
  },
  [AppointmentStatus.Cancelled]: {
    label: 'Cancelado',
    icon: XCircle,
    className: 'bg-red-100 text-red-800 border-red-300 hover:bg-red-100',
  },
} as const;

export function StatusBadge({ status, className }: StatusBadgeProps) {
  const config = STATUS_CONFIG[status];
  const Icon = config.icon;

  return (
    <Badge variant="outline" className={cn(config.className, 'gap-1', className)}>
      <Icon className="h-3 w-3" />
      <span>{config.label}</span>
    </Badge>
  );
}
