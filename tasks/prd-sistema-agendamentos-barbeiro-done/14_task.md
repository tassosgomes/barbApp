---
status: completed
parallelizable: true
blocked_by: ["11.0","13.0"]
---

<task_context>
<domain>engine/frontend/components</domain>
<type>implementation|testing</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies></dependencies>
<unblocks>"15.0","16.0"</unblocks>
</task_context>

# Tarefa 14.0: Componentes UI - Lista de Agendamentos e Card de Agendamento

## Visão Geral
Criar componentes reutilizáveis para exibir a lista de agendamentos do dia e cards individuais de agendamento com indicadores visuais de status e ações rápidas.

## Requisitos
- Componente `AppointmentCard` com informações do agendamento
- Componente `AppointmentsList` para lista do dia
- Indicadores visuais de status (cores)
- Botões de ação inline (confirmar/cancelar)
- Design mobile-first e touch-friendly
- Utilizar Shadcn UI components

## Subtarefas
- [x] 14.1 Criar `src/components/schedule/AppointmentCard.tsx`:
  - Props: appointment, onConfirm, onCancel, onComplete, onClick
  - Exibir: nome cliente, serviço, horário, status
  - Código de cores por status (Pending: yellow, Confirmed: green, Completed: gray, Cancelled: red)
  - Botões de ação condicionais baseados em status
  - Área de toque mínima 44x44px
- [x] 14.2 Criar `src/components/schedule/AppointmentsList.tsx`:
  - Recebe array de appointments
  - Renderiza lista de AppointmentCard
  - Empty state: "Nenhum agendamento para este dia"
  - Loading state com skeletons
  - Ordenação cronológica
- [x] 14.3 Criar `src/components/schedule/StatusBadge.tsx`:
  - Badge visual para status
  - Cores e ícones consistentes
- [x] 14.4 Implementar responsividade mobile-first
- [x] 14.5 Adicionar testes com React Testing Library

## Sequenciamento
- Bloqueado por: 11.0 (Tipos), 13.0 (Hooks)
- Desbloqueia: 15.0, 16.0
- Paralelizável: Sim

## Detalhes de Implementação

**AppointmentCard (estrutura):**
```typescript
interface AppointmentCardProps {
  appointment: Appointment;
  onConfirm?: (id: string) => void;
  onCancel?: (id: string) => void;
  onComplete?: (id: string) => void;
  onClick?: (id: string) => void;
}

export function AppointmentCard({ appointment, ...actions }: AppointmentCardProps) {
  // Status color mapping
  const statusColors = {
    [AppointmentStatus.Pending]: 'border-yellow-500 bg-yellow-50',
    [AppointmentStatus.Confirmed]: 'border-green-500 bg-green-50',
    [AppointmentStatus.Completed]: 'border-gray-400 bg-gray-50',
    [AppointmentStatus.Cancelled]: 'border-red-500 bg-red-50'
  };
  
  return (
    <Card className={cn('cursor-pointer', statusColors[appointment.status])}>
      <CardContent>
        {/* Horário e Cliente */}
        {/* Serviço */}
        {/* Status Badge */}
        {/* Botões de Ação */}
      </CardContent>
    </Card>
  );
}
```

**Código de Cores:**
- Pending: Amarelo/Laranja (warning)
- Confirmed: Verde (success)
- Completed: Cinza (neutral)
- Cancelled: Vermelho (destructive)

**Botões de Ação:**
- Pending: Mostrar "Confirmar" e "Cancelar"
- Confirmed: Mostrar "Cancelar" e "Concluir" (se horário passou)
- Completed/Cancelled: Sem ações

**Componentes Shadcn UI:**
- Card, CardContent, CardHeader
- Badge
- Button (variants)
- Skeleton (loading states)

## Critérios de Sucesso
- Cards renderizam corretamente com dados mockados
- Status visual é claro e consistente
- Botões funcionam e disparam callbacks
- Touch-friendly em mobile (área de toque adequada)
- Empty state e loading state funcionam
- Testes cobrem renderização e interações principais
- Segue regras de `rules/react.md` e `rules/tests-react.md`
