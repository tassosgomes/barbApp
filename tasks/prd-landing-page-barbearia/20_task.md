---
status: pending
parallelizable: false
blocked_by: ["11.0", "12.0", "13.0", "14.0", "15.0", "16.0", "17.0", "18.0"]
---

# Tarefa 20.0: Testes do Frontend do Admin

## Visão Geral
Criar um conjunto de testes unitários e de integração para os novos componentes e hooks desenvolvidos para o módulo de gerenciamento da Landing Page no painel de administração. O objetivo é garantir a qualidade, a estabilidade e o comportamento esperado de cada parte da funcionalidade.

## Requisitos
- **Cobertura de Teste**: Focar em testar a lógica de negócio dos hooks e as interações principais dos componentes.
- **Ferramentas**: Utilizar a stack de testes já estabelecida no projeto, que provavelmente inclui `Vitest` (ou `Jest`) e `React Testing Library`.
- **Mock de API**: Os testes que dependem de chamadas de API devem usar mocks para isolar o frontend do backend.

## Detalhes de Implementação (techspec-frontend.md Seção 6)

### Testes de Hooks

- **`useLandingPage.ts`**:
  - Testar o estado inicial (`isLoading`, `data` como `undefined`).
  - Mockar a API `landingPageApi.getConfig` e verificar se o hook retorna os dados corretamente após a chamada.
  - Mockar a API `landingPageApi.updateConfig`, chamar a função `updateConfig` do hook e verificar se a mutação foi chamada com os argumentos corretos.
  - Testar os estados de sucesso e erro da mutação.

- **`useLogoUpload.ts`**:
  - Testar a lógica de validação de arquivo (tamanho e tipo).
  - Simular a seleção de um arquivo válido e verificar se a função de upload é chamada.
  - Simular a seleção de um arquivo inválido e verificar se a função de upload não é chamada e um erro é reportado.

### Testes de Componentes

- **`ServiceManager.tsx`**:
  - Testar a renderização inicial da lista de serviços.
  - Simular um clique no checkbox de visibilidade e verificar se o callback `onChange` é chamado com os dados corretos.
  - Testar as ações "Selecionar todos" e "Desmarcar todos".
  - (Opcional/Avançado) Testar a funcionalidade de drag and drop, se a biblioteca de testes permitir.

- **`LandingPageForm.tsx`**:
  - Testar se o formulário é preenchido com os dados iniciais.
  - Simular a entrada do usuário em diferentes campos e a submissão do formulário.
  - Verificar se a função `updateConfig` é chamada com o payload correto ao submeter.
  - Testar as validações de campo (ex: inserir um e-mail inválido e verificar se a mensagem de erro aparece).

- **`TemplateGallery.tsx`**:
  - Testar se a galeria renderiza o número correto de templates.
  - Simular um clique em um template e verificar se o callback `onSelectTemplate` é chamado com o ID correto.
  - Verificar se o template selecionado tem o destaque visual (classe CSS ou ícone).

## Exemplo de Teste (com Vitest e React Testing Library)

```typescript
// __tests__/useServiceSelection.test.ts (exemplo do techspec)
import { renderHook, act } from '@testing-library/react';
import { useServiceSelection } from '@/hooks/useServiceSelection';

const mockServices = [
  { id: '1', name: 'Corte', price: 35, duration: 30 },
  { id: '2', name: 'Barba', price: 25, duration: 20 },
];

describe('useServiceSelection', () => {
  it('should toggle service selection', () => {
    const { result } = renderHook(() => useServiceSelection(mockServices));

    act(() => {
      result.current.toggleService('1');
    });

    expect(result.current.selectedIds.has('1')).toBe(true);
    expect(result.current.totalPrice).toBe(35);
  });
});
```

## Critérios de Aceitação
- [ ] Testes unitários para o hook `useLandingPage` foram criados e estão passando.
- [ ] Testes unitários para o hook `useLogoUpload` foram criados e estão passando.
- [ ] Testes de integração para o componente `ServiceManager` cobrindo as principais interações foram criados e estão passando.
- [ ] Testes de integração para o componente `LandingPageForm` cobrindo validação e submissão foram criados e estão passando.
- [ ] Testes de interação para o componente `TemplateGallery` foram criados e estão passando.
- [ ] A cobertura de teste para os novos arquivos atinge o limite mínimo definido pelo projeto (se houver).