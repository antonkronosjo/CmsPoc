using Cms.Framework.Abstractions.Users;
using Cms.Framework.AspNetCore;
using Cms.Framework.Infrastructure;
using Cms.Framework.Sqlite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddCms<Cms.Poc.Domain.ContentTypeKey>(cms =>
{
    cms.UseSqlite("Data Source=cms-poc.db", typeof(Program).Assembly);
    cms.MigrateDatabase = true;
    cms.UseUserAdapter<Cms.Poc.Api.PocUserAdapter>();
    cms.UseDatabaseSettingsStore();
});
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

app.MapCms<Cms.Poc.Domain.ContentTypeKey>("/api/content");
app.MapCmsSettings();

// Lets the frontend know whether the caller is signed in, their display name, and which CMS roles they hold.
app.MapGet("/api/user", (ICmsUserAdapter users) =>
{
    var user = users.GetCurrentUser();
    var displayName = user is null ? null : users.ResolveProfiles([user.Id]).GetValueOrDefault(user.Id)?.DisplayName;
    return new CurrentUserDto(
        user is not null,
        displayName,
        user?.Roles.Select(r => r.ToString()).Order().ToArray() ?? []);
});

app.Run();

internal sealed record CurrentUserDto(bool IsAuthenticated, string? DisplayName, string[] Roles);
