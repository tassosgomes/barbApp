# Component Deep Analysis Report

## Resumo Executivo

O componente **api Service (Frontend)** é uma instância configurada do Axios que serve como camada centralizada de comunicação HTTP para o frontend administrativo da BarbApp. O componente tem papel arquitetural crítico como gateway de comunicação com o backend, implementando interceptors para autenticação automática, tratamento global de erros e logging. Com acoplamento aferente de 8 e eferente de 2, o componente é altamente reutilizado throughout da aplicação, sendo a principal porta de entrada para todas as operações de API.

**Principais achados:**
- Implementação robusta de autenticação JWT com gerenciamento automático de tokens
- Estratégia eficaz de tratamento de erros 401 com redirecionamento automático
- Logging abrangente para depuração e monitoramento
- Configuração de ambiente flexível com fallback values
- Dívida técnica identificada: ausência de implementação de token refresh

## Análise de Fluxo de Dados

1. **Requisição iniciada** via serviços (ex: barbershopService.getAll())
2. **Request interceptor** adiciona automaticamente JWT token ao header Authorization
3. **Logging da requisição** no console para debugging
4. **Comunicação HTTP** com backend via Axios
5. **Response interceptor** processa resposta ou erro
6. **Tratamento especial** para erro 401 (limpa token, seta flag de sessão expirada, redireciona)
7. **Logging da resposta** (sucesso ou erro)
8. **Retorno do dado** para o componente solicitante

```
Component/Service → api.get/post/put/delete() → Request Interceptor (token + log) → Backend → Response Interceptor (log + 401 handling) → Component/Service
```

## Regras de Negócio & Lógica

### Visão Geral das regras de negócio:

| Tipo de Regra | Descrição da Regra | Localização |
|---------------|--------------------|------------|
| Autenticação | JWT token obrigatório para requisições autenticadas | services/api.ts:20-23 |
| Sessão | Redirecionamento automático para login em 401 | services/api.ts:57-62 |
| Configuração | Timeout de 10 segundos para requisições | services/api.ts:8 |
| Logging | Todas as requisições e respostas são logadas | services/api.ts:26, 44, 49-55 |

### Detalhamento das regras de negócio:

### Regra de Negócio: Autenticação JWT Automática

**Visão Geral**:
Sistema de autenticação baseado em JSON Web Tokens que adiciona automaticamente o token de autenticação em todas as requisições HTTP para endpoints protegidos. A regra garante que usuários autenticados tenham acesso autorizado aos recursos da API sem a necessidade de gerenciamento manual de tokens em cada requisição.

**Descrição detalhada**:
O interceptor de requisição verifica automaticamente a presença de um token JWT no localStorage antes de cada requisição HTTP. Se um token estiver disponível, ele é adicionado ao header Authorization no formato "Bearer {token}". Esta abordagem centraliza a lógica de autenticação, eliminando a necessidade de tratamento repetitivo em cada chamada de API e garantindo consistência na implementação de autenticação across toda a aplicação. O sistema permite requisições não autenticadas (quando não há token) para endpoints públicos como login, enquanto protege automaticamente os endpoints que requerem autenticação.

**Fluxo da regra**:
1. Componente faz chamada para api.get/post/put/delete()
2. Request interceptor é executado automaticamente pelo Axios
3. Interceptor busca token em localStorage.getItem('auth_token')
4. Se token existe, adiciona header Authorization: `Bearer ${token}`
5. Requisição é enviada com token de autenticação
6. Backend valida token e autoriza ou nega acesso

---

### Regra de Negócio: Gerenciamento de Sessão Expirada

**Visão Geral**:
Estratégia de tratamento de sessão expirada que detecta automaticamente respostas 401 (Unauthorized) do backend, limpa o token inválido do armazenamento local, notifica o usuário sobre a expiração da sessão e redireciona para a página de login, garantindo uma experiência de usuário controlada e segura.

**Descrição detalhada**:
Quando o backend retorna um erro 401, indicando que o token JWT expirou ou é inválido, o interceptor de resposta implemente um fluxo de recuperação automática. Primeiro, o token inválido é removido do localStorage para evitar requisições subsequentes com credenciais expiradas. Em seguida, uma flag é armazenada no sessionStorage indicando que a sessão expirou, permitindo que o componente de Login exiba uma mensagem contextualizada ao usuário. Finalmente, o usuário é redirecionado automaticamente para a página de login via window.location.href, forçando uma nova autenticação. Esta abordagem previne acesso não autorizado e mantém a integridade do estado de autenticação da aplicação.

**Fluxo da regra**:
1. Backend retorna resposta com status 401
2. Response interceptor intercepta o erro
3. localStorage.removeItem('auth_token') limpa token inválido
4. sessionStorage.setItem('session_expired', 'true') marca sessão como expirada
5. window.location.href = '/login' redireciona usuário
6. Componente Login detecta flag e exibe mensagem de sessão expirada

---

### Regra de Negócio: Timeout de Requisições

**Visão Geral**:
Configuração de timeout global de 10 segundos para todas as requisições HTTP, prevenindo que clientes aguardem indefinidamente por respostas de endpoints que possam estar lentos ou não responsivos, melhorando a experiência do usuário e liberando recursos do sistema.

**Descrição detalhada**:
O timeout de 10 segundos é aplicado em todas as requisições feitas através da instância Axios, estabelecendo um limite máximo de espera para respostas do backend. Se uma requisição não receber resposta dentro deste período, ela é automaticamente cancelada e rejeitada com um erro de timeout. Esta configuração é especialmente importante para ambientes de produção onde a latência da rede pode variar e endpoints podem ficar temporariamente indisponíveis. O valor de 10 segundos representa um equilíbrio entre permitir tempo suficiente para operações legítimas e evitar que usuários aguardem excessivamente por falhas no sistema.

**Fluxo da regra**:
1. Requisição HTTP é iniciada através da instância api
2. Axios inicia timer de 10 segundos
3. Se resposta chegar antes do timeout → fluxo normal
4. Se timeout ocorrer → requisição é cancelada
5. Promise é rejeitada com erro de timeout
6. Componente trata o erro adequadamente

---

### Regra de Negócio: Logging de Atividades de API

**Visumo Geral**:
Sistema de logging abrangente que registra todas as requisições HTTP (método e URL) e respostas (status code, método e URL) no console do navegador, fornecendo visibilidade completa das operações de API para debugging e monitoramento do comportamento da aplicação em tempo de execução.

**Descrição detalhada**:
O sistema implementa logging em dois pontos cruciais: no request interceptor e no response interceptor. Para requisições, o sistema registra o método HTTP e a URL sendo acessada no formato "API Request: {METHOD} {URL}". Para respostas bem-sucedidas, registra o status code juntamente com método e URL no formato "API Response: {STATUS} {METHOD} {URL}". Para erros, o sistema faz logging diferenciado baseado no tipo de erro: erros de resposta (status code específico), erros de rede (sem resposta do servidor) e outros erros (configuração ou setup). Esta abordagem facilita significativamente o processo de debugging, permitindo desenvolvedores rastrearem o fluxo completo das requisições e identificarem rapidamente onde problemas podem estar ocorrendo.

**Fluxo da regra**:
1. Request interceptor loga: "API Request: {METHOD} {URL}"
2. Requisição é enviada ao backend
3. Response interceptor loga sucesso ou erro
4. Para sucesso: "API Response: {STATUS} {METHOD} {URL}"
5. Para erro response: "API Error Response: {STATUS} {METHOD} {URL}" + dados
6. Para erro de rede: "API Network Error: {message}"
7. Para outros erros: "API Error: {message}"

---

## Estrutura do Componente

```
barbapp-admin/src/services/
├── api.ts                           # Instância Axios configurada com interceptors
├── index.ts                         # Exportação centralizada dos serviços
└── barbershop.service.ts            # Exemplo de serviço utilizando o API client

Estrutura Interna do api.ts:
├── Configuração Axios               # baseURL, timeout, headers padrão
├── Request Interceptor              # Autenticação + logging de requisições
├── Response Interceptor             # Logging + tratamento de erros (401)
└── Export default                   # Instância configurada para uso pela aplicação
```

## Análise de Dependências

### Dependências Internas:
```
barbershopService → api (import)
Login Component → api (import)
Outros serviços → api (import)
```

### Dependências Externas:
- **axios** (^1.6.8) - Cliente HTTP com suporte a interceptors
- **localStorage** - Armazenamento persistente de tokens JWT
- **sessionStorage** - Armazenamento temporário de flags de sessão
- **window.location** - Redirecionamento do navegador
- **import.meta.env** - Variáveis de ambiente (Vite)

### Grafo de Dependências:
```
[Componentes/Serviços] → [api.ts] → [axios] → [Backend API]
                                      ↓
                           [localStorage/sessionStorage/window]
```

## Acoplamento Aferente e Eferente

| Componente | Acoplamento Aferente | Acoplamento Eferente | Crítico |
|-----------|----------------------|----------------------|---------|
| api Service | 8 | 2 | Alto |
| barbershopService | 3 | 1 | Médio |
| Login Component | 1 | 1 | Baixo |

**Análise de Acoplamento:**
- **Acoplamento Aferente (8)**: Alto número de componentes dependem do api Service, indicando sua importância crítica como infraestrutura de comunicação HTTP
- **Acoplamento Eferente (2)**: Baixo acoplamento a dependências externas (axios e browser APIs), indicando boa coesão e isolamento de responsabilidades
- **Impacto**: Mudanças no api Service afetam significativamente a aplicação, mas sua implementação está bem isolada de dependências externas

## Endpoints

O componente api Service é um cliente HTTP genérico e não define endpoints específicos. No entanto, baseado nos serviços que o utilizam, os seguintes endpoints são consumidos através deste componente:

| Endpoint | Método | Descrição |
|----------|--------|-----------|
| /auth/admin-central/login | POST | Autenticação de administrador |
| /barbearias | GET | Listagem de barbearias com paginação e filtros |
| /barbearias/{id} | GET | Obter detalhes de barbearia específica |
| /barbearias | POST | Criar nova barbearia |
| /barbearias/{id} | PUT | Atualizar dados da barbearia |
| /barbearias/{id}/desativar | PUT | Desativar barbearia (soft delete) |
| /barbearias/{id}/reativar | PUT | Reativar barbearia |

## Pontos de Integração

| Integração | Tipo | Propósito | Protocolo | Formato de Dados | Tratamento de Erros |
|------------|------|----------|----------|------------------|--------------------|
| Backend API | Serviço Externo | Comunicação HTTP centralizada | HTTPS/REST | JSON | Interceptor global + redirecionamento 401 |
| localStorage | Browser Storage | Persistência de tokens JWT | N/A | String | Tratamento de ausência de token |
| sessionStorage | Browser Storage | Flags temporárias de sessão | N/A | String | Detecção de sessão expirada |
| Console API | Browser API | Logging para debugging | N/A | Text | Tratamento silencioso de falhas de logging |

## Padrões de Projeto & Arquitetura

| Padrão | Implementação | Localização | Propósito |
|--------|---------------|------------|---------|
| Interceptor Pattern | Request/Response Interceptors | services/api.ts:17-68 | Cross-cutting concerns (auth, logging, error handling) |
| Singleton Pattern | Exported axios instance | services/api.ts:6-12 | Instância única compartilhada |
| Factory Pattern | axios.create() | services/api.ts:6 | Criação configurada de instâncias |
| Environment Configuration | import.meta.env fallback | services/api.ts:7 | Configuração por ambiente |
| Centralized Error Handling | Response interceptor | services/api.ts:41-68 | Tratamento consistente de erros HTTP |
| Automatic Token Injection | Request interceptor | services/api.ts:17-34 | Autenticação transparente |

## Dívida Técnica & Riscos

| Nível de Risco | Área do Componente | Problema | Impacto |
|----------------|-------------------|--------|--------|
| Alto | Token Refresh | Ausência de implementação de refresh token (TODO:64) | Experiência do usuário degradation, força login frequente |
| Médio | Security | Token armazenado em localStorage (vulnerável a XSS) | Risco de comprometimento de credenciais |
| Médio | Error Handling | Tratamento genérico para outros erros HTTP | Debugging dificultado, UX inconsistency |
| Baixo | Performance | Logging síncrono pode impactar performance | Overhead mínimo em requisições frequentes |
| Baixo | Testing | Cobertura limitada de testes de interceptors | Risco de regressões em funcionalidades críticas |

## Análise de Cobertura de Testes

| Componente | Testes Unitários | Testes de Integração | Cobertura | Qualidade dos Testes |
|-----------|------------------|----------------------|----------|---------------------|
| api Service | 0 | 1 (interceptors.test.ts) | ~25% | Testes básicos de configuração, faltam testes de fluxo |
| barbershopService | 0 | 1 (barbershop.service.test.ts) | ~80% | Boa cobertura dos métodos CRUD |

**Arquivos de Teste Identificados:**
- `src/__tests__/integration/services/api.interceptors.test.ts` - Testa configuração básica dos interceptors
- `src/__tests__/integration/services/barbershop.service.test.ts` - Testa integração do serviço com API mockada

**Qualidade dos Testes:**
- **Pontos Fortes**: Configuração adequada de mocks, estrutura organizada dos testes
- **Pontos Fracos**: Falta testes para fluxos de autenticação, tratamento de erros 401, logging
- **Recomendações**: Adicionar testes para request interceptor (token injection), response interceptor (401 handling), e cenários de timeout