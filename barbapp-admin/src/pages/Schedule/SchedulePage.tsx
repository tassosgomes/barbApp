import { useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useSchedule, useBarbers } from '@/hooks';
import { Button } from '@/components/ui/button';
import { DatePicker } from '@/components/ui/date-picker';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import { AppointmentStatus } from '@/types/schedule';
import { ScheduleList } from './ScheduleList';

export function SchedulePage() {
  const [searchParams, setSearchParams] = useSearchParams();

  // Get filters from URL
  const dateParam = searchParams.get('date') || getTodayISO();
  const barberIdParam = searchParams.get('barberId') || '';
  const statusParam = searchParams.get('status') || '';

  // Memoize filters
  const filters = useMemo(() => ({
    date: dateParam,
    barberId: barberIdParam || undefined,
    status: statusParam ? (Number(statusParam) as AppointmentStatus) : undefined,
  }), [dateParam, barberIdParam, statusParam]);

  // Fetch schedule data with polling
  const { data, isLoading, error } = useSchedule(filters);

  // Fetch barbers for filter dropdown (active only, no pagination needed)
  const { data: barbersData } = useBarbers({ isActive: true, pageSize: 100 });
  const barbers = barbersData?.items || [];

  // Update URL params
  const updateFilters = (newFilters: Partial<typeof filters>) => {
    const params = new URLSearchParams(searchParams);
    
    if (newFilters.date !== undefined) {
      params.set('date', newFilters.date);
    }
    
    if (newFilters.barberId !== undefined) {
      if (newFilters.barberId) {
        params.set('barberId', newFilters.barberId);
      } else {
        params.delete('barberId');
      }
    }
    
    if (newFilters.status !== undefined) {
      if (newFilters.status !== null) {
        params.set('status', String(newFilters.status));
      } else {
        params.delete('status');
      }
    }
    
    setSearchParams(params);
  };

  // Date navigation
  const handlePreviousDay = () => {
    const currentDate = new Date(dateParam);
    currentDate.setDate(currentDate.getDate() - 1);
    updateFilters({ date: formatDateISO(currentDate) });
  };

  const handleNextDay = () => {
    const currentDate = new Date(dateParam);
    currentDate.setDate(currentDate.getDate() + 1);
    updateFilters({ date: formatDateISO(currentDate) });
  };

  const handleToday = () => {
    updateFilters({ date: getTodayISO() });
  };

  const selectedDate = new Date(dateParam);
  const isToday = dateParam === getTodayISO();

  if (error) {
    return (
      <div className="p-6">
        <div className="text-center">
          <h2 className="text-xl font-semibold text-red-600">Erro ao carregar agenda</h2>
          <p className="text-gray-600 mt-2">Tente novamente mais tarde.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">Agenda</h1>
        {!isToday && (
          <Button variant="outline" onClick={handleToday}>
            Ir para Hoje
          </Button>
        )}
      </div>

      <div className="flex items-center gap-4">
        <Button
          variant="outline"
          size="icon"
          onClick={handlePreviousDay}
          aria-label="Dia anterior"
        >
          <ChevronLeft className="h-4 w-4" />
        </Button>
        
        <div className="flex-1 text-center">
          <h2 className="text-xl font-semibold">
            {formatDateDisplay(selectedDate)}
            {isToday && <span className="text-sm text-gray-500 ml-2">(Hoje)</span>}
          </h2>
        </div>

        <Button
          variant="outline"
          size="icon"
          onClick={handleNextDay}
          aria-label="Próximo dia"
        >
          <ChevronRight className="h-4 w-4" />
        </Button>
      </div>

      <div className="flex flex-wrap gap-4">
        <div className="flex flex-col gap-2 min-w-[200px]">
          <label htmlFor="date-filter" className="text-sm font-medium text-gray-700">Data</label>
          <DatePicker
            id="date-filter"
            value={dateParam}
            onChange={(value) => updateFilters({ date: value })}
          />
        </div>

        <div className="flex flex-col gap-2 min-w-[200px]">
          <label htmlFor="barber-filter" className="text-sm font-medium text-gray-700">Barbeiro</label>
          <Select
            value={barberIdParam || 'all'}
            onValueChange={(value) => updateFilters({ barberId: value === 'all' ? '' : value })}
          >
            <SelectTrigger id="barber-filter" aria-label="Filtrar por barbeiro">
              <SelectValue placeholder="Todos os barbeiros" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Todos os barbeiros</SelectItem>
              {barbers.map(barber => (
                <SelectItem key={barber.id} value={barber.id}>
                  {barber.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="flex flex-col gap-2 min-w-[200px]">
          <label htmlFor="status-filter" className="text-sm font-medium text-gray-700">Status</label>
          <Select
            value={statusParam || 'all'}
            onValueChange={(value) => updateFilters({ status: value === 'all' ? undefined : (Number(value) as AppointmentStatus) })}
          >
            <SelectTrigger id="status-filter" aria-label="Filtrar por status">
              <SelectValue placeholder="Todos os status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Todos os status</SelectItem>
              <SelectItem value={String(AppointmentStatus.Pending)}>Pendente</SelectItem>
              <SelectItem value={String(AppointmentStatus.Confirmed)}>Confirmado</SelectItem>
              <SelectItem value={String(AppointmentStatus.Cancelled)}>Cancelado</SelectItem>
              <SelectItem value={String(AppointmentStatus.Completed)}>Concluído</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      <ScheduleList
        appointments={data?.appointments || []}
        isLoading={isLoading}
        currentDate={dateParam}
      />
    </div>
  );
}

// Helper functions
function getTodayISO(): string {
  const today = new Date();
  return formatDateISO(today);
}

function formatDateISO(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function formatDateDisplay(date: Date): string {
  return new Intl.DateTimeFormat('pt-BR', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  }).format(date);
}
