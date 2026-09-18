using Cms.Framework.Generated;
using Cms.Framework.Infrastructure;
using Cms.Framework.Infrastructure.Editing;
using Cms.Poc.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddContentFramework();
builder.Services.AddCmsDbContext("Data Source=cms-poc.db");
builder.Services.AddContentEditing();
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

app.MapContentEndpoints();

app.Run();
