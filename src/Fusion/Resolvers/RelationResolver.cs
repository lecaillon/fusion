using Fusion.Matchers;
using Fusion.Models;

namespace Fusion.Resolvers;

/// <summary>
/// Resolves database relations (tables and views) from a <see cref="DatabaseContext"/>
/// using prefix or exact-match filtering via <see cref="SqlIdentifierMatcher"/>.
/// </summary>
public class RelationResolver
{
    private readonly SqlIdentifierMatcher _matcher;

    /// <summary>
    /// Initializes a new instance of <see cref="RelationResolver"/>.
    /// </summary>
    /// <param name="matcher">
    /// The identifier matcher used for filtering. When <c>null</c>, a default
    /// (case-insensitive) matcher is created.
    /// </param>
    public RelationResolver(SqlIdentifierMatcher? matcher = null)
    {
        _matcher = matcher ?? new SqlIdentifierMatcher();
    }

    /// <summary>
    /// Returns all relations whose names start with <paramref name="prefix"/>
    /// across every schema in <paramref name="context"/>.
    /// Pass <c>null</c> or an empty string to return all relations.
    /// </summary>
    public IEnumerable<Relation> Resolve(DatabaseContext context, string? prefix = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Schemas
            .SelectMany(s => s.Relations)
            .Where(r => _matcher.StartsWith(r.Name, prefix));
    }

    /// <summary>
    /// Returns all relations whose names start with <paramref name="prefix"/>
    /// within the specified <paramref name="schemaName"/>.
    /// Pass <c>null</c> or an empty string for <paramref name="prefix"/> to
    /// return all relations in the schema.
    /// </summary>
    public IEnumerable<Relation> Resolve(DatabaseContext context, string schemaName, string? prefix = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(schemaName);

        return context.Schemas
            .Where(s => _matcher.Equals(s.Name, schemaName))
            .SelectMany(s => s.Relations)
            .Where(r => _matcher.StartsWith(r.Name, prefix));
    }

    /// <summary>
    /// Finds a single relation by its exact schema-qualified or unqualified name.
    /// Returns <c>null</c> when no match is found.
    /// </summary>
    public Relation? Find(DatabaseContext context, string name, string? schema = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(name);

        return context.Schemas
            .Where(s => schema is null || _matcher.Equals(s.Name, schema))
            .SelectMany(s => s.Relations)
            .FirstOrDefault(r => _matcher.Equals(r.Name, name));
    }
}
