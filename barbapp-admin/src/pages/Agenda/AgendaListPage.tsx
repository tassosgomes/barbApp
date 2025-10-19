import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { agendamentoService } from '@/services/agendamento.service';
import { barbeiroService } from '@/services/barbeiro.service';
import { useBarbearia } from '@/contexts/BarbeariaContext';
import { DataTable } from '@/components/ui/data-table';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { formatDateTime } from '@/utils/formatters';
import {
  translateAppointmentStatus,
  getAppointmentStatusClass,
  type Appointment,
} from '@/types/agendamento';
import { Eye, Calendar } from 'lucide-react';
import { AgendamentoDetailsModal } from '@/components/AgendamentoDetailsModal';

/**
 * AgendaListPage - Página de visualização de agendamentos
 * 
 * Features:
 * - Listagem de agendamentos com DataTable
 * - Filtros: barbeiro, data inicial/final, status
 * - Modal de detalhes
 * - Badges coloridos por status
 * - Paginação
 * - Apenas visualização (sem edição)
 */
export function AgendaListPage() {
  const { barbearia } = useBarbearia();
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  
  // Filtros
  const [barbeiroId, setBarbeiroId] = useState<string>('');
  const [dataInicio, setDataInicio] = useState<string>('');
  const [dataFim, setDataFim] = useState<string>('');
  const [status, setStatus] = useState<string>('');
  
  // Modal
  const [selectedAgendamento, setSelectedAgendamento] = useState<string | null>(null);

  // Query: Lista de agendamentos
  const { data: agendamentos, isLoading } = useQuery({
    queryKey: ['agendamentos', page, pageSize, barbeiroId, dataInicio, dataFim, status],
    queryFn: () =>
      agendamentoService.list({
        page,
        pageSize,
        barberId: barbeiroId || undefined,
        startDate: dataInicio || undefined,
        endDate: dataFim || undefined,
        status: status || undefined,
      }),
    enabled: !!barbearia,
  });

  // Query: Lista de barbeiros (para filtro)
  const { data: barbeiros } = useQuery({
    queryKey: ['barbeiros-dropdown'],
    queryFn: () => barbeiroService.list({ page: 1, pageSize: 100 }),
    enabled: !!barbearia,
  });

  // Colunas da tabela
  const columns = [
    {
      header: 'Data/Hora',
      accessorKey: 'startTime',
      cell: ({ row }: any) => (
        <div className="flex items-center gap-2">
          <Calendar className="h-4 w-4 text-gray-500" />
          <span>{formatDateTime(row.original.startTime)}</span>
        </div>
      ),
    },
    {
      header: 'Cliente',
      accessorKey: 'customerName',
    },
    {
      header: 'Barbeiro',
      accessorKey: 'barberName',
    },
    {
      header: 'Serviço',
      accessorKey: 'serviceName',
    },
    {
      header: 'Status',
      accessorKey: 'status',
      cell: ({ row }: any) => {
        const status = row.original.status;
        const customClass = getAppointmentStatusClass(status);
        const label = translateAppointmentStatus(status);
        return <Badge className={customClass}>{label}</Badge>;
      },
    },
    {
      header: 'Ações',
      cell: ({ row }: any) => (
        <Button
          size="sm"
          variant="outline"
          onClick={() => setSelectedAgendamento(row.original.id)}
        >
          <Eye className="h-4 w-4 mr-1" />
          Ver Detalhes
        </Button>
      ),
    },
  ];

  // Limpar filtros
  const handleClearFilters = () => {
    setBarbeiroId('');
    setDataInicio('');
    setDataFim('');
    setStatus('');
    setPage(1);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold">Agendamentos</h1>
        <p className="text-gray-600 mt-1">
          Visualize todos os agendamentos da sua barbearia
        </p>
      </div>

      {/* Filtros */}
      <Card>
        <CardHeader>
          <CardTitle>Filtros</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            {/* Filtro: Barbeiro */}
            <div className="space-y-2">
              <Label htmlFor="barbeiro">Barbeiro</Label>
              <Select value={barbeiroId || 'all'} onValueChange={(value) => setBarbeiroId(value === 'all' ? '' : value)}>
                <SelectTrigger id="barbeiro">
                  <SelectValue placeholder="Todos os barbeiros" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos os barbeiros</SelectItem>
                  {barbeiros?.items.map((barbeiro) => (
                    <SelectItem key={barbeiro.id} value={barbeiro.id}>
                      {barbeiro.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Filtro: Data Inicial */}
            <div className="space-y-2">
              <Label htmlFor="dataInicio">Data Inicial</Label>
              <Input
                id="dataInicio"
                type="date"
                value={dataInicio}
                onChange={(e) => setDataInicio(e.target.value)}
              />
            </div>

            {/* Filtro: Data Final */}
            <div className="space-y-2">
              <Label htmlFor="dataFim">Data Final</Label>
              <Input
                id="dataFim"
                type="date"
                value={dataFim}
                onChange={(e) => setDataFim(e.target.value)}
              />
            </div>

            {/* Filtro: Status */}
            <div className="space-y-2">
              <Label htmlFor="status">Status</Label>
              <Select value={status || 'all'} onValueChange={(value) => setStatus(value === 'all' ? '' : value)}>
                <SelectTrigger id="status">
                  <SelectValue placeholder="Todos os status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos os status</SelectItem>
                  <SelectItem value="Pending">Pendente</SelectItem>
                  <SelectItem value="Confirmed">Confirmado</SelectItem>
                  <SelectItem value="Completed">Concluído</SelectItem>
                  <SelectItem value="Cancelled">Cancelado</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          {/* Botão Limpar Filtros */}
          <div className="mt-4">
            <Button variant="outline" onClick={handleClearFilters}>
              Limpar Filtros
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Tabela de Agendamentos */}
      <Card>
        <CardContent className="pt-6">
          <DataTable
            columns={columns}
            data={agendamentos}
            isLoading={isLoading}
            onPageChange={setPage}
            emptyMessage="Nenhum agendamento encontrado"
          />
        </CardContent>
      </Card>

      {/* Modal de Detalhes */}
      {selectedAgendamento && (
        <AgendamentoDetailsModal
          agendamentoId={selectedAgendamento}
          onClose={() => setSelectedAgendamento(null)}
        />
      )}
    </div>
  );
}
