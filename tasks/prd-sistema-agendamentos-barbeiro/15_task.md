---
status: completed
parallelizable: true
blocked_by: ["11.0","13.0","14.0"]
---

<task_context>
<domain>engine/frontend/pages</domain>
<type>implementation|testing</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies></dependencies>
<unblocks>"17.0"</unblocks>
</task_context>

# Tarefa 15.0: Página Principal - Agenda do Barbeiro

## Visão Geral
Criar a página principal de agenda do barbeiro com visualização do dia, navegação entre dias, polling automático, modal de detalhes e ações de agendamento.

## Requisitos
- Página `BarberSchedulePage` com agenda do dia atual
- Navegação entre dias (anterior/próximo)
- Seletor de data (date picker)
- Modal de detalhes do agendamento
- Modal de confirmação para cancelamento
- Integração com hooks de React Query
- Feedback visual de ações (toast/snackbar)
- Pull-to-refresh em mobile

## Subtarefas
- [x] 15.1 Criar `src/pages/barber/SchedulePage.tsx`:
  - Estado de data selecionada
  - useBarberSchedule hook com polling
  - Exibir AppointmentsList
  - Header com data e navegação
- [x] 15.2 Implementar navegação de data:
  - Botões "Dia Anterior" / "Próximo Dia"
  - Date picker para selecionar dia específico
  - Indicador de "Hoje"
- [x] 15.3 Criar `src/components/schedule/AppointmentDetailsModal.tsx`:
  - Exibir todos os detalhes do agendamento
  - Incluir botões de ação (confirmar/cancelar/concluir)
  - Informações de contato do cliente
  - Timestamps (criado, confirmado, etc.)
- [x] 15.4 Criar `src/components/schedule/CancelConfirmationDialog.tsx`:
  - Confirmação antes de cancelar
  - Texto: "Tem certeza que deseja cancelar este agendamento?"
- [x] 15.5 Implementar feedback de ações:
  - Toast de sucesso: "Agendamento confirmado!"
  - Toast de erro: mensagens de erro da API
  - Toast de conflito: "Este agendamento foi modificado. Recarregando..."
- [x] 15.6 Adicionar contador de agendamentos: "5 agendamentos hoje"
- [x] 15.7 Implementar pull-to-refresh (mobile)
- [x] 15.8 Estados de loading e error com UI apropriada
- [x] 15.9 Testes de integração da página

## Sequenciamento
- Bloqueado por: 11.0 (Tipos), 13.0 (Hooks), 14.0 (Componentes)
- Desbloqueia: 17.0
- Paralelizável: Sim

## Detalhes de Implementação

**Estrutura da Página:**
```typescript
export function BarberSchedulePage() {
  const [selectedDate, setSelectedDate] = useState(new Date());
  const [selectedAppointment, setSelectedAppointment] = useState<string | null>(null);
  
  const { data: schedule, isLoading, error } = useBarberSchedule(selectedDate);
  const { confirm, cancel, complete } = useAppointmentActions();
  
  // Navegação de data
  const goToPreviousDay = () => setSelectedDate(subDays(selectedDate, 1));
  const goToNextDay = () => setSelectedDate(addDays(selectedDate, 1));
  const goToToday = () => setSelectedDate(new Date());
  
  // Handlers de ações
  const handleConfirm = (id: string) => {
    confirm(id, {
      onSuccess: () => toast.success('Agendamento confirmado!'),
      onError: (error) => handleError(error)
    });
  };
  
  return (
    <div className="container">
      <ScheduleHeader 
        date={selectedDate}
        appointmentsCount={schedule?.appointments.length ?? 0}
        onPrevious={goToPreviousDay}
        onNext={goToNextDay}
        onToday={goToToday}
        onDateSelect={setSelectedDate}
      />
      
      {isLoading && <ScheduleSkeleton />}
      {error && <ErrorState error={error} />}
      {schedule && (
        <AppointmentsList
          appointments={schedule.appointments}
          onAppointmentClick={setSelectedAppointment}
          onConfirm={handleConfirm}
          onCancel={handleCancelClick}
          onComplete={handleComplete}
        />
      )}
      
      {selectedAppointment && (
        <AppointmentDetailsModal
          appointmentId={selectedAppointment}
          onClose={() => setSelectedAppointment(null)}
        />
      )}
    </div>
  );
}
```

**Header da Agenda:**
- Data formatada: "Terça-feira, 15 de Outubro"
- Contador: "5 agendamentos"
- Botões de navegação (< >) grandes
- Botão "Hoje" destacado se não estiver no dia atual

**Modal de Detalhes:**
- Exibir todas as informações do AppointmentDetails
- Telefone do cliente formatado e clicável (tel: link)
- Timestamps em formato legível (relativo se recente)
- Botões de ação baseados no status

**Tratamento de Erros:**
- 403: Redirecionar para login
- 404: "Agendamento não encontrado"
- 409: Recarregar agenda automaticamente

**Componentes Shadcn UI:**
- Dialog/Modal para detalhes
- AlertDialog para confirmação de cancelamento
- Calendar para date picker
- Toast/Sonner para feedback
- Skeleton para loading

## Critérios de Sucesso
- Página carrega agenda do dia atual
- Navegação entre dias funciona suavemente
- Polling atualiza dados a cada 10 segundos
- Ações (confirmar/cancelar/concluir) funcionam e atualizam UI
- Feedback visual claro para todas as ações
- Tratamento de erros apropriado
- Pull-to-refresh funciona em mobile
- Testes cobrem fluxos principais
- Segue regras de `rules/react.md` e `rules/tests-react.md`
