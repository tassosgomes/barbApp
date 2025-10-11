---
status: completed_reviewed
review_date: 2025-10-11
review_status: approved
parallelizable: false
blocked_by: []
---

<task_context>
<domain>infrastructure/setup</domain>
<type>configuration</type>
<scope>project_foundation</scope>
<complexity>low</complexity>
<dependencies>nuget_packages|environment_config</dependencies>
<unblocks>"2.0"</unblocks>
</task_context>

# Tarefa 1.0: Setup e Dependências

## Visão Geral

Configurar ambiente inicial do projeto com instalação de pacotes NuGet necessários para autenticação JWT, hashing de senhas e configuração de variáveis de ambiente. Esta é a tarefa fundacional que habilita todas as demais.

<requirements>
- Criar estrutura do projeto na pasta backend
- Instalar pacotes NuGet: System.IdentityModel.Tokens.Jwt, BCrypt.Net-Next
- Configurar variáveis de ambiente no appsettings.json
- Gerar secret key JWT segura (mínimo 32 caracteres)
- Configurar estrutura de projetos (Domain, Application, Infrastructure, API)
</requirements>

## Subtarefas

- [x] 1.1 Instalar System.IdentityModel.Tokens.Jwt no projeto API
- [x] 1.2 Instalar BCrypt.Net-Next no projeto Infrastructure
- [x] 1.3 Adicionar seção Jwt no appsettings.json com SecretKey, Issuer, Audience
- [x] 1.4 Gerar secret key segura (openssl rand -base64 32)
- [x] 1.5 Adicionar appsettings.Development.json ao .gitignore
- [x] 1.6 Documentar variáveis de ambiente necessárias

## Sequenciamento

- **Bloqueado por**: Nenhum (primeira tarefa)
- **Desbloqueia**: 2.0 (Domain Layer Base)
- **Paralelizável**: Não (fundação do projeto)

## Detalhes de Implementação

### Pacotes NuGet

```bash
# No projeto BarbApp.API
dotnet add package System.IdentityModel.Tokens.Jwt

# No projeto BarbApp.Infrastructure
dotnet add package BCrypt.Net-Next
```

### Configuração appsettings.json

```json
{
  "Jwt": {
    "SecretKey": "sua-chave-secreta-aqui-minimo-32-caracteres",
    "Issuer": "barbapp",
    "Audience": "barbapp-api",
    "ExpirationHours": 24
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=barbapp;Username=postgres;Password=postgres"
  }
}
```

### Geração de Secret Key Segura

```bash
# Linux/Mac
openssl rand -base64 32

# PowerShell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
```

### Estrutura de Projetos Esperada

```
barbApp/
└── backend/
    ├── src/
    │   ├── BarbApp.Domain/          # Camada de Domínio (Core)
    │   ├── BarbApp.Application/     # Camada de Aplicação (Use Cases)
    │   ├── BarbApp.Infrastructure/  # Camada de Infraestrutura (Implementações)
    │   └── BarbApp.API/            # Camada de Apresentação (Web API)
    └── tests/
        ├── BarbApp.Domain.Tests/
        ├── BarbApp.Application.Tests/
        └── BarbApp.IntegrationTests/
```

## Critérios de Sucesso

- ✅ Todos os pacotes instalados sem conflitos de versão
- ✅ appsettings.json configurado com secret key de 32+ caracteres
- ✅ appsettings.Development.json não commitado (no .gitignore)
- ✅ Documentação criada com variáveis de ambiente necessárias
- ✅ Build do projeto executando sem erros: `dotnet build`
- ✅ Secret key testada (consegue gerar string base64 de 44+ caracteres)

## Tempo Estimado

**2 horas**

## Referências

- TechSpec: Seção "Sequenciamento de Desenvolvimento" - Fase 1.1
- TechSpec: Seção "Pontos de Integração" - JWT Token Generator
- PRD: Seção "Restrições Técnicas de Alto Nível" - Segurança

---

## 📋 Revisão e Aprovação

### Status da Revisão
- ✅ **APROVADA COM LOUVOR**
- 📅 Data: 2025-10-11
- 👤 Revisor: GitHub Copilot (IA)

### Checklist de Validação
- [x] ✅ Todos os requisitos da tarefa implementados
- [x] ✅ Alinhamento com PRD validado
- [x] ✅ Conformidade com TechSpec verificada
- [x] ✅ Regras de código analisadas (aplicáveis)
- [x] ✅ Padrões de commits validados
- [x] ✅ Build sem erros ou warnings
- [x] ✅ Documentação completa e precisa
- [x] ✅ Estrutura pronta para próxima fase

### Relatório Completo
Veja o relatório detalhado de revisão em: `1_task_review.md`

### Próximos Passos
✅ Pronto para iniciar **Tarefa 2.0 - Implementar Domain Layer Base**

