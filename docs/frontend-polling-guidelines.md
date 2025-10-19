# 🔄 Diretrizes de Polling - Agenda do Barbeiro

## 📋 Visão Geral

Este documento define as diretrizes de implementação de polling para a tela de agenda do barbeiro, incluindo intervalo de atualização, cancelamento de requisições, tratamento de erros e boas práticas de UX.

## 🎯 Objetivos

- Manter a agenda do barbeiro atualizada de forma automática
- Minimizar carga no servidor através de polling inteligente
- Proporcionar feedback visual adequado ao usuário
- Garantir cancelamento de requisições ao sair da tela
- Tratar erros de forma resiliente com backoff exponencial

---

## ⏱️ Intervalo de Polling

### Configuração Padrão

- **Intervalo**: 10 segundos
- **Início**: Automático ao montar o componente da agenda
- **Parada**: Automático ao desmontar o componente

### Justificativa

O intervalo de 10 segundos oferece um balanço adequado entre:
- ✅ Atualização próxima ao tempo real (aceitável para o contexto de agendamentos)
- ✅ Baixa carga no servidor (6 requisições/minuto por barbeiro)
- ✅ Economia de dados para usuários mobile

### Quando NÃO fazer polling

- ❌ Quando a tela não está visível (ex: navegou para outra tela)
- ❌ Quando a aplicação está em background (mobile)
- ❌ Durante a execução de uma ação (confirm/cancel/complete)
- ❌ Quando há erro persistente (após backoff máximo)

---

## 🛑 Cancelamento de Polling

### Requisito Crítico

**SEMPRE** cancele requisições pendentes ao sair da tela ou desmontar o componente para evitar:
- Memory leaks
- Requisições desnecessárias
- Atualizações em componentes desmontados
- Erros de state em componentes inativos

### Implementação com AbortController

```typescript
import { useEffect, useRef, useState } from 'react';

function useBarberSchedulePolling(date: Date) {
  const [schedule, setSchedule] = useState<BarberScheduleOutput | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);
  const abortControllerRef = useRef<AbortController | null>(null);
  const pollingIntervalRef = useRef<NodeJS.Timeout | null>(null);

  const fetchSchedule = async () => {
    // Cancela requisição anterior se ainda estiver pendente
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    // Cria novo AbortController para esta requisição
    abortControllerRef.current = new AbortController();

    try {
      const response = await fetch(
        `/api/schedule/my-schedule?date=${formatDate(date)}`,
        {
          headers: {
            'Authorization': `Bearer ${getToken()}`,
          },
          signal: abortControllerRef.current.signal,
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
      }

      const data = await response.json();
      setSchedule(data);
      setError(null);
      setLoading(false);
    } catch (err) {
      // Ignora erros de abort (requisição cancelada intencionalmente)
      if (err instanceof Error && err.name === 'AbortError') {
        return;
      }

      setError(err as Error);
      setLoading(false);
    }
  };

  useEffect(() => {
    // Busca inicial
    fetchSchedule();

    // Configura polling a cada 10 segundos
    pollingIntervalRef.current = setInterval(() => {
      fetchSchedule();
    }, 10000);

    // Cleanup: cancela polling e requisição pendente ao desmontar
    return () => {
      if (pollingIntervalRef.current) {
        clearInterval(pollingIntervalRef.current);
      }
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, [date]); // Re-executa se a data mudar

  return { schedule, loading, error, refetch: fetchSchedule };
}
```

### Integração com Page Visibility API

Para pausar polling quando a página não está visível:

```typescript
useEffect(() => {
  const handleVisibilityChange = () => {
    if (document.hidden) {
      // Pausa polling quando página fica invisível
      if (pollingIntervalRef.current) {
        clearInterval(pollingIntervalRef.current);
        pollingIntervalRef.current = null;
      }
    } else {
      // Retoma polling quando página fica visível
      fetchSchedule(); // Busca imediata
      pollingIntervalRef.current = setInterval(fetchSchedule, 10000);
    }
  };

  document.addEventListener('visibilitychange', handleVisibilityChange);

  return () => {
    document.removeEventListener('visibilitychange', handleVisibilityChange);
  };
}, []);
```

---

## ❌ Tratamento de Erros

### Estratégias de Resiliência

#### 1. Backoff Exponencial

Ao detectar erro de rede, aumentar progressivamente o intervalo de polling:

```typescript
interface PollingConfig {
  baseInterval: number;      // 10000ms (10s)
  maxInterval: number;        // 60000ms (60s)
  backoffMultiplier: number;  // 2
  maxRetries: number;         // 5
}

const config: PollingConfig = {
  baseInterval: 10000,
  maxInterval: 60000,
  backoffMultiplier: 2,
  maxRetries: 5,
};

let currentInterval = config.baseInterval;
let retryCount = 0;

const scheduleNextPoll = (hasError: boolean) => {
  if (hasError) {
    retryCount++;
    
    if (retryCount >= config.maxRetries) {
      // Parar polling após max retries
      console.error('Max retries reached. Polling stopped.');
      return;
    }

    // Aumenta intervalo com backoff exponencial
    currentInterval = Math.min(
      currentInterval * config.backoffMultiplier,
      config.maxInterval
    );
  } else {
    // Reseta para intervalo normal em caso de sucesso
    currentInterval = config.baseInterval;
    retryCount = 0;
  }

  pollingIntervalRef.current = setTimeout(fetchSchedule, currentInterval);
};
```

#### 2. Classificação de Erros

| Tipo de Erro | HTTP Status | Ação |
|--------------|-------------|------|
| **Rede** | - | Aplicar backoff exponencial |
| **Unauthorized** | 401 | Redirecionar para login |
| **Forbidden** | 403 | Mostrar mensagem e parar polling |
| **Not Found** | 404 | Log de erro (não deveria ocorrer) |
| **Conflito** | 409 | Ignorar (não deveria ocorrer no GET) |
| **Server Error** | 500-599 | Aplicar backoff exponencial |

```typescript
const handleError = (error: Error, statusCode?: number) => {
  if (statusCode === 401) {
    // Token expirado - redirecionar para login
    redirectToLogin();
    return;
  }

  if (statusCode === 403) {
    // Sem permissão - mostrar mensagem e parar polling
    setError(new Error('Você não tem permissão para acessar esta agenda.'));
    return; // Não agenda próximo poll
  }

  if (statusCode && statusCode >= 500) {
    // Erro no servidor - aplicar backoff
    setError(new Error('Erro no servidor. Tentando novamente...'));
    scheduleNextPoll(true);
    return;
  }

  if (!navigator.onLine) {
    // Sem internet
    setError(new Error('Sem conexão com a internet.'));
    scheduleNextPoll(true);
    return;
  }

  // Erro genérico
  setError(error);
  scheduleNextPoll(true);
};
```

---

## 🎨 Estados de UI

### Loading States

#### 1. Loading Inicial (primeira carga)

```tsx
if (loading && !schedule) {
  return (
    <div className="flex flex-col items-center justify-center h-screen">
      <Spinner size="lg" />
      <p className="mt-4 text-gray-600">Carregando sua agenda...</p>
    </div>
  );
}
```

#### 2. Loading em Background (polling)

```tsx
// Mostrar indicador sutil durante polling
{loading && schedule && (
  <div className="fixed top-4 right-4 z-50">
    <Spinner size="sm" />
  </div>
)}
```

#### 3. Pull to Refresh (Mobile)

```tsx
import { useState } from 'react';

function AgendaScreen() {
  const [refreshing, setRefreshing] = useState(false);
  const { schedule, refetch } = useBarberSchedulePolling(new Date());

  const handlePullToRefresh = async () => {
    setRefreshing(true);
    await refetch();
    setRefreshing(false);
  };

  return (
    <PullToRefresh onRefresh={handlePullToRefresh} refreshing={refreshing}>
      {/* Conteúdo da agenda */}
    </PullToRefresh>
  );
}
```

### Empty States

```tsx
if (!loading && schedule && schedule.Appointments.length === 0) {
  return (
    <div className="flex flex-col items-center justify-center h-64">
      <CalendarIcon className="w-16 h-16 text-gray-400" />
      <h3 className="mt-4 text-lg font-medium text-gray-900">
        Nenhum agendamento
      </h3>
      <p className="mt-2 text-sm text-gray-500">
        Você não tem agendamentos para {formatDate(schedule.Date)}
      </p>
    </div>
  );
}
```

### Error States

```tsx
if (error && !schedule) {
  return (
    <div className="flex flex-col items-center justify-center h-64">
      <AlertCircleIcon className="w-16 h-16 text-red-500" />
      <h3 className="mt-4 text-lg font-medium text-gray-900">
        Erro ao carregar agenda
      </h3>
      <p className="mt-2 text-sm text-gray-500">{error.message}</p>
      <button
        onClick={refetch}
        className="mt-4 px-4 py-2 bg-primary text-white rounded"
      >
        Tentar novamente
      </button>
    </div>
  );
}
```

---

## 📱 Indicadores de Atualização

### Opções de Feedback Visual

#### 1. Badge de Atualização (Recomendado)

```tsx
{lastUpdated && (
  <div className="text-xs text-gray-500 text-center py-2">
    Atualizado há {formatRelativeTime(lastUpdated)}
  </div>
)}
```

#### 2. Barra de Progresso Sutil

```tsx
{isPolling && (
  <div className="fixed top-0 left-0 right-0 h-1 bg-blue-500 animate-pulse" />
)}
```

#### 3. Ícone de Sincronização

```tsx
<button onClick={refetch} disabled={loading} className="...">
  <RefreshIcon className={`w-5 h-5 ${loading ? 'animate-spin' : ''}`} />
</button>
```

---

## 🚀 Otimizações de Performance

### 1. Memoização de Componentes

```tsx
import { memo } from 'react';

const AppointmentCard = memo(({ appointment }: { appointment: BarberAppointmentOutput }) => {
  return (
    <div className="...">
      {/* Conteúdo do card */}
    </div>
  );
});
```

### 2. Comparação Inteligente de Dados

```tsx
const hasScheduleChanged = (prev: BarberScheduleOutput, next: BarberScheduleOutput) => {
  // Compara apenas campos relevantes
  if (prev.Appointments.length !== next.Appointments.length) return true;
  
  return prev.Appointments.some((prevAppt, index) => {
    const nextAppt = next.Appointments[index];
    return prevAppt.Status !== nextAppt.Status || prevAppt.Id !== nextAppt.Id;
  });
};

// Atualiza state apenas se houver mudança real
if (hasScheduleChanged(schedule, newSchedule)) {
  setSchedule(newSchedule);
}
```

### 3. Debounce de Ações do Usuário

```tsx
// Pausa polling durante ações do usuário
const pausePolling = () => {
  if (pollingIntervalRef.current) {
    clearInterval(pollingIntervalRef.current);
  }
};

const resumePolling = () => {
  pollingIntervalRef.current = setInterval(fetchSchedule, 10000);
};

const handleConfirm = async (id: string) => {
  pausePolling();
  try {
    await confirmAppointment(id);
    await refetch(); // Atualiza imediatamente
  } finally {
    resumePolling(); // Retoma polling
  }
};
```

---

## ⚠️ Edge Cases e Boas Práticas

### 1. Mudança de Data

Quando o usuário navega para outro dia:

```tsx
useEffect(() => {
  // Cancela polling anterior
  if (pollingIntervalRef.current) {
    clearInterval(pollingIntervalRef.current);
  }
  
  // Reseta estado
  setLoading(true);
  setError(null);
  
  // Inicia novo polling para a nova data
  fetchSchedule();
  pollingIntervalRef.current = setInterval(fetchSchedule, 10000);
}, [date]);
```

### 2. Troca de Contexto (Barbearia)

Quando o barbeiro troca de barbearia:

```tsx
useEffect(() => {
  // Cancela tudo e reinicia do zero
  if (pollingIntervalRef.current) {
    clearInterval(pollingIntervalRef.current);
  }
  if (abortControllerRef.current) {
    abortControllerRef.current.abort();
  }
  
  // Reseta completamente o estado
  setSchedule(null);
  setLoading(true);
  setError(null);
  retryCount = 0;
  currentInterval = config.baseInterval;
  
  // Inicia novo polling
  fetchSchedule();
  pollingIntervalRef.current = setInterval(fetchSchedule, 10000);
}, [currentBarbeariaId]);
```

### 3. Conflito de Status (409)

Embora raro no GET, tratar caso ocorra:

```tsx
if (response.status === 409) {
  // Status mudou, atualizar imediatamente
  await refetch();
}
```

### 4. Token Expirado Durante Polling

```tsx
const fetchWithTokenRefresh = async (url: string, options: RequestInit) => {
  let response = await fetch(url, options);
  
  if (response.status === 401) {
    // Tenta renovar token
    const newToken = await refreshToken();
    
    if (newToken) {
      // Retry com novo token
      options.headers = {
        ...options.headers,
        'Authorization': `Bearer ${newToken}`,
      };
      response = await fetch(url, options);
    } else {
      // Não conseguiu renovar - redirecionar para login
      redirectToLogin();
      throw new Error('Sessão expirada');
    }
  }
  
  return response;
};
```

---

## 📊 Monitoramento

### Métricas Recomendadas

```typescript
// Rastrear performance do polling
const trackPollingMetrics = () => {
  // Tempo de resposta
  const startTime = performance.now();
  
  fetchSchedule().finally(() => {
    const duration = performance.now() - startTime;
    analytics.track('schedule_polling', {
      duration_ms: duration,
      success: !error,
      retry_count: retryCount,
    });
  });
};
```

### Logs para Debug

```typescript
if (process.env.NODE_ENV === 'development') {
  console.log('[Polling] Schedule updated:', {
    date: schedule.Date,
    appointments: schedule.Appointments.length,
    interval: currentInterval,
    retries: retryCount,
  });
}
```

---

## ✅ Checklist de Implementação

- [ ] Implementar polling com intervalo de 10 segundos
- [ ] Configurar AbortController para cancelamento
- [ ] Implementar cleanup no useEffect
- [ ] Adicionar backoff exponencial para erros
- [ ] Integrar com Page Visibility API
- [ ] Implementar estados de loading (inicial e background)
- [ ] Implementar empty state
- [ ] Implementar error state com retry
- [ ] Adicionar pull-to-refresh (mobile)
- [ ] Pausar polling durante ações do usuário
- [ ] Tratar mudança de data
- [ ] Tratar troca de contexto (barbearia)
- [ ] Implementar indicador visual de atualização
- [ ] Adicionar logs de debug (dev mode)
- [ ] Testar cancelamento ao desmontar componente
- [ ] Testar comportamento em background
- [ ] Testar cenários de erro (offline, 500, etc.)

---

## 📚 Referências

- [MDN - AbortController](https://developer.mozilla.org/en-US/docs/Web/API/AbortController)
- [MDN - Page Visibility API](https://developer.mozilla.org/en-US/docs/Web/API/Page_Visibility_API)
- [React - useEffect Cleanup](https://react.dev/reference/react/useEffect#useeffect)
- [Exponential Backoff Pattern](https://en.wikipedia.org/wiki/Exponential_backoff)

---

**Última Atualização**: 2025-10-19  
**Versão**: 1.0  
**Autor**: Task 5.0 - Sistema de Agendamentos (Barbeiro)
