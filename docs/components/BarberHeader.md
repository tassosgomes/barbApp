# BarberHeader Component

## 📝 Descrição

Componente de header para o painel do barbeiro que exibe informações do usuário autenticado e fornece acesso rápido ao logout.

## ✨ Funcionalidades

- **Exibição de Informações**: Mostra o nome do barbeiro e o nome da barbearia
- **Botão de Logout**: Permite ao barbeiro sair do sistema de forma rápida
- **Design Responsivo**: Adapta-se a diferentes tamanhos de tela
- **Sticky Header**: Permanece fixo no topo durante a rolagem
- **Integração com AuthContext**: Usa o contexto de autenticação para obter dados do usuário

## 🎨 Visual

```
┌─────────────────────────────────────────────────┐
│ João Silva                          [🚪 Sair]   │
│ Barbearia Top                                   │
└─────────────────────────────────────────────────┘
```

## 📦 Uso

```tsx
import { BarberHeader } from '@/components/barber';

function BarberSchedulePage() {
  return (
    <>
      <BarberHeader />
      <div className="container">
        {/* Conteúdo da página */}
      </div>
    </>
  );
}
```

## 🔧 Props

Nenhuma prop é necessária. O componente usa o `useAuth()` hook para obter os dados do usuário.

## 🎯 Comportamento

### Quando há usuário autenticado
- Exibe o nome do barbeiro em negrito (h1)
- Exibe o nome da barbearia em texto menor
- Mostra o botão "Sair" com ícone

### Quando não há usuário autenticado
- Retorna `null` (não renderiza nada)

## 📱 Responsividade

### Mobile (< 640px)
- Mostra apenas o ícone do botão de logout
- Layout compacto

### Desktop (≥ 640px)
- Mostra o ícone e o texto "Sair"
- Layout mais espaçado

## 🎨 Estilos

- **Header**: Fundo branco, borda inferior, sombra sutil
- **Posicionamento**: Sticky, fixo no topo (z-index: 10)
- **Espaçamento**: Container com padding responsivo
- **Botão**: Estilo ghost com hover suave

## ♿ Acessibilidade

- ✅ Usa elementos semânticos (`<header>`, `<h1>`)
- ✅ Landmark `banner` para navegação por leitores de tela
- ✅ Hierarquia de headings correta
- ✅ Botão com texto descritivo
- ✅ Ícone decorativo (não interfere na navegação)

## 🧪 Testes

### Cobertura: 100%

**17 testes passando:**
- ✅ Renderização (5 testes)
  - Nome do barbeiro
  - Nome da barbearia
  - Botão de logout
  - Ícone do botão
  - Não renderizar quando user é null
  
- ✅ Interações (1 teste)
  - Chamada do logout ao clicar

- ✅ Responsividade (1 teste)
  - Texto oculto em telas pequenas

- ✅ Estilos (3 testes)
  - Header sticky
  - Borda e sombra
  - Fundo branco

- ✅ Layout (3 testes)
  - Fonte do nome
  - Fonte da barbearia
  - Layout flex

- ✅ Acessibilidade (2 testes)
  - Hierarquia de headings
  - Header landmark

- ✅ Diferentes Usuários (2 testes)
  - Nomes diferentes
  - Nomes longos

## 🔄 Integração com AuthContext

O componente usa o hook `useAuth()` que fornece:

```typescript
interface AuthContextType {
  user: User | null;
  logout: () => void;
}

interface User {
  id: string;
  name: string;
  email: string;
  role: 'Barbeiro';
  barbeariaId: string;
  nomeBarbearia: string;
}
```

## 📂 Estrutura de Arquivos

```
src/components/barber/
├── BarberHeader.tsx          # Componente principal
├── __tests__/
│   └── BarberHeader.test.tsx # Testes unitários
└── index.ts                  # Exportações
```

## 🚀 Páginas que usam este componente

- `/barber/schedule` - Página de agenda do barbeiro

## 💡 Melhorias Futuras (Opcional)

1. **Avatar do Barbeiro**: Adicionar foto de perfil
2. **Menu Dropdown**: Expandir para incluir mais opções (perfil, configurações)
3. **Notificações**: Badge com contador de notificações
4. **Temas**: Suporte a tema escuro
5. **Breadcrumbs**: Navegação contextual
6. **Status Online**: Indicador de disponibilidade

## 🐛 Resolução de Problemas

### Header não aparece
**Causa**: Usuário não autenticado  
**Solução**: Verificar se o AuthProvider está configurado corretamente

### Logout não funciona
**Causa**: Função logout não implementada no AuthContext  
**Solução**: Verificar implementação do AuthContext

### Estilos não aplicados
**Causa**: Tailwind CSS não configurado  
**Solução**: Verificar configuração do Tailwind

## 📄 Licença

Este componente faz parte do sistema BarbApp.

---

**Desenvolvido em:** 20 de Outubro de 2025  
**Versão:** 1.0.0  
**Status:** ✅ Pronto para produção
