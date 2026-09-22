using Cms.Framework.Abstractions.Settings;
using Cms.Framework.Abstractions.Users;
using Cms.Framework.Infrastructure.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Cms.Framework.AspNetCore;

/// <summary>The HTTP surface for reading and admin-editing CMS-wide settings.</summary>
public static class CmsSettingsEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapCmsSettings(this IEndpointRouteBuilder app, string prefix = "/api/settings")
    {
        prefix = "/" + prefix.Trim('/');
        var group = app.MapGroup(prefix);

        group.AddEndpointFilter(async (context, next) =>
        {
            try
            {
                return await next(context);
            }
            catch (CmsUnauthenticatedException)
            {
                return Results.StatusCode(StatusCodes.Status401Unauthorized);
            }
            catch (CmsForbiddenException)
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }
            catch (CmsSettingsNotConfiguredException ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status501NotImplemented);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
            }
        });

        group.MapGet("/languages", (ICmsSettingsService settings)
            => settings.GetLanguageSettings());

        group.MapPut("/languages", (ICmsSettingsService settings, LanguageSettings request) =>
        {
            settings.UpdateLanguageSettings(request);
            return Results.NoContent();
        });

        return app;
    }
}
