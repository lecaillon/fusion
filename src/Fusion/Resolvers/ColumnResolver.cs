using Fusion.Matchers;
using Fusion.Models;

namespace Fusion.Resolvers;

/// <summary>
/// Resolves columns from a specific <see cref="Relation"/> or across multiple relations
/// using prefix or exact-match filtering via <see cref="SqlIdentifierMatcher"/>.
/// </summary>
public class ColumnResolver
{
    private readonly SqlIdentifierMatcher _matcher;

    /// <summary>
    /// Initializes a new instance of <see cref="ColumnResolver"/>.
    /// </summary>
    /// <param name="matcher">
    /// The identifier matcher used for filtering. When <c>null</c>, a default
    /// (case-insensitive) matcher is created.
    /// </param>
    public ColumnResolver(SqlIdentifierMatcher? matcher = null)
    {
        _matcher = matcher ?? new SqlIdentifierMatcher();
    }

    /// <summary>
    /// Returns all columns from <paramref name="relation"/> whose names start with
    /// <paramref name="prefix"/>. Pass <c>null</c> or an empty string to return
    /// all columns.
    /// </summary>
    public IEnumerable<Column> Resolve(Relation relation, string? prefix = null)
    {
        ArgumentNullException.ThrowIfNull(relation);

        return relation.Columns
            .Where(c => _matcher.StartsWith(c.Name, prefix));
    }

    /// <summary>
    /// Returns columns from all relations whose names start with
    /// <paramref name="prefix"/> across the entire <paramref name="context"/>.
    /// Pass <c>null</c> or an empty string to return all columns.
    /// </summary>
    public IEnumerable<Column> Resolve(DatabaseContext context, string? prefix = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Schemas
            .SelectMany(s => s.Relations)
            .SelectMany(r => r.Columns)
            .Where(c => _matcher.StartsWith(c.Name, prefix));
    }

    /// <summary>
    /// Finds a single column by its exact name within <paramref name="relation"/>.
    /// Returns <c>null</c> when no match is found.
    /// </summary>
    public Column? Find(Relation relation, string columnName)
    {
        ArgumentNullException.ThrowIfNull(relation);
        ArgumentNullException.ThrowIfNull(columnName);

        return relation.Columns
            .FirstOrDefault(c => _matcher.Equals(c.Name, columnName));
    }
}
