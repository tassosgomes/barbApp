---
status: completed
parallelizable: true
blocked_by: ["11.0"]
---

<task_context>
<domain>engine/frontend/navigation</domain>
<type>implementation|testing</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies></dependencies>
<unblocks>"15.0"</unblocks>
</task_context>

# Tarefa 16.0: Seletor de Contexto Multi-Barbearia

## Visão Geral
Implementar seletor de contexto (dropdown/menu) no header para barbeiros que trabalham em múltiplas barbearias, permitindo trocar facilmente entre diferentes estabelecimentos e suas respectivas agendas.

## Requisitos
- Componente de seletor sempre visível no header
- Lista de barbearias onde o barbeiro trabalha
- Persistir contexto selecionado na sessão
- Indicador visual da barbearia atual
- Atualizar agenda ao trocar de barbearia
- Fluxo de login: redirecionar direto se apenas 1 barbearia

## Subtarefas
- [x] 16.1 Criar `src/components/layout/BarbershopSelector.tsx`:
  - Dropdown com lista de barbearias
  - Exibir nome da barbearia atual
  - Ícone/badge da barbearia selecionada
- [x] 16.2 Criar hook `useBarbershopContext.ts`:
  - Gerenciar barbearia selecionada
  - Persistir no sessionStorage
  - Função para trocar contexto
- [x] 16.3 Criar `src/pages/barber/SelectBarbershopPage.tsx`:
  - Página de seleção inicial (se múltiplas barbearias)
  - Lista visual de barbearias
  - Redirecionamento após seleção
- [x] 16.4 Implementar lógica de login/redirecionamento:
  - Se 1 barbearia: redirecionar direto para agenda
  - Se múltiplas: mostrar página de seleção
- [x] 16.5 Atualizar contexto de autenticação:
  - Incluir barbeariaId no token/contexto
  - Validar acesso do barbeiro à barbearia selecionada
- [x] 16.6 Implementar invalidação de queries ao trocar contexto
- [x] 16.7 Testes de fluxo de seleção e troca de contexto

## Sequenciamento
- Bloqueado por: 11.0 (Tipos)
- Desbloqueia: 15.0 (depende indiretamente)
- Paralelizável: Sim

## Detalhes de Implementação

**Hook de Contexto:**
```typescript
// useBarbershopContext.ts
interface BarbershopContext {
  barbershopId: string;
  barbershopName: string;
}

export function useBarbershopContext() {
  const [context, setContext] = useState<BarbershopContext | null>(() => {
    const stored = sessionStorage.getItem('barbershop-context');
    return stored ? JSON.parse(stored) : null;
  });
  
  const queryClient = useQueryClient();
  
  const selectBarbershop = (barbershop: BarbershopContext) => {
    setContext(barbershop);
    sessionStorage.setItem('barbershop-context', JSON.stringify(barbershop));
    
    // Invalidar todas as queries ao trocar de contexto
    queryClient.invalidateQueries();
  };
  
  return {
    currentBarbershop: context,
    selectBarbershop,
    isSelected: !!context
  };
}
```

**BarbershopSelector Component:**
```typescript
export function BarbershopSelector() {
  const { currentBarbershop, availableBarbershops } = useBarbershopContext();
  
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="outline" className="min-w-[200px]">
          <Building2 className="mr-2 h-4 w-4" />
          {currentBarbershop?.name || 'Selecione a barbearia'}
          <ChevronDown className="ml-auto h-4 w-4" />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-[200px]">
        {availableBarbershops.map((barbershop) => (
          <DropdownMenuItem
            key={barbershop.id}
            onClick={() => handleSelect(barbershop)}
          >
            {barbershop.name}
            {currentBarbershop?.id === barbershop.id && (
              <Check className="ml-auto h-4 w-4" />
            )}
          </DropdownMenuItem>
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
```

**Página de Seleção Inicial:**
- Lista de cards das barbearias
- Visual atraente e touch-friendly
- "Trabalhe em: [Nome da Barbearia]"
- Redirecionamento automático após seleção

**Fluxo de Autenticação:**
1. Barbeiro faz login
2. Sistema busca lista de barbearias vinculadas
3. Se 1: redireciona direto para `/barber/schedule`
4. Se múltiplas: redireciona para `/barber/select-barbershop`
5. Após seleção: salva contexto e vai para `/barber/schedule`

**Indicador Visual:**
- Nome da barbearia sempre visível no header
- Cor ou badge diferenciado
- Facilmente identificável

**Tratamento de Segurança:**
- Backend valida que barbeiro tem acesso à barbearia
- Token JWT deve incluir barbeariaId
- Requisições incluem contexto atual

## Critérios de Sucesso
- Seletor exibe barbearias corretamente
- Trocar de contexto atualiza agenda automaticamente
- Contexto persiste ao recarregar página (sessão)
- Fluxo de login funciona para 1 ou múltiplas barbearias
- Indicador visual da barbearia atual é claro
- Queries são invalidadas ao trocar contexto
- Testes cobrem fluxos de seleção e troca
- Segue requisitos de isolamento multi-tenant
