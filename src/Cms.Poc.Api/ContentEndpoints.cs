using Cms.Framework.Infrastructure.Editing;

namespace Cms.Poc.Api;

/// <summary>
/// The entire HTTP surface for creating, editing and browsing content -
/// generic over every registered content type. Adding a new content type
/// to <c>Cms.Poc.Domain</c> never requires touching this file.
/// </summary>
public static class ContentEndpoints
{
    public static WebApplication MapContentEndpoints(this WebApplication app)
    {
        app.MapGet("/api/content/types", (IContentEditingService editing)
            => editing.GetContentTypes());

        app.MapGet("/api/content/creationschema", (IContentEditingService editing, string contentTypeName, string language)
            => editing.GetCreationSchema(contentTypeName, language));

        app.MapPost("/api/content", (IContentEditingService editing, CreateContentSchema request) =>
        {
            var created = editing.Create(request);
            return editing.GetUpdateSchema(created.Id, created.Language);
        });

        app.MapGet("/api/content/{id:int}/updateschema", (IContentEditingService editing, int id, string language, int? version)
            => editing.GetUpdateSchema(id, language, version));

        app.MapPut("/api/content/{id:int}", (IContentEditingService editing, int id, UpdateContentSchema request) =>
        {
            editing.Update(request);
            return editing.GetUpdateSchema(id, request.Metadata.Language);
        });

        app.MapGet("/api/content/{id:int}", (IContentEditingService editing, int id, string language)
            => editing.GetSummary(id, language) is { } summary
                ? Results.Ok(summary)
                : Results.NotFound());

        app.MapGet("/api/content/search", (IContentEditingService editing, string? query, string language, string? contentTypeName, int? page, int? pageSize)
            => editing.Search(query, language, contentTypeName, page ?? 1, pageSize ?? 20));

        app.MapGet("/api/content/{id:int}/history", (IContentEditingService editing, int id, string language)
            => editing.GetHistory(id, language));

        app.MapPost("/api/content/validate", (IContentEditingService editing, string contentTypeName, string propertyName, ContentPropertyValueDto value)
            => editing.ValidateProperty(contentTypeName, propertyName, value));

        return app;
    }
}
