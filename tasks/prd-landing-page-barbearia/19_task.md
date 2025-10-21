---
status: pending
parallelizable: false
blocked_by: ["18.0"]
---

# Tarefa 19.0: Integração com Rotas e Navegação

## Visão Geral
Integrar a nova seção de gerenciamento da Landing Page ao sistema de rotas principal do painel de administração (`barbapp-admin`). Isso envolve adicionar um novo item de menu e configurar a rota para que a página `LandingPageEditor` seja acessível.

## Requisitos Funcionais
- **Acesso via Menu**: Deve haver um novo item no menu de navegação principal do painel admin, chamado "Landing Page", com um ícone apropriado (ex: 🎨).
- **Rota Dedicada**: Acessar a URL `/admin/landing-page` (ou uma rota similar, dependendo do padrão do projeto) deve renderizar a página `LandingPageEditor`.
- **Autenticação**: A rota deve ser protegida e acessível apenas por usuários autenticados com o perfil de "Admin da Barbearia".

## Detalhes de Implementação
- **Biblioteca de Roteamento**: A implementação dependerá da biblioteca de roteamento usada no projeto `barbapp-admin` (provavelmente `react-router-dom`).
- **Arquivo de Rotas**: Modificar o arquivo principal de configuração de rotas (geralmente `App.tsx` ou um arquivo dedicado como `routes.tsx`) para adicionar a nova rota.
- **Layout do Admin**: A nova rota deve ser renderizada dentro do layout principal do painel de administração, que inclui o menu lateral, header, etc.
- **Componente de Navegação**: Adicionar o link (`<Link>` ou `<NavLink>`) no componente que renderiza o menu de navegação, associando-o à nova rota.

## Estrutura da Implementação

**Exemplo de configuração de rota (em `App.tsx` ou similar):**
```typescript
<Routes>
  {/* Outras rotas do admin */}
  <Route 
    path="/admin/landing-page" 
    element={
      <ProtectedRoute requiredRole="ADMIN">
        <AdminLayout>
          <LandingPageEditor />
        </AdminLayout>
      </ProtectedRoute>
    }
  />
  {/* Outras rotas */}
</Routes>
```

**Exemplo de item de menu (em `Sidebar.tsx` ou similar):**
```typescript
<nav>
  {/* Outros itens de menu */}
  <NavLink to="/admin/landing-page" className={...}>
    <Palette size={20} />
    <span>Landing Page</span>
  </NavLink>
</nav>
```

## Critérios de Aceitação
- [ ] Um novo item de menu "Landing Page" está visível no painel de administração.
- [ ] Clicar no item de menu navega para a URL `/admin/landing-page`.
- [ ] Acessar a URL `/admin/landing-page` diretamente renderiza a página `LandingPageEditor`.
- [ ] A rota é protegida e redireciona para o login se o usuário não estiver autenticado.
- [ ] O item de menu "Landing Page" fica com o estado "ativo" quando o usuário está na página de edição.