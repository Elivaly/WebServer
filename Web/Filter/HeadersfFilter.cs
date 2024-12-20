using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AuthService.Filter;

public class HeadersfFilter(IConfiguration config) : IOperationFilter
{
    private readonly IConfiguration _config = config;


    /// <summary>
    /// Swagger: Creates new required http header.
    /// </summary>
    public void Apply(
        OpenApiOperation operation,
        OperationFilterContext context)
    {
        var filterDescriptors = context.ApiDescription.ActionDescriptor.FilterDescriptors;

        var customAuhtorizeFilter = (AuthServiceAttribute?)filterDescriptors
            .Select(filterInfo => filterInfo.Filter)
            .FirstOrDefault(filter => filter is AuthServiceAttribute);

        if (customAuhtorizeFilter != null)
        {
            operation.Parameters.Add(new()
            {
                Name = "_jwt",
                In = ParameterLocation.Header,
                Description = "наличие этого параметра указывает что метод доступен лишь для авторизованных пользователей<br>" +
                $"ПРИМЕЧАНИЕ: заполнять не нужно, реальное значение берётся из Header 'Authorization'",
                // вызывает баг сваггера: нельзя выполнить метод
                //Required = true, 
                AllowEmptyValue = false,
                Schema = new()
                {
                    Type = "String",
                    Default = new OpenApiString("jwt")
                }
            });
        }
    }
}
