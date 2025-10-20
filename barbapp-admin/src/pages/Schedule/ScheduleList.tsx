import { Skeleton } from '@/components/ui/skeleton';
import { AppointmentCard } from './AppointmentCard';
import type { Appointment } from '@/types';

interface ScheduleListProps {
  appointments: Appointment[];
  isLoading: boolean;
  currentDate: string;
}

export function ScheduleList({ appointments, isLoading, currentDate }: ScheduleListProps) {
  if (isLoading) {
    return (
      <div className="space-y-4">
        {[...Array(5)].map((_, i) => (
          <Skeleton key={i} className="h-32 w-full" />
        ))}
      </div>
    );
  }

  if (appointments.length === 0) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-500 text-lg">Nenhum agendamento encontrado para esta data.</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {appointments
        .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime())
        .map(appointment => (
          <AppointmentCard
            key={appointment.id}
            appointment={appointment}
            currentDate={currentDate}
          />
        ))}
    </div>
  );
}
