using Cms.Framework.Abstractions.Users;
using Cms.Framework.Infrastructure.Editing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Cms.Framework.AspNetCore;

/// <summary>
/// The entire HTTP surface for creating, editing and browsing content -
/// generic over every registered content type. Adding a new content type
/// never requires touching this file.
/// </summary>
public static class CmsEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapCms(this IEndpointRouteBuilder app, string prefix = "/api/content")
    {
        prefix = "/" + prefix.Trim('/');
        var group = app.MapGroup(prefix);

        // Surface user-tracking failures as plain status codes; the framework
        // doesn't own authentication, so there is no challenge to issue.
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
        });

        group.MapGet("/types", (IContentEditingService editing)
            => editing.GetContentTypes());

        group.MapGet("/creationschema", (IContentEditingService editing, string contentTypeName, string language)
            => editing.GetCreationSchema(contentTypeName, language));

        group.MapPost("",(IContentEditingService editing, CreateContentSchema request) =>
        {
            var created = editing.Create(request);
            return editing.GetUpdateSchema(created.Id, created.Language);
        });

        group.MapGet("/{id:int}/updateschema", (IContentEditingService editing, int id, string language, int? version)
            => editing.GetUpdateSchema(id, language, version));

        group.MapPut("/{id:int}", (IContentEditingService editing, int id, UpdateContentSchema request) =>
        {
            editing.Update(request);
            return editing.GetUpdateSchema(id, request.Metadata.Language);
        });

        group.MapGet("/{id:int}", (IContentEditingService editing, int id, string language)
            => editing.GetSummary(id, language) is { } summary
                ? Results.Ok(summary)
                : Results.NotFound());

        group.MapGet("/search", (IContentEditingService editing, string? query, string language, string? contentTypeName, int? page, int? pageSize, bool? publishedOnly)
            => editing.Search(query, language, contentTypeName, page ?? 1, pageSize ?? 20, publishedOnly ?? false));

        group.MapGet("/{id:int}/history", (IContentEditingService editing, int id, string language)
            => editing.GetHistory(id, language));

        group.MapPost("/validate", (IContentEditingService editing, string contentTypeName, string propertyName, ContentPropertyValueDto value)
            => editing.ValidateProperty(contentTypeName, propertyName, value));

        group.MapPost("/{id:int}/publish", (IContentEditingService editing, int id, PublishContentRequest request) =>
        {
            editing.Publish(id, request.VersionNumber, request.StartPublish, request.StopPublish);
            return Results.NoContent();
        });

        group.MapPost("/{id:int}/unpublish", (IContentEditingService editing, int id) =>
        {
            editing.Unpublish(id);
            return Results.NoContent();
        });

        group.MapPost("/users/{userId}/remove-references", (IContentEditingService editing, string userId) =>
        {
            editing.RemoveUserReferences(userId);
            return Results.NoContent();
        });

        return app;
    }
}
