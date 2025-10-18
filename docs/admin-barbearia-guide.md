# Guia do Usuário - Admin Barbearia

Guia completo para utilização da interface de administração de barbearias no sistema BarbApp.

## Índice

1. [Visão Geral](#visão-geral)
2. [Acesso e Autenticação](#acesso-e-autenticação)
3. [Dashboard](#dashboard)
4. [Gestão de Barbeiros](#gestão-de-barbeiros)
5. [Gestão de Serviços](#gestão-de-serviços)
6. [Visualização de Agenda](#visualização-de-agenda)
7. [Dúvidas Frequentes](#dúvidas-frequentes)

---

## Visão Geral

O **Admin Barbearia** é a interface específica para que administradores de cada barbearia possam gerenciar seus barbeiros, serviços e visualizar a agenda de agendamentos.

**Principais funcionalidades:**
- ✅ Gestão completa de barbeiros (criar, editar, desativar)
- ✅ Gestão completa de serviços (preços, duração, descrições)
- ✅ Visualização de agenda com filtros avançados
- ✅ Dashboard com métricas da barbearia
- ✅ Sistema multi-tenant (cada barbearia tem seus dados isolados)

---

## Acesso e Autenticação

### Passo 1: Obter Código da Barbearia

Antes de acessar o sistema, você precisa do **código único** da sua barbearia. Este código é fornecido pelo administrador central durante o onboarding.

**Exemplo de código:** `ABC123`, `BARBER2024`, `TEST1234`

### Passo 2: Acessar a URL

A URL de acesso segue o padrão:

```
https://barbapp.com/{codigo-da-barbearia}
```

**Exemplos:**
- `https://barbapp.com/ABC123`
- `https://barbapp.com/BARBER2024`
- `http://localhost:3001/TEST1234` (desenvolvimento)

### Passo 3: Login

1. Acesse a URL com o código da sua barbearia
2. O sistema validará o código e exibirá o nome da barbearia
3. Preencha seu **email** e **senha** de administrador
4. Clique em **"Entrar"**

**Credenciais de teste (ambiente de desenvolvimento):**
```
Código: TEST1234
Email: admin@test.com
Senha: Test@123
```

### Segurança

- 🔒 Senha deve ter no mínimo 8 caracteres
- 🔒 Tokens JWT com expiração de 24 horas
- 🔒 Isolamento multi-tenant (cada barbearia acessa apenas seus dados)
- 🔒 Proteção de rotas (redirecionamento automático para login se não autenticado)

---

## Dashboard

Após o login, você é direcionado para o **Dashboard**, que apresenta:

### Métricas Principais

- 📊 **Total de Barbeiros**: Quantidade de barbeiros ativos
- 📊 **Total de Serviços**: Quantidade de serviços cadastrados
- 📊 **Agendamentos Hoje**: Agendamentos programados para o dia atual
- 📊 **Próximos Agendamentos**: Lista dos próximos agendamentos

### Navegação

O menu lateral/superior contém os links principais:
- **Dashboard**: Visão geral e métricas
- **Barbeiros**: Gestão de barbeiros
- **Serviços**: Gestão de serviços
- **Agenda**: Visualização de agendamentos

---

## Gestão de Barbeiros

Gerencie todos os barbeiros da sua barbearia nesta seção.

### Listar Barbeiros

1. Acesse **"Barbeiros"** no menu
2. Visualize a lista de todos os barbeiros cadastrados
3. Use o campo de busca para filtrar por nome
4. Cada linha exibe: Nome, Email, Telefone, Especialidade, Status

### Criar Novo Barbeiro

1. Clique no botão **"Novo Barbeiro"**
2. Preencha o formulário:
   - **Nome*** (obrigatório)
   - **Email*** (obrigatório, deve ser único)
   - **Telefone*** (obrigatório, formato: (11) 99999-9999)
   - **Especialidade** (opcional, ex: "Cortes modernos", "Barbas")
3. Clique em **"Salvar"**
4. Uma notificação de sucesso será exibida

**Validações:**
- Email deve ser válido e único no sistema
- Telefone deve seguir formato brasileiro
- Nome é obrigatório

### Editar Barbeiro

1. Na lista de barbeiros, clique no ícone de **"Editar"** (✏️) na linha do barbeiro
2. Modifique os campos desejados
3. Clique em **"Salvar"**
4. As alterações serão aplicadas imediatamente

### Desativar Barbeiro

1. Na lista de barbeiros, clique no ícone de **"Desativar"** (🗑️)
2. Confirme a ação no modal de confirmação
3. O barbeiro ficará com status **"Inativo"**

**Importante:**
- Barbeiros inativos não aparecem em novos agendamentos
- Agendamentos já marcados com esse barbeiro não são afetados
- É possível reativar um barbeiro posteriormente

### Filtros e Busca

- **Buscar por nome**: Digite no campo de busca para filtrar instantaneamente
- **Filtrar por status**: Selecione "Ativo" ou "Inativo" no dropdown

---

## Gestão de Serviços

Cadastre e gerencie todos os serviços oferecidos pela barbearia.

### Listar Serviços

1. Acesse **"Serviços"** no menu
2. Visualize a lista de todos os serviços cadastrados
3. Cada linha exibe: Nome, Descrição, Preço, Duração, Status

### Criar Novo Serviço

1. Clique no botão **"Novo Serviço"**
2. Preencha o formulário:
   - **Nome*** (obrigatório, ex: "Corte Masculino")
   - **Descrição** (opcional, ex: "Corte tradicional com máquina e tesoura")
   - **Preço*** (obrigatório, em reais, ex: 50.00)
   - **Duração*** (obrigatória, em minutos, ex: 60)
3. Clique em **"Salvar"**

**Validações:**
- Preço deve ser maior que zero
- Duração deve ser maior que zero
- Nome é obrigatório

### Editar Serviço

1. Na lista de serviços, clique no ícone de **"Editar"** (✏️)
2. Modifique os campos desejados
3. Clique em **"Salvar"**

**Dica:** Ajuste preços e durações conforme necessário sem afetar agendamentos já confirmados.

### Desativar Serviço

1. Na lista de serviços, clique no ícone de **"Desativar"** (🗑️)
2. Confirme a ação
3. O serviço ficará com status **"Inativo"**

**Importante:**
- Serviços inativos não aparecem em novos agendamentos
- Agendamentos já marcados com esse serviço não são afetados

### Formatação Automática

O sistema formata automaticamente:
- **Preço**: R$ 50,00 (moeda brasileira)
- **Duração**: 60 min

---

## Visualização de Agenda

Visualize todos os agendamentos da barbearia com filtros avançados.

### Acessar Agenda

1. Acesse **"Agenda"** no menu
2. A tabela de agendamentos será carregada automaticamente
3. Por padrão, exibe todos os agendamentos

### Colunas da Tabela

| Coluna | Descrição |
|--------|-----------|
| **Data/Hora** | Data e hora do agendamento (formato: dd/MM/yyyy, HH:mm) |
| **Cliente** | Nome do cliente |
| **Barbeiro** | Nome do barbeiro responsável |
| **Serviço** | Serviço a ser realizado |
| **Status** | Status atual do agendamento (badge colorido) |
| **Ações** | Botão "Detalhes" para ver informações completas |

### Filtros Disponíveis

#### Filtro por Barbeiro
1. Clique no dropdown **"Barbeiro"**
2. Selecione o barbeiro desejado
3. A tabela filtrará apenas agendamentos desse barbeiro
4. Selecione "Todos" para limpar o filtro

#### Filtro por Data
1. **Data Início**: Preencha para ver agendamentos a partir desta data
2. **Data Fim**: Preencha para ver agendamentos até esta data
3. Use ambos para definir um período específico

**Exemplo:**
- Data Início: 01/01/2024
- Data Fim: 31/01/2024
- Resultado: Todos os agendamentos de janeiro de 2024

#### Filtro por Status
1. Clique no dropdown **"Status"**
2. Selecione o status desejado:
   - 🟡 **Agendado**: Agendamento criado, aguardando confirmação
   - 🔵 **Confirmado**: Cliente confirmou presença
   - 🟠 **Em Andamento**: Atendimento em progresso
   - 🟢 **Concluído**: Atendimento finalizado
   - 🔴 **Cancelado**: Agendamento cancelado

#### Limpar Filtros
Clique no botão **"Limpar"** para remover todos os filtros aplicados.

### Visualizar Detalhes

1. Na linha de um agendamento, clique no botão **"Detalhes"**
2. Um modal será aberto com informações completas:
   - Data e hora formatada
   - Nome do cliente
   - Telefone do cliente
   - Nome do barbeiro
   - Serviço contratado
   - Preço do serviço
   - Duração estimada
   - Status atual com badge colorido
3. Clique em **"Fechar"** para retornar à lista

### Status e Cores

Cada status tem uma cor específica para fácil identificação visual:

| Status | Cor | Descrição |
|--------|-----|-----------|
| Agendado | 🟡 Amarelo | Novo agendamento |
| Confirmado | 🔵 Azul | Cliente confirmou |
| Em Andamento | 🟠 Laranja | Atendimento iniciado |
| Concluído | 🟢 Verde | Atendimento finalizado |
| Cancelado | 🔴 Vermelho | Cancelado por cliente/barbeiro |

---

## Dúvidas Frequentes

### 1. Esqueci minha senha, como recuperar?

Entre em contato com o administrador central da plataforma para redefinição de senha.

### 2. Posso ter mais de um administrador por barbearia?

Sim, o administrador central pode criar múltiplas contas de administrador para a mesma barbearia.

### 3. O que acontece se eu desativar um barbeiro que tem agendamentos futuros?

Os agendamentos já confirmados permanecerão ativos. O barbeiro apenas não aparecerá em novos agendamentos.

### 4. Posso alterar o preço de um serviço?

Sim, você pode alterar a qualquer momento. Agendamentos já confirmados manterão o preço original.

### 5. Como funciona o sistema multi-tenant?

Cada barbearia tem um código único. Todos os dados (barbeiros, serviços, agendamentos) são isolados por barbearia. Você só acessa os dados da sua barbearia.

### 6. Posso acessar de dispositivos móveis?

Sim, a interface é responsiva e funciona em smartphones e tablets.

### 7. Meu código de barbearia parou de funcionar, o que fazer?

Verifique se o código está correto. Se o problema persistir, entre em contato com o administrador central - a barbearia pode ter sido desativada.

### 8. Como faço logout?

Clique no botão **"Sair"** no menu superior. Você será redirecionado para a tela de login.

### 9. Por quanto tempo minha sessão fica ativa?

Tokens de autenticação expiram após 24 horas de inatividade. Após esse período, você precisará fazer login novamente.

### 10. Posso criar agendamentos pela interface Admin Barbearia?

A versão atual permite apenas **visualização** de agendamentos. A criação de novos agendamentos é feita por outras interfaces (app do cliente, sistema do barbeiro).

---

## Suporte

Para suporte técnico ou dúvidas adicionais, entre em contato:

- 📧 **Email**: suporte@barbapp.com
- 📱 **WhatsApp**: (11) 99999-9999
- 📚 **Documentação Técnica**: [docs/README.md](../README.md)

---

**Última atualização:** Janeiro 2025  
**Versão:** 1.0.0
