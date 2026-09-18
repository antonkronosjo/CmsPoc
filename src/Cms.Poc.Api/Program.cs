using Cms.Framework.Abstractions;
using Cms.Framework.Generated;
using Cms.Framework.Infrastructure;
using Cms.Poc.Domain;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddContentFramework();
builder.Services.AddCmsDbContext("Data Source=cms-poc.db");
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        // Vite's default dev port is 5173, but it falls forward to the next free
        // port (5174, 5175, ...) if something else is already listening on it.
        .WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:5175")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<CmsDbContext>().Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

// ---- News --------------------------------------------------------------

app.MapGet("/api/news", (IContentRepository repo, string lang = "en")
    => repo.Query<NewsContent>(lang).ToList());

app.MapGet("/api/news/{id:int}", (IContentRepository repo, int id, string lang = "en")
    => repo.Query<NewsContent>(lang).Where(x => x.Id == id).FirstOrDefault() is { } news
        ? Results.Ok(news)
        : Results.NotFound());

app.MapGet("/api/news/{id:int}/history", (IContentRepository repo, int id, string lang = "en")
    => repo.QueryHistory<NewsContent>(id, lang));

app.MapPost("/api/news", (IContentRepository repo, CreateNewsRequest request)
    => Results.Ok(repo.Create(new NewsContent
    {
        Name = request.Name,
        Heading = request.Heading,
        Body = request.Body,
        Color = request.Color,
    }, request.Language)));

app.MapPut("/api/news/{id:int}", (IContentRepository repo, int id, UpdateNewsRequest request)
    => Results.Ok(repo.Update(new NewsContent
    {
        Id = id,
        Language = request.Language,
        Heading = request.Heading,
        Body = request.Body,
        Color = request.Color,
    })));

// ---- Events --------------------------------------------------------------

app.MapGet("/api/events", (IContentRepository repo, string lang = "en")
    => repo.Query<EventContent>(lang).ToList());

app.MapGet("/api/events/{id:int}", (IContentRepository repo, int id, string lang = "en")
    => repo.Query<EventContent>(lang).Where(x => x.Id == id).FirstOrDefault() is { } ev
        ? Results.Ok(ev)
        : Results.NotFound());

app.MapGet("/api/events/{id:int}/history", (IContentRepository repo, int id, string lang = "en")
    => repo.QueryHistory<EventContent>(id, lang));

app.MapPost("/api/events", (IContentRepository repo, CreateEventRequest request)
    => Results.Ok(repo.Create(new EventContent
    {
        Name = request.Name,
        Title = request.Title,
        Description = request.Description,
        StartDate = request.StartDate,
    }, request.Language)));

app.MapPut("/api/events/{id:int}", (IContentRepository repo, int id, UpdateEventRequest request)
    => Results.Ok(repo.Update(new EventContent
    {
        Id = id,
        Language = request.Language,
        Title = request.Title,
        Description = request.Description,
        StartDate = request.StartDate,
    })));

// ---- Polymorphic content search ------------------------------------------

app.MapGet("/api/content", (IContentRepository repo, string name, string lang = "en")
    => repo.Query<Content>(lang).Where(x => x.Name == name).ToList().Select(ContentDto.From));

app.Run();

record CreateNewsRequest(string Name, string Language, string Heading, string Body, string Color);
record UpdateNewsRequest(string Language, string Heading, string Body, string Color);
record CreateEventRequest(string Name, string Language, string Title, string Description, DateTime StartDate);
record UpdateEventRequest(string Language, string Title, string Description, DateTime StartDate);

/// <summary>
/// A type-tagged shape for the polymorphic content list - the only place in
/// this demo that needs to know about every concrete content type, since
/// JSON has no notion of "here's a NewsContent, here's an EventContent."
/// </summary>
record ContentDto(string Type, int Id, string Name, string Language, int VersionNumber, object Fields)
{
    public static ContentDto From(Content content) => content switch
    {
        NewsContent n => new ContentDto(nameof(NewsContent), n.Id, n.Name, n.Language, n.VersionNumber,
            new { n.Heading, n.Body, n.Color }),
        EventContent e => new ContentDto(nameof(EventContent), e.Id, e.Name, e.Language, e.VersionNumber,
            new { e.Title, e.Description, e.StartDate }),
        _ => new ContentDto(content.GetType().Name, content.Id, content.Name, content.Language, content.VersionNumber, new { }),
    };
}
