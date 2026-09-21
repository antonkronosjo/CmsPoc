# Content Framework POC

A proof of concept for an internal content framework that lets developers
define flat, database-agnostic content models (`NewsContent`, `EventContent`,
...) while a Roslyn incremental source generator produces the EF Core
persistence layer (root/version/translation tables), globally unique content
IDs, and immutable, copy-on-write versioning behind the scenes.

See `Cms.Poc.Domain/NewsContent.cs` and `Cms.Poc.Domain/EventContent.cs` for
the only hand-written "persistence" code in the whole solution - everything
else under `Cms.Poc.Domain/Generated/` (visible after a build) is produced by
`Cms.Framework.Generator`.

## Using the framework

Mark each content class you want stored with `[ContentType]`:

```csharp
[ContentType]
public class NewsContent : Content { ... }
```

Then register everything with one call - content types are discovered
automatically, no assembly needs to be named:

```csharp
builder.Services.AddCms(cms =>
{
    cms.UseSqlite("Data Source=cms.db");   // or cms.UseDatabase(db => db.UseXxx(...))
    cms.EnsureDatabaseCreated = true;       // optional: create the schema on startup
});

app.MapCms("/api/content");                 // optional HTTP endpoints
```

## Solution layout

```
src/
  Cms.Framework.Abstractions/   Content, [ContentType], [CultureSpecific], IContentRepository, IContentQuery<T> (ships the generator as an analyzer)
  Cms.Framework.Generator/      the Roslyn incremental source generator
  Cms.Framework.Infrastructure/ AddCms, ContentRoot, CmsDbContext, ContentRepository, IContentEditingService, versioning, polymorphic query
  Cms.Framework.Sqlite/         UseSqlite(...) provider shortcut
  Cms.Framework.AspNetCore/     MapCms(...) - the generic HTTP endpoints
  Cms.Poc.Domain/               NewsContent, EventContent - the only flat models a developer writes
  Cms.Poc.Api/                  sample host: AddCms + MapCms plus CORS/OpenAPI
  Cms.Poc.Web/                  minimal React (Vite + TS) demo UI
tests/
  Cms.Framework.Tests/          xUnit tests proving the framework end-to-end against real SQLite
```

## Build & test

```bash
dotnet build CmsFramework.slnx
dotnet test tests/Cms.Framework.Tests/Cms.Framework.Tests.csproj
```

`Cms.Poc.Domain.csproj` sets `EmitCompilerGeneratedFiles` so the generator's
output lands on disk under `src/Cms.Poc.Domain/Generated/` after a build -
open any file there to see the Version/Translation entities, EF
configuration (including the `ToSqlQuery` read model), and store class that
were generated purely from `NewsContent.cs`/`EventContent.cs`.

## Run the API + React demo

```bash
# terminal 1
dotnet run --project src/Cms.Poc.Api/Cms.Poc.Api.csproj --urls http://localhost:5075

# terminal 2
cd src/Cms.Poc.Web
npm install
npm run dev
```

Open the Vite dev server URL it prints (usually http://localhost:5173, or
the next free port). The API's CORS policy allows 5173-5175. The demo lets
you create `NewsContent`/`EventContent`, search across both types by name
(`Query<Content>()`), and edit an item to see a new immutable version appear
in its history.

The API uses a SQLite file (`src/Cms.Poc.Api/cms-poc.db`) created on first
run; delete it to start fresh.

## What to look at first

- `src/Cms.Framework.Generator/CodeEmitter.cs` - what gets generated per content type.
- `src/Cms.Framework.Infrastructure/PolymorphicContentQuery.cs` - the two-phase, all-SQL implementation of `Query<Content>()`.
- `src/Cms.Framework.Infrastructure/ContentRepository.cs` - the single generic entry point the service layer uses.
- `tests/Cms.Framework.Tests/` - one test class per POC requirement from the design plan.
