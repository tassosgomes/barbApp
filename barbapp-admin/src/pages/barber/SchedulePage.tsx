/**
 * BarberSchedulePage Component
 * 
 * Página principal de agenda do barbeiro com:
 * - Visualização da agenda do dia
 * - Navegação entre dias
 * - Polling automático de atualizações
 * - Ações de agendamento (confirmar, cancelar, concluir)
 * - Modal de detalhes
 * - Feedback visual via toast
 */

import { useState } from 'react';
import { addDays, subDays, startOfDay } from 'date-fns';
import { useBarberSchedule } from '@/hooks/useBarberSchedule';
import { useAppointmentActions } from '@/hooks/useAppointmentActions';
import { useToast } from '@/hooks/use-toast';
import {
  ScheduleHeader,
  AppointmentsList,
  AppointmentDetailsModal,
  CancelConfirmationDialog,
} from '@/components/schedule';
import { Skeleton } from '@/components/ui/skeleton';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { AlertCircle, WifiOff, RefreshCw } from 'lucide-react';
import { Button } from '@/components/ui/button';

export function BarberSchedulePage() {
  // Estado de data selecionada (iniciando com hoje)
  const [selectedDate, setSelectedDate] = useState<Date>(startOfDay(new Date()));
  
  // Estado do modal de detalhes
  const [selectedAppointmentId, setSelectedAppointmentId] = useState<string | null>(null);
  
  // Estado do dialog de confirmação de cancelamento
  const [cancelDialogOpen, setCancelDialogOpen] = useState(false);
  const [appointmentToCancel, setAppointmentToCancel] = useState<{
    id: string;
    customerName?: string;
  } | null>(null);

  // Hooks de dados e ações
  const { data: schedule, isLoading, error, refetch } = useBarberSchedule(selectedDate);
  const appointmentActions = useAppointmentActions();
  const { toast } = useToast();

  // Navegação de data
  const goToPreviousDay = () => setSelectedDate((prev) => subDays(prev, 1));
  const goToNextDay = () => setSelectedDate((prev) => addDays(prev, 1));
  const goToToday = () => setSelectedDate(startOfDay(new Date()));

  // Handlers de ações
  const handleConfirm = (id: string) => {
    appointmentActions.confirm(id, {
      onSuccess: () => {
        toast({
          title: 'Agendamento confirmado!',
          description: 'O cliente foi notificado sobre a confirmação.',
          variant: 'default',
        });
      },
      onError: (error: Error) => {
        handleError(error, 'confirmar');
      },
    });
  };

  const handleCancelClick = (id: string, customerName?: string) => {
    setAppointmentToCancel({ id, customerName });
    setCancelDialogOpen(true);
  };

  const handleCancelConfirm = () => {
    if (!appointmentToCancel) return;

    appointmentActions.cancel(appointmentToCancel.id, {
      onSuccess: () => {
        toast({
          title: 'Agendamento cancelado',
          description: 'O agendamento foi cancelado com sucesso.',
          variant: 'default',
        });
        setCancelDialogOpen(false);
        setAppointmentToCancel(null);
        
        // Fechar modal de detalhes se estiver aberto
        if (selectedAppointmentId === appointmentToCancel.id) {
          setSelectedAppointmentId(null);
        }
      },
      onError: (error: Error) => {
        handleError(error, 'cancelar');
      },
    });
  };

  const handleComplete = (id: string) => {
    appointmentActions.complete(id, {
      onSuccess: () => {
        toast({
          title: 'Agendamento concluído!',
          description: 'O serviço foi marcado como concluído.',
          variant: 'default',
        });
        
        // Fechar modal de detalhes se estiver aberto
        if (selectedAppointmentId === id) {
          setSelectedAppointmentId(null);
        }
      },
      onError: (error: Error) => {
        handleError(error, 'concluir');
      },
    });
  };

  const handleAppointmentClick = (id: string) => {
    setSelectedAppointmentId(id);
  };

  // Tratamento de erros
  const handleError = (error: any, action: string) => {
    // 403: Não autorizado (redirecionar para login seria tratado no interceptor)
    if (error.response?.status === 403) {
      toast({
        title: 'Sessão expirada',
        description: 'Por favor, faça login novamente.',
        variant: 'destructive',
      });
      return;
    }

    // 404: Agendamento não encontrado
    if (error.response?.status === 404) {
      toast({
        title: 'Agendamento não encontrado',
        description: 'O agendamento pode ter sido removido.',
        variant: 'destructive',
      });
      refetch(); // Recarregar lista
      return;
    }

    // 409: Conflito (agendamento foi modificado)
    if (error.response?.status === 409) {
      toast({
        title: 'Agendamento modificado',
        description: 'Este agendamento foi modificado. Recarregando...',
        variant: 'destructive',
      });
      refetch(); // Recarregar lista
      return;
    }

    // Erro genérico
    toast({
      title: `Erro ao ${action} agendamento`,
      description: error.response?.data?.message || error.message || 'Ocorreu um erro. Tente novamente.',
      variant: 'destructive',
    });
  };

  // Estado de loading das ações
  const isActionLoading = appointmentActions.isLoading;

  return (
    <div className="container mx-auto py-6 px-4 max-w-4xl">
      {/* Header com navegação de data */}
      <ScheduleHeader
        date={selectedDate}
        appointmentsCount={schedule?.appointments.length ?? 0}
        onPrevious={goToPreviousDay}
        onNext={goToNextDay}
        onToday={goToToday}
        onDateSelect={(date) => setSelectedDate(startOfDay(date))}
      />

      {/* Conteúdo principal */}
      <div className="mt-6">
        {/* Estado de Loading */}
        {isLoading && <ScheduleLoadingSkeleton />}

        {/* Estado de Erro */}
        {error && !isLoading && (
          <Alert variant="destructive" className="mb-4">
            <AlertCircle className="h-4 w-4" />
            <AlertDescription className="flex items-center justify-between">
              <span>
                {error.message === 'Network Error' ? (
                  <>
                    <WifiOff className="h-4 w-4 inline mr-2" />
                    Sem conexão com o servidor
                  </>
                ) : (
                  'Erro ao carregar agendamentos'
                )}
              </span>
              <Button
                variant="outline"
                size="sm"
                onClick={() => refetch()}
                className="ml-4"
              >
                <RefreshCw className="h-4 w-4 mr-2" />
                Tentar novamente
              </Button>
            </AlertDescription>
          </Alert>
        )}

        {/* Lista de Agendamentos */}
        {!isLoading && !error && schedule && (
          <AppointmentsList
            appointments={schedule.appointments}
            onAppointmentClick={handleAppointmentClick}
            onConfirm={handleConfirm}
            onCancel={(id) => {
              const appointment = schedule.appointments.find((a) => a.id === id);
              handleCancelClick(id, appointment?.customerName);
            }}
            onComplete={handleComplete}
            isLoading={isActionLoading}
          />
        )}
      </div>

      {/* Modal de Detalhes */}
      {selectedAppointmentId && (
        <AppointmentDetailsModal
          appointmentId={selectedAppointmentId}
          onClose={() => setSelectedAppointmentId(null)}
          onConfirm={handleConfirm}
          onCancel={(id) => {
            const appointment = schedule?.appointments.find((a) => a.id === id);
            handleCancelClick(id, appointment?.customerName);
          }}
          onComplete={handleComplete}
          isActionLoading={isActionLoading}
        />
      )}

      {/* Dialog de Confirmação de Cancelamento */}
      <CancelConfirmationDialog
        open={cancelDialogOpen}
        onOpenChange={setCancelDialogOpen}
        onConfirm={handleCancelConfirm}
        customerName={appointmentToCancel?.customerName}
        isLoading={appointmentActions.cancelState.isPending}
      />
    </div>
  );
}

// Skeleton para estado de loading
function ScheduleLoadingSkeleton() {
  return (
    <div className="space-y-4">
      {[1, 2, 3].map((i) => (
        <div key={i} className="border rounded-lg p-4 space-y-3">
          <div className="flex items-start justify-between">
            <Skeleton className="h-5 w-32" />
            <Skeleton className="h-6 w-24" />
          </div>
          <Skeleton className="h-6 w-48" />
          <Skeleton className="h-5 w-40" />
        </div>
      ))}
    </div>
  );
}
