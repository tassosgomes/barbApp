---
status: pending
parallelizable: false
blocked_by: ["12.0", "13.0"]
---

<task_context>
<domain>Validação e Qualidade</domain>
<type>Validação</type>
<scope>End-to-End</scope>
<complexity>média</complexity>
<dependencies>Todos os componentes do sistema</dependencies>
<unblocks>None - Tarefa final</unblocks>
</task_context>

# Tarefa 14.0: Validação End-to-End e Ajustes Finais

## Visão Geral
Realizar validação completa end-to-end de todos os fluxos do sistema, executar testes de carga, verificar conformidade com requisitos do PRD, corrigir bugs identificados e realizar refatorações finais para garantir qualidade de produção.

<requirements>
- Validação de todos os fluxos de autenticação
- Testes end-to-end de isolamento multi-tenant
- Testes de performance e carga
- Validação de segurança (OWASP Top 10)
- Verificação de conformidade com PRD
- Correção de bugs identificados
- Refatoração de código onde necessário
- Validação de documentação
- Preparação para deploy
</requirements>

## Subtarefas
- [ ] 14.1 Executar suite completa de testes
- [ ] 14.2 Validar fluxos end-to-end manualmente
- [ ] 14.3 Realizar testes de performance e carga
- [ ] 14.4 Executar análise de segurança
- [ ] 14.5 Verificar conformidade com requisitos do PRD
- [ ] 14.6 Corrigir bugs identificados
- [ ] 14.7 Refatorar código conforme necessário
- [ ] 14.8 Validar documentação Swagger
- [ ] 14.9 Preparar checklist de deploy
- [ ] 14.10 Code review final

## Sequenciamento
- **Bloqueado por**: 12.0 (Documentação), 13.0 (Testes)
- **Desbloqueia**: None (Tarefa final)
- **Paralelizável**: Não (validação final integrada)

## Detalhes de Implementação

### Fluxos End-to-End a Validar

#### Fluxo 1: Autenticação AdminCentral
```
1. POST /api/auth/admin-central/login
   - Validar credenciais corretas retornam 200 + token
   - Validar credenciais incorretas retornam 401
   - Validar input inválido retorna 400
   - Validar token gerado tem claims corretos
   - Validar token expira conforme configurado
```

#### Fluxo 2: Autenticação AdminBarbearia
```
1. POST /api/auth/admin-barbearia/login
   - Validar credenciais corretas retornam 200 + token
   - Validar BarbeariaId correto no token
   - Validar nome da barbearia no response
   - Validar login em barbearia diferente falha
   - Validar isolamento de dados por tenant
```

#### Fluxo 3: Autenticação Barbeiro
```
1. POST /api/auth/barbeiro/login
   - Validar login com BarbeariaId correto
   - Validar token tem contexto de barbearia

2. GET /api/auth/barbeiros (autenticado)
   - Validar retorna apenas barbeiros da barbearia
   - Validar isolamento multi-tenant
   - Validar sem token retorna 401

3. POST /api/auth/barbeiro/trocar-contexto
   - Validar troca para barbearia válida
   - Validar novo token com contexto atualizado
   - Validar falha para barbearia onde não trabalha
```

#### Fluxo 4: Autenticação Cliente
```
1. POST /api/auth/cliente/login
   - Validar credenciais corretas retornam 200 + token
   - Validar token não tem BarbeariaId
   - Validar tipo de usuário correto
```

### Testes de Performance

```bash
# Load testing com k6
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '2m', target: 50 },   // Ramp up to 50 users
    { duration: '5m', target: 50 },   // Stay at 50 users
    { duration: '2m', target: 100 },  // Ramp up to 100 users
    { duration: '5m', target: 100 },  // Stay at 100 users
    { duration: '2m', target: 0 },    // Ramp down to 0 users
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests should be below 500ms
    http_req_failed: ['rate<0.01'],   // Error rate should be below 1%
  },
};

export default function () {
  // Test AdminCentral login
  let payload = JSON.stringify({
    email: 'admin@barbapp.com',
    senha: 'Admin@123',
  });

  let params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  let res = http.post('http://localhost:5000/api/auth/admin-central/login', payload, params);

  check(res, {
    'status is 200': (r) => r.status === 200,
    'has token': (r) => JSON.parse(r.body).token !== undefined,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });

  sleep(1);
}
```

### Security Checklist (OWASP Top 10)

```markdown
## A01: Broken Access Control
- [ ] Isolamento multi-tenant validado (nenhum vazamento de dados)
- [ ] Autorização em todos os endpoints protegidos
- [ ] TenantContext limpo após cada requisição

## A02: Cryptographic Failures
- [ ] Senhas hasheadas com BCrypt (work factor 12)
- [ ] JWT com secret forte (>32 chars)
- [ ] HTTPS enforced em produção
- [ ] Dados sensíveis não em logs

## A03: Injection
- [ ] EF Core parametrização automática (sem SQL injection)
- [ ] FluentValidation em todos os inputs
- [ ] Nenhuma concatenação de queries

## A04: Insecure Design
- [ ] JWT expiration apropriada (8 horas)
- [ ] Rate limiting considerado (próxima fase)
- [ ] Princípio do menor privilégio

## A05: Security Misconfiguration
- [ ] Secrets em variáveis de ambiente (não hardcoded)
- [ ] Detailed errors apenas em dev
- [ ] CORS configurado apropriadamente
- [ ] Security headers configurados

## A07: Identification and Authentication Failures
- [ ] Senhas com requisitos mínimos (6+ chars)
- [ ] JWT validation apropriada
- [ ] Token expiration funciona
- [ ] Credenciais inválidas retornam 401

## A08: Software and Data Integrity Failures
- [ ] Dependências atualizadas
- [ ] CI/CD pipeline com verificações
- [ ] Integridade de dados mantida

## A10: Server-Side Request Forgery
- [ ] Nenhuma chamada externa baseada em input do usuário
- [ ] Validação de URLs se aplicável
```

### PRD Compliance Checklist

```markdown
## Requisitos Funcionais
- [ ] RF-01: Login AdminCentral implementado
- [ ] RF-02: Login AdminBarbearia implementado
- [ ] RF-03: Login Barbeiro implementado
- [ ] RF-04: Login Cliente implementado
- [ ] RF-05: Listagem de barbeiros implementada
- [ ] RF-06: Troca de contexto implementada

## Requisitos Não-Funcionais
- [ ] RNF-01: Isolamento multi-tenant validado
- [ ] RNF-02: JWT com claims apropriados
- [ ] RNF-03: Middleware de tenant funcional
- [ ] RNF-04: Performance adequada (<500ms p95)
- [ ] RNF-05: Segurança (OWASP compliance)
- [ ] RNF-06: Documentação Swagger completa
- [ ] RNF-07: Testes de integração >80% cobertura

## Requisitos Técnicos
- [ ] RT-01: .NET 8 utilizado
- [ ] RT-02: PostgreSQL configurado
- [ ] RT-03: Entity Framework Core funcionando
- [ ] RT-04: Clean Architecture seguida
- [ ] RT-05: Padrões Repository implementados
```

### Checklist de Refatoração

```markdown
## Code Quality
- [ ] Nenhum código duplicado
- [ ] Nomes descritivos e claros
- [ ] Métodos curtos (<50 linhas)
- [ ] Classes com responsabilidade única
- [ ] Comentários apenas onde necessário

## Performance
- [ ] Queries otimizadas (Include apropriado)
- [ ] Índices de database configurados
- [ ] Async/await usado consistentemente
- [ ] Nenhum N+1 query problem

## Maintainability
- [ ] DTOs imutáveis (records)
- [ ] Validadores separados e reutilizáveis
- [ ] Configurações em appsettings.json
- [ ] Logging apropriado
- [ ] Tratamento de erros consistente
```

### Checklist de Deploy

```markdown
## Preparação
- [ ] Secrets movidos para variáveis de ambiente
- [ ] Connection strings parametrizadas
- [ ] JWT secret gerado aleatoriamente
- [ ] Configurações de produção validadas
- [ ] CORS configurado para domínios de produção

## Database
- [ ] Migrations prontas para aplicação
- [ ] Backup strategy definida
- [ ] Rollback plan documentado
- [ ] Seed data preparado (se necessário)

## Infraestrutura
- [ ] Health checks funcionando
- [ ] Logging configurado
- [ ] Monitoring planejado
- [ ] Error tracking configurado (Sentry/AppInsights)

## Documentação
- [ ] README atualizado
- [ ] Swagger acessível
- [ ] Postman collection disponível
- [ ] Guia de deployment documentado
```

### Script de Validação Final

```bash
#!/bin/bash

echo "🚀 BarbApp - Validação Final"
echo "=============================="

# 1. Run all tests
echo "📋 1. Executando testes..."
dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"

if [ $? -ne 0 ]; then
    echo "❌ Testes falhou!"
    exit 1
fi

# 2. Check code coverage
echo "📊 2. Verificando cobertura de código..."
dotnet tool run reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report"

# 3. Run security scan
echo "🔒 3. Executando análise de segurança..."
dotnet list package --vulnerable --include-transitive

# 4. Check for outdated packages
echo "📦 4. Verificando pacotes desatualizados..."
dotnet list package --outdated

# 5. Run code analysis
echo "🔍 5. Executando análise de código..."
dotnet format --verify-no-changes

# 6. Build release configuration
echo "🏗️  6. Building release configuration..."
dotnet build --configuration Release

if [ $? -ne 0 ]; then
    echo "❌ Build falhou!"
    exit 1
fi

echo "✅ Validação concluída com sucesso!"
```

## Critérios de Sucesso
- ✅ Todos os testes passando (unit + integration)
- ✅ Cobertura de código >80%
- ✅ Todos os fluxos end-to-end validados manualmente
- ✅ Performance adequada (p95 <500ms)
- ✅ Nenhuma vulnerabilidade de segurança conhecida
- ✅ Conformidade 100% com PRD
- ✅ Zero bugs críticos ou high-priority
- ✅ Código refatorado e limpo
- ✅ Documentação validada e completa
- ✅ Checklist de deploy completo
- ✅ Code review aprovado

## Tempo Estimado
**4 horas**

## Referências
- TechSpec: Seção "4.9 Fase 1.9: Validação e Ajustes"
- PRD: Todas as seções de requisitos
- OWASP Top 10 2021
- .NET Performance Best Practices

## Observações
Esta é a tarefa final antes do sistema estar pronto para deploy. Certifique-se de que todos os critérios de sucesso são atendidos antes de considerar a implementação completa.
