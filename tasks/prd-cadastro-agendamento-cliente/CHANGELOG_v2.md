# Changelog - PRD Cadastro e Agendamento Cliente

## Versão 2.0 (19/10/2025)

### 🎯 Mudanças Principais

#### 1. **Cadastro Automático (Sem Cadastro Prévio)**
**Antes**: Cliente precisava se cadastrar antes de agendar  
**Agora**: Cliente agenda primeiro, cadastro acontece automaticamente na finalização

**Impacto**: Reduz fricção drasticamente, aumenta conversão

#### 2. **Novo Fluxo de Agendamento**
**Antes**: Barbeiro → Serviço → Data/Hora → Confirmar  
**Agora**: **Serviços → Barbeiro → Data/Hora → Nome+Telefone → Confirmar**

**Justificativa**: Cliente decide primeiro O QUE quer fazer, depois COM QUEM

#### 3. **Login Simplificado**
**Antes**: Telefone + Nome para login  
**Agora**: **Apenas Telefone**

**Nota**: Validação por SMS vem em fase futura

#### 4. **Opção "Qualquer Barbeiro"**
**Nova Funcionalidade**: Cliente pode escolher barbeiro específico OU deixar sistema escolher aleatoriamente

**Benefício**: Agiliza agendamento para clientes sem preferência

#### 5. **Múltiplos Serviços**
**Nova Funcionalidade**: Cliente pode agendar múltiplos serviços de uma vez (ex: Corte + Barba)

**Impacto**: Aumenta ticket médio, melhora experiência do cliente

#### 6. **Dashboard do Cliente**
**Nova Funcionalidade Completa**: 
- Visualizar agendamentos futuros
- Cancelar agendamentos
- **Editar agendamentos** (novo!)
- Visualizar histórico
- Criar novo agendamento

**Antes**: PRD focava apenas em criar agendamento  
**Agora**: Gestão completa de agendamentos pelo cliente

---

## 📋 Alterações Detalhadas

### Funcionalidades Atualizadas

| # | Funcionalidade | Mudança | Motivo |
|---|---------------|---------|---------|
| 1 | Acesso à Barbearia | Apenas URL (código transparente) | Cliente não precisa "conhecer" código |
| 2 | Visualização de Serviços | **NOVA** - Primeira etapa do fluxo | Definir O QUE antes de COM QUEM |
| 3 | Escolha de Barbeiro | Adicionado "Qualquer um" | Flexibilidade e agilidade |
| 4 | Seleção de Data/Horário | Sugerir dia atual | Facilitar escolha |
| 5 | Cadastro | Automático na finalização | Zero fricção |
| 6 | Login | Apenas telefone | Simplificação máxima |
| 7 | Dashboard | **NOVA** - Gestão completa | Cliente gerencia próprios agendamentos |
| 8 | Cancelamento | Via dashboard | Autonomia do cliente |
| 9 | **Edição** | **NOVA** - Editar agendamento | Evitar cancelar + criar novo |
| 10 | Histórico | Integrado ao dashboard | Visualização unificada |

### Fluxos Completamente Reescritos

✅ **Fluxo 1**: Primeiro Agendamento (Novo Cliente)
- Serviços → Barbeiro/Qualquer → Data/Hora → Nome+Telefone → Confirmação
- Cadastro automático transparente

✅ **Fluxo 2**: Login e Dashboard
- Apenas telefone → Acesso ao dashboard

✅ **Fluxo 3**: Novo Agendamento (Cliente Cadastrado)
- Fluxo igual ao Fluxo 1, mas sem pedir nome/telefone

✅ **Fluxo 4**: Cancelar Agendamento
- Dashboard → Cancelar → Confirmação

✅ **Fluxo 5**: Editar Agendamento (NOVO!)
- Dashboard → Editar → Modificar dados → Confirmar

✅ **Fluxo 6**: Múltiplas Barbearias
- URLs independentes, dados isolados

---

## 🔄 Requisitos Removidos

| Requisito Removido | Motivo |
|-------------------|--------|
| Tela de cadastro separada | Cadastro agora é automático |
| Login com telefone + nome | Simplificado para apenas telefone |
| Cliente escolher barbeiro primeiro | Agora escolhe serviços primeiro |
| Validação de nome completo | Nome livre, sem validação |

---

## ➕ Novos Requisitos Adicionados

| Novo Requisito | Descrição | Prioridade |
|---------------|-----------|-----------|
| Múltiplos serviços | Cliente pode selecionar vários serviços | Alta |
| Opção "Qualquer barbeiro" | Sistema sorteia aleatoriamente | Alta |
| Editar agendamento | Modificar serviços, barbeiro, data/hora | Alta |
| Dashboard completo | Gestão centralizada de agendamentos | Alta |
| Cadastro automático | Criar cliente na finalização | Crítica |
| Login apenas telefone | Simplificação de acesso | Alta |
| Sugestão de data (hoje) | UX melhorada | Média |
| Confirmação visual | Feedback após agendamento | Alta |

---

## ⚠️ Simplificações do MVP

| Aspecto | MVP Simplificado | Implementação Futura |
|---------|-----------------|---------------------|
| Disponibilidade | Todos horários 08:00-20:00 disponíveis | Validação real de agenda de barbeiros |
| Validação de Telefone | Apenas formato | Código SMS |
| Login | Apenas telefone | Telefone + SMS |
| Seleção de Barbeiro Aleatório | Sorteio simples entre ativos | Considerar especialidades, carga de trabalho |
| Cancelamento | Sem limite de tempo | Política de cancelamento (ex: 2h antes) |
| No-show | Sem controle | Penalidades, bloqueios |
| Preços | Opcional (TBD) | Exibição e soma total obrigatórias |

---

## 📊 Comparação de Complexidade

### Antes (v1.0)
```
Telas: ~8-10
Fluxo Principal: 5 etapas
Cadastro Prévio: SIM
Tempo Estimado: ~5-7 min
```

### Agora (v2.0)
```
Telas: ~6-8
Fluxo Principal: 4 etapas + cadastro automático
Cadastro Prévio: NÃO
Tempo Estimado: ~3-4 min
```

**Redução de ~40% no tempo de primeiro agendamento!**

---

## 🎨 Mudanças de UX

### Antes
- Foco em cadastro
- Múltiplos passos de validação
- Cliente consciente do processo de cadastro

### Agora
- **Foco em agendamento**
- **Cadastro transparente** (cliente nem percebe)
- **Fluxo linear simples**
- **Zero barreiras**

### Novos Requisitos de UX
- Mobile-first obrigatório
- Máximo 6 telas do início ao fim
- Indicador de progresso visual
- CTAs descritivos (não genéricos)
- Cards para seleção (não dropdowns)
- Empty states amigáveis
- Feedback visual em todas as ações

---

## 🔐 Segurança

### MVP (Simplificado)
- Login apenas com telefone
- Sem validação SMS
- Sessão com token JWT
- Isolamento por barbearia

### Futuro (Fase 2+)
- Código de verificação SMS
- Two-factor authentication
- Biometria (app mobile)
- Histórico de acessos

---

## 📝 Decisões Pendentes Críticas

Antes de ir para Tech Spec, precisamos definir:

1. ✅ **Fluxo de Agendamento**: DEFINIDO (Serviços → Barbeiro → Data/Hora → Confirmar)
2. ✅ **Cadastro**: DEFINIDO (Automático na finalização)
3. ✅ **Login**: DEFINIDO (Apenas telefone no MVP)
4. ⚠️ **Status Inicial**: Agendamento começa como "Pendente" ou "Confirmado"?
5. ⚠️ **Limites de Agendamento**: Quantos agendamentos simultâneos permitir?
6. ⚠️ **Preços**: Exibir preços dos serviços? Soma total?
7. ⚠️ **Concorrência**: Estratégia para evitar double-booking
8. ⚠️ **Persistência de Sessão**: Quanto tempo token é válido?
9. ⚠️ **Múltiplos Serviços**: Como validar se horário + duração total cabe no dia?

---

## 🚀 Próximos Passos

1. **Revisar decisões pendentes** (acima)
2. **Aprovar mudanças do PRD v2.0**
3. **Criar Tech Spec** baseado neste PRD
4. **Definir tasks de implementação**
5. **Iniciar desenvolvimento**

---

**Revisado por**: Tassos Gomes  
**Data**: 19/10/2025  
**Aprovação**: Pendente
