using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;

namespace Tasky.Gateway;

public static class OpenApiOptionsExtensions
{
    public static OpenApiOptions UseJwtBearerAuthentication(this OpenApiOptions options)
    {
        // Create the security scheme
        var securityScheme = new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.SecuritySchemeType.Http,
            Name = JwtBearerDefaults.AuthenticationScheme,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
        };

        // Create the security scheme reference with required parameter
        var schemeReference = new Microsoft.OpenApi.OpenApiSecuritySchemeReference(
            JwtBearerDefaults.AuthenticationScheme
        );

        options.AddDocumentTransformer(
            (document, context, cancellationToken) =>
            {
                document.Components ??= new();
                document.Components.SecuritySchemes!.Add(
                    JwtBearerDefaults.AuthenticationScheme,
                    securityScheme
                );

                return Task.CompletedTask;
            }
        );

        options.AddOperationTransformer(
            (operation, context, cancellationToken) =>
            {
                if (
                    context
                        .Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>()
                        .Any()
                )
                {
                    operation.Security = [new() { [schemeReference] = [] }];
                }

                return Task.CompletedTask;
            }
        );

        return options;
    }
}
