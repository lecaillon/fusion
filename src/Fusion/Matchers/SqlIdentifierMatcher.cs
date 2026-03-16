namespace Fusion.Matchers;

/// <summary>
/// Matches SQL identifiers using prefix or substring comparison with support for
/// quoted identifiers and configurable case sensitivity.
/// Designed to be reused across all resolver classes.
/// </summary>
public class SqlIdentifierMatcher
{
    private readonly StringComparison _comparison;

    /// <summary>
    /// Initializes a new instance of <see cref="SqlIdentifierMatcher"/>.
    /// </summary>
    /// <param name="caseSensitive">
    /// When <c>true</c>, identifier matching is case-sensitive.
    /// SQL Server and PostgreSQL default to case-insensitive comparisons for unquoted identifiers.
    /// Defaults to <c>false</c>.
    /// </param>
    public SqlIdentifierMatcher(bool caseSensitive = false)
    {
        _comparison = caseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="identifier"/> starts with
    /// the given <paramref name="prefix"/> (after stripping any surrounding quotes).
    /// An empty or null <paramref name="prefix"/> matches every identifier.
    /// </summary>
    public bool StartsWith(string identifier, string? prefix)
    {
        if (string.IsNullOrEmpty(prefix))
            return true;

        var stripped = StripQuotes(identifier);
        var strippedPrefix = StripQuotes(prefix);

        return stripped.StartsWith(strippedPrefix, _comparison);
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="identifier"/> contains
    /// the given <paramref name="fragment"/> (after stripping any surrounding quotes).
    /// An empty or null <paramref name="fragment"/> matches every identifier.
    /// </summary>
    public bool Contains(string identifier, string? fragment)
    {
        if (string.IsNullOrEmpty(fragment))
            return true;

        var stripped = StripQuotes(identifier);
        var strippedFragment = StripQuotes(fragment);

        return stripped.Contains(strippedFragment, _comparison);
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="identifier"/> exactly matches
    /// <paramref name="other"/> (after stripping any surrounding quotes).
    /// </summary>
    public bool Equals(string identifier, string other)
    {
        var stripped = StripQuotes(identifier);
        var strippedOther = StripQuotes(other);

        return stripped.Equals(strippedOther, _comparison);
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="identifier"/> is a valid SQL identifier:
    /// non-empty, starts with a letter or underscore, and contains only letters,
    /// digits, underscores, or dollar signs — or is wrapped in double-quotes or brackets
    /// (allowing any content inside the quotes).
    /// </summary>
    public static bool IsValid(string? identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return false;

        if (IsQuoted(identifier))
            return identifier.Length > 2; // at least one character inside the quotes

        // Unquoted identifier: must start with letter or underscore,
        // followed by letters, digits, underscores, or dollar signs.
        if (!char.IsLetter(identifier[0]) && identifier[0] != '_')
            return false;

        for (int i = 1; i < identifier.Length; i++)
        {
            char c = identifier[i];
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '$')
                return false;
        }

        return true;
    }

    /// <summary>
    /// Removes surrounding double-quotes (<c>"…"</c>) or brackets (<c>[…]</c>)
    /// from a SQL identifier, if present. Otherwise returns the identifier unchanged.
    /// </summary>
    public static string StripQuotes(string identifier)
    {
        if (identifier is null) throw new ArgumentNullException(nameof(identifier));

        if (identifier.Length >= 2)
        {
            if (identifier[0] == '"' && identifier[^1] == '"')
                return identifier[1..^1];

            if (identifier[0] == '[' && identifier[^1] == ']')
                return identifier[1..^1];
        }

        return identifier;
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="identifier"/> is wrapped in
    /// double-quotes or brackets.
    /// </summary>
    public static bool IsQuoted(string identifier)
    {
        if (identifier is null) throw new ArgumentNullException(nameof(identifier));

        return (identifier.StartsWith('"') && identifier.EndsWith('"'))
            || (identifier.StartsWith('[') && identifier.EndsWith(']'));
    }
}
