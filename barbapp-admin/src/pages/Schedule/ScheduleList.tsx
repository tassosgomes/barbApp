import { Skeleton } from '@/components/ui/skeleton';
import { AppointmentCard } from './AppointmentCard';
import type { Appointment } from '@/types/schedule';

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

  const groupedAppointments = appointments.reduce<Record<string, { barberName: string; items: Appointment[] }>>((acc, appointment) => {
    const group = acc[appointment.barberId] ?? {
      barberName: appointment.barberName,
      items: [],
    };

    group.items.push(appointment);
    acc[appointment.barberId] = group;
    return acc;
  }, {});

  const sortedGroups = Object.values(groupedAppointments).sort((a, b) =>
    a.barberName.localeCompare(b.barberName, 'pt-BR')
  );

  const formatCountLabel = (count: number) =>
    `${count} agendamento${count === 1 ? '' : 's'}`;

  return (
    <div className="space-y-6">
      {sortedGroups.map(({ barberName, items }) => (
        <section key={barberName} className="space-y-3 rounded-lg border p-4">
          <header className="flex flex-col gap-1 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <p className="text-lg font-semibold">{barberName}</p>
              <p className="text-sm text-gray-500">{formatCountLabel(items.length)}</p>
            </div>
          </header>
          <div className="space-y-4">
            {items
              .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime())
              .map((appointment) => (
                <AppointmentCard
                  key={appointment.id}
                  appointment={appointment}
                  currentDate={currentDate}
                />
              ))}
          </div>
        </section>
      ))}
    </div>
  );
}
