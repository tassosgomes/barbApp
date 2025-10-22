# PreviewPanel Component

## Visão Geral

O componente `PreviewPanel` exibe uma pré-visualização em tempo real da landing page pública dentro do painel de administração. Ele permite que administradores visualizem como suas alterações aparecerão para os clientes antes de publicá-las.

## Funcionalidades

- ✅ Renderização em tempo real do template selecionado
- ✅ Alternância entre visualizações Mobile e Desktop
- ✅ Suporte para todos os 5 templates
- ✅ Modo fullScreen para visualização ampliada
- ✅ Preview não-interativo (pointer-events-none)
- ✅ Botão para abrir landing page em nova aba
- ✅ Informações do dispositivo e template
- ✅ Acessibilidade completa

## Uso

### Básico

```tsx
import { PreviewPanel } from '@/features/landing-page';

function MyComponent() {
  const config = useLandingPage('barbershop-id');
  
  return <PreviewPanel config={config} />;
}
```

### Com controles customizados

```tsx
import { PreviewPanel } from '@/features/landing-page';
import { useState } from 'react';

function MyComponent() {
  const [device, setDevice] = useState<'mobile' | 'desktop'>('desktop');
  const config = useLandingPage('barbershop-id');
  
  return (
    <PreviewPanel
      config={config}
      device={device}
      onDeviceChange={setDevice}
    />
  );
}
```

### Modo fullScreen

```tsx
import { PreviewPanel } from '@/features/landing-page';

function PreviewPage() {
  const config = useLandingPage('barbershop-id');
  
  return <PreviewPanel config={config} fullScreen />;
}
```

## Props

| Prop | Tipo | Padrão | Descrição |
|------|------|--------|-----------|
| `config` | `LandingPageConfig` | `undefined` | Configuração da landing page para renderizar |
| `fullScreen` | `boolean` | `false` | Se deve renderizar em modo tela cheia |
| `device` | `PreviewDevice` | `'desktop'` | Dispositivo inicial para preview |
| `onDeviceChange` | `(device: PreviewDevice) => void` | `undefined` | Callback quando dispositivo muda |

### LandingPageConfig

```typescript
interface LandingPageConfig {
  id: string;
  barbershopId: string;
  templateId: number; // 1-5
  logoUrl?: string;
  aboutText?: string;
  openingHours?: string;
  instagramUrl?: string;
  facebookUrl?: string;
  whatsappNumber: string;
  isPublished: boolean;
  services: LandingPageService[];
  updatedAt: string;
  createdAt: string;
}
```

## Estrutura Visual

```
┌─────────────────────────────────────────────────┐
│ Preview da Landing Page             [Mobile][Desktop] [Abrir] │
│ Mobile (375px × 667px) • Template: 1                          │
├─────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────────┐                                         │
│  │                 │                                         │
│  │   Template      │ ← Preview não-interativo               │
│  │   Renderizado   │                                         │
│  │                 │                                         │
│  └─────────────────┘                                         │
│                                                               │
└─────────────────────────────────────────────────┘
```

## Dispositivos Suportados

### Mobile
- Largura: 375px
- Altura mínima: 667px
- Representação: iPhone X/11/12

### Desktop
- Largura: 100%
- Altura mínima: 600px
- Responsivo ao container

## Templates

O componente renderiza dinamicamente um dos 5 templates disponíveis:

1. **Template 1 - Clássico**: Elegante e tradicional (Preto/Dourado)
2. **Template 2 - Moderno**: Limpo e minimalista (Azul/Branco)
3. **Template 3 - Vintage**: Retrô anos 50/60 (Marrom/Vermelho)
4. **Template 4 - Urbano**: Street/Hip-hop (Preto/Vermelho)
5. **Template 5 - Premium**: Luxuoso e sofisticado (Preto/Dourado metálico)

## Características Técnicas

### Performance

- **Memoização**: Template component e container classes são memoizados com `useMemo`
- **Renderização eficiente**: Apenas re-renderiza quando `config` ou `device` mudam
- **Isolamento**: Preview usa `pointer-events: none` para evitar interações

### Acessibilidade

- ARIA labels apropriados
- Títulos descritivos nos botões
- Role `presentation` no container do preview
- Suporte a navegação por teclado

### Responsividade

- Adapta-se ao container em modo normal
- Largura fixa em modo fullScreen
- Breakpoints: mobile (375px), desktop (100%)

## Casos de Uso

### 1. Edição de Informações

```tsx
<div className="grid grid-cols-2 gap-6">
  <div>
    <LandingPageForm config={config} />
  </div>
  <div>
    <PreviewPanel config={config} />
  </div>
</div>
```

### 2. Galeria de Templates

```tsx
function TemplateModal({ template }) {
  return (
    <Dialog>
      <DialogContent className="max-w-6xl">
        <PreviewPanel 
          config={previewConfig} 
          fullScreen 
        />
      </DialogContent>
    </Dialog>
  );
}
```

### 3. Aba de Preview

```tsx
<Tabs>
  <TabsList>
    <TabsTrigger value="edit">Editar</TabsTrigger>
    <TabsTrigger value="preview">Preview</TabsTrigger>
  </TabsList>
  <TabsContent value="preview">
    <PreviewPanel config={config} fullScreen />
  </TabsContent>
</Tabs>
```

## Limitações Atuais

- Templates são placeholders (serão substituídos por implementações completas)
- Preview em tempo real requer debounce manual no componente pai
- Não suporta dispositivo "tablet" (previsto para futuro)
- Links e botões não são clicáveis (by design)

## Testes

O componente possui 32 testes automatizados cobrindo:

- ✅ Renderização básica
- ✅ Controles de dispositivo
- ✅ Renderização de templates
- ✅ Estilos do container
- ✅ Botão de abrir em nova aba
- ✅ Acessibilidade
- ✅ Casos extremos (edge cases)
- ✅ Performance
- ✅ Integração

```bash
npm test -- PreviewPanel
```

## Roadmap

### v1.1 (Futuro)
- [ ] Suporte para dispositivo "tablet"
- [ ] Zoom in/out
- [ ] Captura de screenshot
- [ ] Comparação lado-a-lado de templates
- [ ] Histórico de versões do preview

### v1.2 (Futuro)
- [ ] Preview com debounce automático
- [ ] Modo dark para o painel
- [ ] Anotações no preview
- [ ] Compartilhamento de preview

## Contribuindo

Ao contribuir para este componente, siga:

1. **Padrões de código**: Ver `rules/code-standard.md` e `rules/react.md`
2. **Testes**: Adicione testes para novas funcionalidades
3. **Documentação**: Atualize este README
4. **Types**: Mantenha types em `landing-page.types.ts`

## Links Relacionados

- [PRD - Landing Page](../../tasks/prd-landing-page-barbearia/prd.md)
- [Tech Spec Frontend](../../tasks/prd-landing-page-barbearia/techspec-frontend.md)
- [Task 16.0](../../tasks/prd-landing-page-barbearia/16_task.md)
- [Tipos TypeScript](../types/landing-page.types.ts)
- [Templates](./templates/)

---

**Versão**: 1.0  
**Data**: 2025-10-21  
**Autor**: BarbApp Team  
**Status**: ✅ Completo
