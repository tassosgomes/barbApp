using BarbApp.Application.DTOs;
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