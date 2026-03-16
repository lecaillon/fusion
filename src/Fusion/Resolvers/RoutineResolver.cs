using Fusion.Matchers;
using Fusion.Models;

namespace Fusion.Resolvers;

/// <summary>
/// Resolves database routines (stored procedures and functions) from a
/// <see cref="DatabaseContext"/> using prefix or exact-match filtering
/// via <see cref="SqlIdentifierMatcher"/>.
/// </summary>
public class RoutineResolver
{
    private readonly SqlIdentifierMatcher _matcher;

    /// <summary>
    /// Initializes a new instance of <see cref="RoutineResolver"/>.
    /// </summary>
    /// <param name="matcher">
    /// The identifier matcher used for filtering. When <c>null</c>, a default
    /// (case-insensitive) matcher is created.
    /// </param>
    public RoutineResolver(SqlIdentifierMatcher? matcher = null)
    {
        _matcher = matcher ?? new SqlIdentifierMatcher();
    }

    /// <summary>
    /// Returns all routines whose names start with <paramref name="prefix"/>
    /// across every schema in <paramref name="context"/>.
    /// Pass <c>null</c> or an empty string to return all routines.
    /// </summary>
    public IEnumerable<Routine> Resolve(DatabaseContext context, string? prefix = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Schemas
            .SelectMany(s => s.Routines)
            .Where(r => _matcher.StartsWith(r.Name, prefix));
    }

    /// <summary>
    /// Returns all routines whose names start with <paramref name="prefix"/>
    /// within the specified <paramref name="schemaName"/>.
    /// Pass <c>null</c> or an empty string for <paramref name="prefix"/> to
    /// return all routines in the schema.
    /// </summary>
    public IEnumerable<Routine> Resolve(DatabaseContext context, string schemaName, string? prefix = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(schemaName);

        return context.Schemas
            .Where(s => _matcher.Equals(s.Name, schemaName))
            .SelectMany(s => s.Routines)
            .Where(r => _matcher.StartsWith(r.Name, prefix));
    }

    /// <summary>
    /// Returns only stored procedures whose names start with <paramref name="prefix"/>.
    /// </summary>
    public IEnumerable<Routine> ResolveStoredProcedures(DatabaseContext context, string? prefix = null)
    {
        return Resolve(context, prefix)
            .Where(r => r.Kind == RoutineKind.StoredProcedure);
    }

    /// <summary>
    /// Returns only functions whose names start with <paramref name="prefix"/>.
    /// </summary>
    public IEnumerable<Routine> ResolveFunctions(DatabaseContext context, string? prefix = null)
    {
        return Resolve(context, prefix)
            .Where(r => r.Kind == RoutineKind.Function);
    }

    /// <summary>
    /// Finds a single routine by its exact name (optionally scoped to a schema).
    /// Returns <c>null</c> when no match is found.
    /// </summary>
    public Routine? Find(DatabaseContext context, string name, string? schema = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(name);

        return context.Schemas
            .Where(s => schema is null || _matcher.Equals(s.Name, schema))
            .SelectMany(s => s.Routines)
            .FirstOrDefault(r => _matcher.Equals(r.Name, name));
    }
}
