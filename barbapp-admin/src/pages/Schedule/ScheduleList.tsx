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

  // Group appointments by barber
  const groupedAppointments = groupAppointmentsByBarber(appointments);

  return (
    <div className="space-y-8">
      {Object.entries(groupedAppointments).map(([barberName, barberAppointments]) => (
        <div key={barberName} className="space-y-3">
          <h3 className="text-lg font-semibold text-gray-900 border-b pb-2">
            {barberName}
            <span className="text-sm font-normal text-gray-500 ml-2">
              ({barberAppointments.length} {barberAppointments.length === 1 ? 'agendamento' : 'agendamentos'})
            </span>
          </h3>
          <div className="space-y-3">
            {barberAppointments
              .sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime())
              .map(appointment => (
                <AppointmentCard
                  key={appointment.id}
                  appointment={appointment}
                  currentDate={currentDate}
                />
              ))}
          </div>
        </div>
      ))}
    </div>
  );
}

// Helper function to group appointments by barber
function groupAppointmentsByBarber(appointments: Appointment[]): Record<string, Appointment[]> {
  return appointments.reduce((acc, appointment) => {
    const barberName = appointment.barberName;
    if (!acc[barberName]) {
      acc[barberName] = [];
    }
    acc[barberName].push(appointment);
    return acc;
  }, {} as Record<string, Appointment[]>);
}
