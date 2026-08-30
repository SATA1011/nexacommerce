using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NexaCommerce.Api.Filters;

public sealed class AuthorizeCheckDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Components ??= new OpenApiComponents();
        swaggerDoc.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        if (!swaggerDoc.Components.SecuritySchemes.ContainsKey("Bearer"))
        {
            swaggerDoc.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste your JWT access token below (without 'Bearer ' prefix):"
            };
        }

        var schemeRef = new OpenApiSecuritySchemeReference("Bearer", swaggerDoc);

        foreach (var apiDesc in context.ApiDescriptions)
        {
            var hasAuthorize = apiDesc.CustomAttributes().OfType<AuthorizeAttribute>().Any()
                || apiDesc.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().Any();

            if (!hasAuthorize) continue;

            var routeKey = "/" + apiDesc.RelativePath?.TrimStart('/');
            Console.WriteLine($"[DocumentFilter] Protected Route: '{routeKey}', Method: '{apiDesc.HttpMethod}'");

            var matchingPath = swaggerDoc.Paths.FirstOrDefault(p => string.Equals(p.Key, routeKey, StringComparison.OrdinalIgnoreCase));
            if (matchingPath.Value != null && matchingPath.Value.Operations != null)
            {
                foreach (var opKvp in matchingPath.Value.Operations)
                {
                    if (string.Equals(opKvp.Key.ToString(), apiDesc.HttpMethod, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"[DocumentFilter] MATCHED Operation for '{routeKey}'!");
                        opKvp.Value.Security = new List<OpenApiSecurityRequirement>
                        {
                            new OpenApiSecurityRequirement
                            {
                                { schemeRef, new List<string>() }
                            }
                        };
                    }
                }
            }
            else
            {
                Console.WriteLine($"[DocumentFilter] NO MATCH for '{routeKey}' in swaggerDoc.Paths!");
            }
        }

        swaggerDoc.RegisterComponents();
    }
}
