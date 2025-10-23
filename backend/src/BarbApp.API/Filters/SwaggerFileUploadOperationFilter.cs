using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace BarbApp.API.Filters;

/// <summary>
/// Filtro para corrigir a documentação do Swagger para uploads de arquivos IFormFile
/// </summary>
public class SwaggerFileUploadOperationFilter : IOperationFilter, IParameterFilter
{
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (context.ParameterInfo.ParameterType == typeof(Microsoft.AspNetCore.Http.IFormFile))
        {
            // Don't generate parameter for IFormFile, it will be handled in the operation filter
            parameter.Schema = null;
            parameter.Required = false;
        }
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParameters = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(Microsoft.AspNetCore.Http.IFormFile))
            .ToArray();

        if (!fileParameters.Any())
            return;

        // Remove file parameters from the operation parameters
        foreach (var param in fileParameters)
        {
            var parameter = operation.Parameters.FirstOrDefault(p => p.Name == param.Name);
            if (parameter != null)
            {
                operation.Parameters.Remove(parameter);
            }
        }

        // Ensure request body exists
        if (operation.RequestBody == null)
        {
            operation.RequestBody = new OpenApiRequestBody();
        }

        if (operation.RequestBody.Content == null)
        {
            operation.RequestBody.Content = new Dictionary<string, OpenApiMediaType>();
        }

        // Add multipart/form-data content type
        operation.RequestBody.Content["multipart/form-data"] = new OpenApiMediaType
        {
            Schema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>()
            }
        };

        // Add file properties
        foreach (var param in fileParameters)
        {
            operation.RequestBody.Content["multipart/form-data"].Schema.Properties[param.Name] = new OpenApiSchema
            {
                Type = "string",
                Format = "binary",
                Description = "Arquivo para upload"
            };
        }

        // Add other form parameters if any
        var otherFormParameters = context.MethodInfo.GetParameters()
            .Where(p => !fileParameters.Contains(p) &&
                       p.GetCustomAttribute(typeof(Microsoft.AspNetCore.Mvc.FromFormAttribute)) != null)
            .ToArray();

        foreach (var param in otherFormParameters)
        {
            var schema = context.SchemaGenerator.GenerateSchema(param.ParameterType, context.SchemaRepository);
            operation.RequestBody.Content["multipart/form-data"].Schema.Properties[param.Name] = schema;
        }
    }
}