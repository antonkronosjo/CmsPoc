using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Cms.Framework.Infrastructure;

/// <summary>
/// Finds assemblies carrying a <see cref="ContentTypeRegistrarAttribute"/>
/// by combining what is already loaded with a walk of the entry assembly's
/// references (references are loaded lazily, so a content assembly the host
/// hasn't touched yet is not in the AppDomain).
/// </summary>
internal static class ContentAssemblyDiscovery
{
    private static readonly string[] SkippedPrefixes =
    {
        "System", "Microsoft", "netstandard", "mscorlib", "WindowsBase", "Cms.Framework.",
    };

    public static IReadOnlyList<Assembly> Find()
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<Assembly>();

        foreach (var loaded in AppDomain.CurrentDomain.GetAssemblies())
            Enqueue(loaded);
        if (Assembly.GetEntryAssembly() is { } entry)
            Enqueue(entry);

        // The compiler drops references that are never used in code, so a host that
        // never names a domain type has no assembly reference to it. deps.json still
        // lists every project/package the app ships with.
        foreach (var name in DependentAssemblyNames())
        {
            if (visited.Contains(name.FullName)) continue;
            try { Enqueue(Assembly.Load(name)); }
            catch (Exception ex) when (ex is FileNotFoundException or FileLoadException or BadImageFormatException) { }
        }

        var found = new List<Assembly>();
        while (queue.Count > 0)
        {
            var assembly = queue.Dequeue();
            if (assembly.IsDynamic || IsSkipped(assembly.GetName().Name)) continue;

            if (assembly.GetCustomAttributes<ContentTypeRegistrarAttribute>().Any())
                found.Add(assembly);

            foreach (var reference in assembly.GetReferencedAssemblies())
            {
                if (IsSkipped(reference.Name) || visited.Contains(reference.FullName)) continue;
                try { Enqueue(Assembly.Load(reference)); }
                catch (Exception ex) when (ex is FileNotFoundException or FileLoadException or BadImageFormatException) { }
            }
        }

        return found;

        void Enqueue(Assembly assembly)
        {
            if (visited.Add(assembly.FullName ?? assembly.GetName().Name ?? string.Empty))
                queue.Enqueue(assembly);
        }
    }

    /// <summary>Runtime libraries that depend on the framework, i.e. can plausibly hold content types.</summary>
    private static IEnumerable<AssemblyName> DependentAssemblyNames()
        => DependencyContext.Default?.RuntimeLibraries
            .Where(lib => !IsSkipped(lib.Name) && lib.Dependencies.Any(d => d.Name.StartsWith("Cms.Framework.", StringComparison.Ordinal)))
            .SelectMany(lib => lib.GetDefaultAssemblyNames(DependencyContext.Default))
            ?? Enumerable.Empty<AssemblyName>();

    private static bool IsSkipped(string? name)
        => name is null || SkippedPrefixes.Any(p => name.StartsWith(p, StringComparison.Ordinal));
}
