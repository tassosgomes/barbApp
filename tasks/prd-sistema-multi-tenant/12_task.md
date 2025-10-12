---
status: completed
parallelizable: true
blocked_by: ["11.0"]
---

<task_context>
<domain>Documentação</domain>
<type>Documentação</type>
<scope>OpenAPI/Swagger</scope>
<complexity>baixa</complexity>
<dependencies>Swagger, Controllers</dependencies>
<unblocks>"14.0"</unblocks>
</task_context>

# Tarefa 12.0: Documentar Endpoints com Swagger

## Visão Geral
Criar documentação completa dos endpoints usando Swagger/OpenAPI com exemplos de requisições, respostas, schemas detalhados e guia de autenticação.

<requirements>
- Documentação XML para todos os endpoints
- Exemplos de requisições para cada endpoint
- Exemplos de respostas (sucesso e erro)
- Schemas detalhados de DTOs
- Guia de autenticação JWT
- Descrições claras de parâmetros
- Códigos de status HTTP documentados
- Collection Postman exportável
</requirements>

## Subtarefas
- [x] 12.1 Adicionar comentários XML em todos os controllers ✅ CONCLUÍDA
- [x] 12.2 Criar exemplos de requisição para cada endpoint ✅ CONCLUÍDA
- [x] 12.3 Criar exemplos de resposta (sucesso e erro) ✅ CONCLUÍDA
- [x] 12.4 Documentar schemas de DTOs ✅ CONCLUÍDA
- [x] 12.5 Criar guia de autenticação na documentação ✅ CONCLUÍDA
- [x] 12.6 Adicionar descrições de códigos de status ✅ CONCLUÍDA
- [x] 12.7 Exportar collection Postman ✅ CONCLUÍDA
- [x] 12.8 Criar README com instruções de uso da API ✅ CONCLUÍDA

## Sequenciamento
- **Bloqueado por**: 11.0 (Configuração de API)
- **Desbloqueia**: 14.0 (Validação End-to-End)
- **Paralelizável**: Sim (pode ser desenvolvido em paralelo com 13.0)

## Detalhes de Implementação

### Swagger Configuration Avançada

```csharp
// No Program.cs
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BarbApp API",
        Version = "v1.0.0",
        Description = @"
            API REST para sistema de gerenciamento de barbearias multi-tenant.

            ## Autenticação
            Esta API utiliza JWT Bearer tokens para autenticação.

            ### Como obter um token:
            1. Faça login usando um dos endpoints de autenticação
            2. Copie o token retornado no campo 'token'
            3. Clique no botão 'Authorize' no topo desta página
            4. Digite 'Bearer {seu_token}' no campo de entrada
            5. Clique em 'Authorize' para salvar

            ### Tipos de Usuário:
            - **AdminCentral**: Acesso total ao sistema
            - **AdminBarbearia**: Acesso administrativo a uma barbearia específica
            - **Barbeiro**: Acesso a operações de barbeiro em uma ou mais barbearias
            - **Cliente**: Acesso a funcionalidades de cliente

            ## Multi-tenancy
            O sistema suporta múltiplas barbearias isoladas.
            Usuários AdminBarbearia e Barbeiro têm acesso restrito aos dados de suas barbearias.
        ",
        Contact = new OpenApiContact
        {
            Name = "BarbApp Team",
            Email = "support@barbapp.com",
            Url = new Uri("https://barbapp.com")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header usando Bearer scheme.
                      Digite 'Bearer' [espaço] e então seu token.
                      Exemplo: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Custom schema IDs
    c.CustomSchemaIds(type => type.FullName);

    // Add examples
    c.SchemaFilter<SwaggerExamplesSchemaFilter>();
});
```

### Swagger Examples Schema Filter

```csharp
public class SwaggerExamplesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(LoginAdminCentralInput))
        {
            schema.Example = new OpenApiObject
            {
                ["email"] = new OpenApiString("admin@barbapp.com"),
                ["senha"] = new OpenApiString("Admin@123")
            };
        }
        else if (context.Type == typeof(LoginAdminBarbeariaInput))
        {
            schema.Example = new OpenApiObject
            {
                ["codigo"] = new OpenApiString("ABC12345"),
                ["email"] = new OpenApiString("admin@barbearia1.com"),
                ["senha"] = new OpenApiString("Admin@123")
            };
        }
        else if (context.Type == typeof(LoginBarbeiroInput))
        {
            schema.Example = new OpenApiObject
            {
                ["codigo"] = new OpenApiString("ABC12345"),
                ["telefone"] = new OpenApiString("11987654321")
            };
        }
        else if (context.Type == typeof(LoginClienteInput))
        {
            schema.Example = new OpenApiObject
            {
                ["codigo"] = new OpenApiString("ABC12345"),
                ["telefone"] = new OpenApiString("11987654321"),
                ["nome"] = new OpenApiString("João Silva")
            };
        }
        else if (context.Type == typeof(TrocarContextoInput))
        {
            schema.Example = new OpenApiObject
            {
                ["novaBarbeariaId"] = new OpenApiString("4ea95f64-5717-4562-b3fc-2c963f66afa7")
            };
        }
        else if (context.Type == typeof(AuthResponse))
        {
            schema.Example = new OpenApiObject
            {
                ["token"] = new OpenApiString("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."),
                ["tipoUsuario"] = new OpenApiString("AdminBarbearia"),
                ["barbeariaId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                ["nomeBarbearia"] = new OpenApiString("Barbearia Premium"),
                ["expiresAt"] = new OpenApiDateTime(DateTime.UtcNow.AddHours(8))
            };
        }
        else if (context.Type == typeof(AuthenticationOutput))
        {
            schema.Example = new OpenApiObject
            {
                ["userId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                ["name"] = new OpenApiString("João Silva"),
                ["role"] = new OpenApiString("Barbeiro"),
                ["barbeariaId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                ["barbeariaCode"] = new OpenApiString("ABC12345"),
                ["barbeariaNome"] = new OpenApiString("Barbearia Premium")
            };
        }
        else if (context.Type == typeof(BarberInfo))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                ["nome"] = new OpenApiString("João Silva"),
                ["telefone"] = new OpenApiString("11987654321"),
                ["barbeariaId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                ["nomeBarbearia"] = new OpenApiString("Barbearia Premium")
            };
        }
    }
}
```

### README.md da API

```markdown
# BarbApp API Documentation

## Visão Geral
API REST para sistema de gerenciamento de barbearias com suporte multi-tenant.

## Base URL
- **Development**: `http://localhost:5000`
- **Production**: `https://api.barbapp.com`

## Autenticação
Todos os endpoints protegidos requerem autenticação JWT Bearer token.

### Obtendo um Token
1. Faça login usando um dos endpoints de autenticação
2. Use o token retornado no header `Authorization: Bearer {token}`

### Exemplo
```bash
# Login
curl -X POST http://localhost:5000/api/auth/admin-central/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@barbapp.com","senha":"Admin@123"}'

# Resposta
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tipoUsuario": "AdminCentral",
  "barbeariaId": null,
  "nomeBarbearia": "",
  "expiresAt": "2024-01-15T18:00:00Z"
}

# Usando o token
curl -X GET http://localhost:5000/api/auth/barbeiros \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

## Endpoints

### Autenticação

#### POST /api/auth/admin-central/login
Autentica um administrador central.

**Request Body:**
```json
{
  "email": "admin@barbapp.com",
  "senha": "Admin@123"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tipoUsuario": "AdminCentral",
  "barbeariaId": null,
  "nomeBarbearia": "",
  "expiresAt": "2024-01-15T18:00:00Z"
}
```

#### POST /api/auth/admin-barbearia/login
Autentica um administrador de barbearia.

**Request Body:**
```json
{
  "codigo": "ABC12345",
  "email": "admin@barbearia1.com",
  "senha": "Admin@123"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tipoUsuario": "AdminBarbearia",
  "barbeariaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nomeBarbearia": "Barbearia Premium",
  "expiresAt": "2024-01-15T18:00:00Z"
}
```

#### POST /api/auth/barbeiro/login
Autentica um barbeiro.

**Request Body:**
```json
{
  "codigo": "ABC12345",
  "telefone": "11987654321"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tipoUsuario": "Barbeiro",
  "barbeariaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nomeBarbearia": "Barbearia Premium",
  "expiresAt": "2024-01-15T18:00:00Z"
}
```

#### POST /api/auth/cliente/login
Autentica um cliente.

**Request Body:**
```json
{
  "codigo": "ABC12345",
  "telefone": "11987654321",
  "nome": "João Silva"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tipoUsuario": "Cliente",
  "barbeariaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nomeBarbearia": "Barbearia Premium",
  "expiresAt": "2024-01-15T18:00:00Z"
}
```

#### GET /api/auth/barbeiros
Lista barbeiros da barbearia do usuário autenticado. **[Requer Autenticação]**

**Response 200:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nome": "João Silva",
    "telefone": "11987654321",
    "barbeariaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nomeBarbearia": "Barbearia Premium"
  }
]
```

#### POST /api/auth/barbeiro/trocar-contexto
Troca contexto de barbearia para barbeiro. **[Requer Autenticação]**

**Request Body:**
```json
{
  "novaBarbeariaId": "4ea95f64-5717-4562-b3fc-2c963f66afa7"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tipoUsuario": "Barbeiro",
  "barbeariaId": "4ea95f64-5717-4562-b3fc-2c963f66afa7",
  "nomeBarbearia": "Barbearia Nova",
  "expiresAt": "2024-01-15T18:00:00Z"
}
```

## Tipos de Usuário

| Tipo | Descrição | Acesso |
|------|-----------|--------|
| AdminCentral | Administrador do sistema | Acesso total |
| AdminBarbearia | Administrador de barbearia | Dados da barbearia |
| Barbeiro | Profissional barbeiro | Dados da(s) barbearia(s) |
| Cliente | Cliente do sistema | Dados pessoais e agendamentos |

## Códigos de Status

| Código | Descrição |
|--------|-----------|
| 200 | Sucesso |
| 400 | Requisição inválida |
| 401 | Não autenticado |
| 403 | Sem permissão |
| 404 | Não encontrado |
| 500 | Erro interno |

## Postman Collection
Importe a collection do Postman: [BarbApp.postman_collection.json](./BarbApp.postman_collection.json)

## Swagger UI
Acesse a documentação interativa em: http://localhost:5000/swagger
```

### Collection Postman

```json
{
  "info": {
    "name": "BarbApp API",
    "description": "Collection para testar endpoints da BarbApp API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Auth",
      "item": [
        {
          "name": "Login Admin Central",
          "request": {
            "method": "POST",
            "header": [{"key": "Content-Type", "value": "application/json"}],
            "body": {
              "mode": "raw",
              "raw": "{\n  \"email\": \"admin@barbapp.com\",\n  \"senha\": \"Admin@123\"\n}"
            },
            "url": {
              "raw": "{{baseUrl}}/api/auth/admin-central/login",
              "host": ["{{baseUrl}}"],
              "path": ["api", "auth", "admin-central", "login"]
            }
          }
        },
        {
          "name": "Login Admin Barbearia",
          "request": {
            "method": "POST",
            "header": [{"key": "Content-Type", "value": "application/json"}],
            "body": {
              "mode": "raw",
              "raw": "{\n  \"email\": \"admin@barbearia1.com\",\n  \"senha\": \"Admin@123\",\n  \"barbeariaId\": \"{{barbeariaId}}\"\n}"
            },
            "url": {
              "raw": "{{baseUrl}}/api/auth/admin-barbearia/login",
              "host": ["{{baseUrl}}"],
              "path": ["api", "auth", "admin-barbearia", "login"]
            }
          }
        },
        {
          "name": "List Barbeiros",
          "request": {
            "method": "GET",
            "header": [{"key": "Authorization", "value": "Bearer {{token}}"}],
            "url": {
              "raw": "{{baseUrl}}/api/auth/barbeiros",
              "host": ["{{baseUrl}}"],
              "path": ["api", "auth", "barbeiros"]
            }
          }
        }
      ]
    }
  ],
  "variable": [
    {
      "key": "baseUrl",
      "value": "http://localhost:5000"
    },
    {
      "key": "token",
      "value": ""
    },
    {
      "key": "barbeariaId",
      "value": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    }
  ]
}
```

## Critérios de Sucesso
- ✅ Swagger UI acessível e funcional
- ✅ Todos os endpoints documentados com comentários XML
- ✅ Exemplos de requisição para cada endpoint
- ✅ Exemplos de resposta (sucesso e erros)
- ✅ Schemas de DTOs com descrições claras
- ✅ Guia de autenticação JWT completo
- ✅ Códigos de status HTTP documentados
- ✅ Collection Postman funcional
- ✅ README com instruções de uso
- ✅ Documentação clara e profissional

## Tempo Estimado
**2 horas**

## Referências
- TechSpec: Seção "4.7 Fase 1.7: Documentação Swagger"
- PRD: Seção "Requisitos de Documentação"
- Swagger/OpenAPI Specification
