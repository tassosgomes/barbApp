using BarbApp.Application.DTOs;
using BarbApp.API.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BarbApp.API.Filters;

/// <summary>
/// Filtro para adicionar exemplos aos schemas do Swagger
/// </summary>
public class SwaggerExamplesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(Microsoft.AspNetCore.Http.IFormFile))
        {
            schema.Type = "string";
            schema.Format = "binary";
            schema.Description = "Arquivo para upload";
            return;
        }
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
        else if (context.Type == typeof(UploadLogoRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["file"] = new OpenApiString("Arquivo de imagem (JPG, PNG ou SVG)")
            };
        }
        else if (context.Type == typeof(CreateBarberInput))
        {
            schema.Example = new OpenApiObject
            {
                ["name"] = new OpenApiString("João Silva"),
                ["email"] = new OpenApiString("joao.silva@email.com"),
                ["password"] = new OpenApiString("Senha@123"),
                ["phone"] = new OpenApiString("11987654321"),
                ["serviceIds"] = new OpenApiArray
                {
                    new OpenApiString("4ea95f64-5717-4562-b3fc-2c963f66afa7"),
                    new OpenApiString("5fb96f75-6828-5673-c4gd-3d974g77bfb8")
                }
            };
        }
        else if (context.Type == typeof(UpdateBarberInput))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                ["name"] = new OpenApiString("João Silva Atualizado"),
                ["phone"] = new OpenApiString("11987654322"),
                ["serviceIds"] = new OpenApiArray
                {
                    new OpenApiString("4ea95f64-5717-4562-b3fc-2c963f66afa7")
                }
            };
        }
        else if (context.Type == typeof(BarberOutput))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                ["name"] = new OpenApiString("João Silva"),
                ["email"] = new OpenApiString("joao.silva@email.com"),
                ["phoneFormatted"] = new OpenApiString("(11) 98765-4321"),
                ["services"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"] = new OpenApiString("4ea95f64-5717-4562-b3fc-2c963f66afa7"),
                        ["name"] = new OpenApiString("Corte de Cabelo"),
                        ["description"] = new OpenApiString("Corte completo com lavagem"),
                        ["durationMinutes"] = new OpenApiInteger(30),
                        ["price"] = new OpenApiDouble(25.00),
                        ["isActive"] = new OpenApiBoolean(true)
                    }
                },
                ["isActive"] = new OpenApiBoolean(true),
                ["createdAt"] = new OpenApiDateTime(DateTime.UtcNow.AddDays(-30))
            };
        }
        else if (context.Type == typeof(PaginatedBarbersOutput))
        {
            schema.Example = new OpenApiObject
            {
                ["barbers"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                        ["name"] = new OpenApiString("João Silva"),
                        ["email"] = new OpenApiString("joao.silva@email.com"),
                        ["phoneFormatted"] = new OpenApiString("(11) 98765-4321"),
                        ["services"] = new OpenApiArray(),
                        ["isActive"] = new OpenApiBoolean(true),
                        ["createdAt"] = new OpenApiDateTime(DateTime.UtcNow.AddDays(-30))
                    }
                },
                ["totalCount"] = new OpenApiInteger(1),
                ["page"] = new OpenApiInteger(1),
                ["pageSize"] = new OpenApiInteger(20)
            };
        }
        else if (context.Type == typeof(TeamScheduleOutput))
        {
            schema.Example = new OpenApiObject
            {
                ["appointments"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"] = new OpenApiString("6gc07g86-7939-6784-d5he-4e085h88cgc9"),
                        ["barberId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                        ["barberName"] = new OpenApiString("João Silva"),
                        ["customerId"] = new OpenApiString("7hd18h97-8a4a-7895-e6if-5f196i99dhda"),
                        ["customerName"] = new OpenApiString("Maria Santos"),
                        ["serviceId"] = new OpenApiString("4ea95f64-5717-4562-b3fc-2c963f66afa7"),
                        ["serviceName"] = new OpenApiString("Corte de Cabelo"),
                        ["scheduledAt"] = new OpenApiDateTime(DateTime.UtcNow.AddHours(2)),
                        ["durationMinutes"] = new OpenApiInteger(30),
                        ["price"] = new OpenApiDouble(25.00),
                        ["status"] = new OpenApiString("Confirmed"),
                        ["notes"] = new OpenApiString("Cliente prefere corte curto")
                    }
                }
            };
        }
        else if (context.Type == typeof(CreateBarbershopServiceInput))
        {
            schema.Example = new OpenApiObject
            {
                ["name"] = new OpenApiString("Corte de Cabelo"),
                ["description"] = new OpenApiString("Corte completo com lavagem e finalização"),
                ["durationMinutes"] = new OpenApiInteger(30),
                ["price"] = new OpenApiDouble(25.00)
            };
        }
        else if (context.Type == typeof(UpdateBarbershopServiceInput))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString("4ea95f64-5717-4562-b3fc-2c963f66afa7"),
                ["name"] = new OpenApiString("Corte de Cabelo Premium"),
                ["description"] = new OpenApiString("Corte completo com lavagem, finalização e barba"),
                ["durationMinutes"] = new OpenApiInteger(45),
                ["price"] = new OpenApiDouble(35.00)
            };
        }
        else if (context.Type == typeof(BarbershopServiceOutput))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString("4ea95f64-5717-4562-b3fc-2c963f66afa7"),
                ["name"] = new OpenApiString("Corte de Cabelo"),
                ["description"] = new OpenApiString("Corte completo com lavagem e finalização"),
                ["durationMinutes"] = new OpenApiInteger(30),
                ["price"] = new OpenApiDouble(25.00),
                ["isActive"] = new OpenApiBoolean(true)
            };
        }
        else if (context.Type == typeof(PaginatedBarbershopServicesOutput))
        {
            schema.Example = new OpenApiObject
            {
                ["services"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"] = new OpenApiString("4ea95f64-5717-4562-b3fc-2c963f66afa7"),
                        ["name"] = new OpenApiString("Corte de Cabelo"),
                        ["description"] = new OpenApiString("Corte completo com lavagem e finalização"),
                        ["durationMinutes"] = new OpenApiInteger(30),
                        ["price"] = new OpenApiDouble(25.00),
                        ["isActive"] = new OpenApiBoolean(true)
                    }
                },
                ["totalCount"] = new OpenApiInteger(1),
                ["page"] = new OpenApiInteger(1),
                ["pageSize"] = new OpenApiInteger(20)
            };
        }
    }
}