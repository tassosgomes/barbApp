/**
 * AppointmentsList Component
 * 
 * Lista de agendamentos do dia com ordenação cronológica.
 * Inclui estados de loading (skeleton) e empty state.
 */

import { Appointment } from '@/types/appointment';
import { AppointmentCard } from './AppointmentCard';
import { Skeleton } from '@/components/ui/skeleton';
import { Calendar } from 'lucide-react';

export interface AppointmentsListProps {
  appointments?: Appointment[];
  onConfirm?: (id: string) => void;
  onCancel?: (id: string) => void;
  onComplete?: (id: string) => void;
  onAppointmentClick?: (id: string) => void;
  isLoading?: boolean;
  loadingItemsCount?: number;
}

export function AppointmentsList({
  appointments,
  onConfirm,
  onCancel,
  onComplete,
  onAppointmentClick,
  isLoading = false,
  loadingItemsCount = 3,
}: AppointmentsListProps) {
  // Loading state
  if (isLoading) {
    return (
      <div className="space-y-4">
        {Array.from({ length: loadingItemsCount }).map((_, index) => (
          <AppointmentCardSkeleton key={index} />
        ))}
      </div>
    );
  }

  // Empty state
  if (!appointments || appointments.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-12 px-4 text-center">
        <div className="rounded-full bg-muted p-6 mb-4">
          <Calendar className="h-12 w-12 text-muted-foreground" />
        </div>
        <h3 className="text-lg font-semibold mb-2">Nenhum agendamento</h3>
        <p className="text-sm text-muted-foreground max-w-sm">
          Não há agendamentos para este dia. Aproveite para descansar ou realizar outras
          atividades.
        </p>
      </div>
    );
  }

  // Ordenar appointments por startTime (cronologicamente)
  const sortedAppointments = [...appointments].sort((a, b) => {
    return new Date(a.startTime).getTime() - new Date(b.startTime).getTime();
  });

  return (
    <div className="space-y-4" data-testid="appointments-list">
      {sortedAppointments.map((appointment) => (
        <AppointmentCard
          key={appointment.id}
          appointment={appointment}
          onConfirm={onConfirm}
          onCancel={onCancel}
          onComplete={onComplete}
          onClick={onAppointmentClick}
        />
      ))}
    </div>
  );
}

/**
 * Skeleton component para estado de loading
 */
function AppointmentCardSkeleton() {
  return (
    <div className="rounded-lg border border-l-4 border-l-gray-300 bg-card p-4 space-y-3">
      {/* Header skeleton */}
      <div className="flex items-start justify-between">
        <Skeleton className="h-5 w-24" />
        <Skeleton className="h-6 w-20" />
      </div>

      {/* Cliente skeleton */}
      <Skeleton className="h-6 w-40" />

      {/* Serviço skeleton */}
      <Skeleton className="h-5 w-32" />

      {/* Botões skeleton */}
      <div className="flex gap-2 pt-2 border-t">
        <Skeleton className="h-11 flex-1" />
        <Skeleton className="h-11 flex-1" />
      </div>
    </div>
  );
}
