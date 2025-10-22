# LandingPageForm Component

Formulário principal no painel de administração para personalizar a landing page da barbearia.

## Visão Geral

O `LandingPageForm` é o componente central para gerenciamento da landing page, integrando outros componentes menores (`LogoUploader` e `ServiceManager`) e coordenando a submissão de dados para a API.

## Funcionalidades

- ✅ Edição de informações da barbearia
- ✅ Upload/gerenciamento de logo
- ✅ Configuração de serviços exibidos
- ✅ Validação em tempo real de todos os campos
- ✅ Feedback de erro/sucesso
- ✅ Estados de loading
- ✅ Modo readonly (opcional)
- ✅ Contador de caracteres
- ✅ Integração com API via React Query

## Uso

```typescript
import { LandingPageForm } from '@/features/landing-page/components';

function MyPage() {
  return (
    <LandingPageForm
      barbershopId="barbershop-uuid"
      onSaveSuccess={() => console.log('Saved!')}
      onCancel={() => console.log('Cancelled')}
    />
  );
}
```

## Props

| Prop | Tipo | Obrigatório | Descrição |
|------|------|-------------|-----------|
| `barbershopId` | `string` | Sim | ID da barbearia para carregar configuração |
| `onSaveSuccess` | `() => void` | Não | Callback executado após salvamento bem-sucedido |
| `onCancel` | `() => void` | Não | Callback executado ao cancelar edição |
| `readonly` | `boolean` | Não | Se o formulário está em modo visualização apenas (padrão: `false`) |

## Campos do Formulário

### Logo da Barbearia
- Componente: `LogoUploader`
- Upload de imagem (JPG, PNG, SVG)
- Tamanho máximo: 2MB
- Preview em tempo real

### Sobre a Barbearia
- Tipo: `textarea`
- Máximo: 1000 caracteres
- Contador de caracteres em tempo real
- Opcional

### Horário de Funcionamento
- Tipo: `textarea`
- Máximo: 500 caracteres
- Contador de caracteres em tempo real
- Opcional
- Suporte a múltiplas linhas

### WhatsApp
- Tipo: `tel`
- Formato obrigatório: `+55XXXXXXXXXXX`
- Validação de formato brasileiro
- Obrigatório

### Instagram
- Tipo: `url`
- Validação de URL
- Opcional

### Facebook
- Tipo: `url`
- Validação de URL
- Opcional

### Serviços
- Componente: `ServiceManager`
- Seleção de serviços visíveis
- Reordenação drag & drop
- Configuração de ordem de exibição

## Validação

O formulário utiliza `react-hook-form` + `zod` para validação:

```typescript
const formSchema = z.object({
  aboutText: z.string().max(1000).optional().or(z.literal('')),
  openingHours: z.string().max(500).optional().or(z.literal('')),
  instagramUrl: z.string().url().optional().or(z.literal('')),
  facebookUrl: z.string().url().optional().or(z.literal('')),
  whatsappNumber: z.string().regex(/^\+55\d{11}$/).min(1),
});
```

### Mensagens de Erro

- **aboutText**: "O texto sobre a barbearia deve ter no máximo 1000 caracteres"
- **openingHours**: "O horário de funcionamento deve ter no máximo 500 caracteres"
- **instagramUrl**: "URL do Instagram inválida"
- **facebookUrl**: "URL do Facebook inválida"
- **whatsappNumber**: "Formato inválido. Use +55 seguido de DDD e número"

## Comportamento

### Carregamento Inicial
1. Busca configuração existente via hook `useLandingPage`
2. Preenche formulário com dados salvos
3. Inicializa estado dos serviços

### Submissão
1. Valida todos os campos
2. Combina dados do formulário + estado dos serviços
3. Envia payload para API
4. Exibe feedback de sucesso/erro via toast
5. Chama callback `onSaveSuccess` se fornecido

### Cancelamento
1. Reseta formulário para valores originais
2. Restaura estado dos serviços
3. Chama callback `onCancel` se fornecido

## Estados

### Loading States
- `isLoading`: Carregando configuração inicial
- `isUpdating`: Salvando alterações
- Todos os campos ficam desabilitados durante salvamento

### Empty State
Se não houver configuração disponível, exibe mensagem de erro.

## Integração com Hooks

```typescript
const { config, updateConfig, isUpdating, isLoading } = useLandingPage(barbershopId);
```

- **config**: Configuração atual da landing page
- **updateConfig**: Função para atualizar configuração
- **isUpdating**: Se está salvando
- **isLoading**: Se está carregando

## Componentes Integrados

### LogoUploader
```typescript
<LogoUploader
  barbershopId={barbershopId}
  currentLogoUrl={config.logoUrl}
  disabled={readonly || isUpdating}
/>
```

### ServiceManager
```typescript
<ServiceManager
  services={services}
  onChange={setServices}
  disabled={readonly || isUpdating}
/>
```

## Acessibilidade

- Labels descritivos para todos os campos
- Mensagens de erro acessíveis (`aria-describedby`)
- Estados de foco visíveis
- Feedback de loading anunciado

## Testes

Cobertura completa de testes unitários em `__tests__/LandingPageForm.test.tsx`:

- ✅ Renderização de todos os elementos
- ✅ População de formulário com dados existentes
- ✅ Validação de campos
- ✅ Submissão de formulário
- ✅ Cancelamento
- ✅ Estados de loading
- ✅ Modo readonly
- ✅ Integração com componentes filhos

## Exemplo Completo

```typescript
import { LandingPageForm } from '@/features/landing-page/components';

export function LandingPageEditor() {
  const handleSaveSuccess = () => {
    toast.success('Landing page atualizada com sucesso!');
    navigate('/admin/landing-page/preview');
  };

  const handleCancel = () => {
    navigate('/admin/dashboard');
  };

  return (
    <div className="container mx-auto py-8">
      <h1 className="text-3xl font-bold mb-6">
        Personalizar Landing Page
      </h1>
      
      <LandingPageForm
        barbershopId="my-barbershop-id"
        onSaveSuccess={handleSaveSuccess}
        onCancel={handleCancel}
      />
    </div>
  );
}
```

## Dependências

- `react-hook-form`: Gerenciamento de formulário
- `zod`: Validação de schema
- `@hookform/resolvers`: Integração zod + react-hook-form
- `@tanstack/react-query`: Gerenciamento de estado assíncrono
- Componentes UI do `shadcn/ui`:
  - `Form`, `FormField`, `FormItem`, `FormLabel`, `FormControl`, `FormMessage`, `FormDescription`
  - `Input`, `Textarea`, `Button`
- `lucide-react`: Ícones

## Notas Técnicas

### Gerenciamento de Estado dos Serviços

O estado dos serviços é gerenciado separadamente do formulário principal porque é controlado pelo componente filho `ServiceManager`:

```typescript
const [services, setServices] = useState<LandingPageService[]>([]);
```

Na submissão, os serviços são combinados com os dados do formulário:

```typescript
const payload = {
  ...data,
  services: services.map((service) => ({
    serviceId: service.serviceId,
    displayOrder: service.displayOrder,
    isVisible: service.isVisible,
  })),
};
```

### Callback de Sucesso

O callback `onSaveSuccess` é chamado quando a mutation de update é concluída com sucesso. Isso é rastreado através de um estado local:

```typescript
const [wasUpdating, setWasUpdating] = useState(false);

useEffect(() => {
  if (wasUpdating && !isUpdating && onSaveSuccess) {
    onSaveSuccess();
    setWasUpdating(false);
  }
}, [wasUpdating, isUpdating, onSaveSuccess]);
```

## Referências

- **PRD**: `tasks/prd-landing-page-barbearia/prd.md` - Seção 3
- **Tech Spec**: `tasks/prd-landing-page-barbearia/techspec-frontend.md` - Seção 1.4
- **Task**: `tasks/prd-landing-page-barbearia/17_task.md`
