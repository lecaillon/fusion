namespace Fusion.Models;

/// <summary>
/// Represents the database context used for SQL resolution.
/// Contains metadata about schemas, relations, columns, and routines.
/// </summary>
public class DatabaseContext
{
    public DatabaseContext(string databaseName, IReadOnlyList<Schema> schemas)
    {
        DatabaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
        Schemas = schemas ?? throw new ArgumentNullException(nameof(schemas));
    }

    /// <summary>The name of the database.</summary>
    public string DatabaseName { get; }

    /// <summary>The schemas available in the database.</summary>
    public IReadOnlyList<Schema> Schemas { get; }
}

/// <summary>
/// Represents a database schema.
/// </summary>
public class Schema
{
    public Schema(string name, IReadOnlyList<Relation> relations, IReadOnlyList<Routine> routines)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Relations = relations ?? throw new ArgumentNullException(nameof(relations));
        Routines = routines ?? throw new ArgumentNullException(nameof(routines));
    }

    /// <summary>The schema name.</summary>
    public string Name { get; }

    /// <summary>The tables and views in this schema.</summary>
    public IReadOnlyList<Relation> Relations { get; }

    /// <summary>The stored procedures and functions in this schema.</summary>
    public IReadOnlyList<Routine> Routines { get; }
}

/// <summary>
/// Represents a database relation (table or view).
/// </summary>
public class Relation
{
    public Relation(string schema, string name, RelationKind kind, IReadOnlyList<Column> columns)
    {
        Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Kind = kind;
        Columns = columns ?? throw new ArgumentNullException(nameof(columns));
    }

    /// <summary>The schema that contains this relation.</summary>
    public string Schema { get; }

    /// <summary>The relation name.</summary>
    public string Name { get; }

    /// <summary>Whether this relation is a table or a view.</summary>
    public RelationKind Kind { get; }

    /// <summary>The columns of this relation.</summary>
    public IReadOnlyList<Column> Columns { get; }

    /// <summary>The fully qualified name of the relation (schema.name).</summary>
    public string QualifiedName => $"{Schema}.{Name}";
}

/// <summary>
/// Represents a column in a database relation.
/// </summary>
public class Column
{
    public Column(string name, string dataType, bool isNullable, int? ordinalPosition = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
        IsNullable = isNullable;
        OrdinalPosition = ordinalPosition;
    }

    /// <summary>The column name.</summary>
    public string Name { get; }

    /// <summary>The SQL data type of the column.</summary>
    public string DataType { get; }

    /// <summary>Whether the column allows NULL values.</summary>
    public bool IsNullable { get; }

    /// <summary>The 1-based ordinal position of the column in the relation, if known.</summary>
    public int? OrdinalPosition { get; }
}

/// <summary>
/// Represents a database routine (stored procedure or function).
/// </summary>
public class Routine
{
    public Routine(string schema, string name, RoutineKind kind, IReadOnlyList<RoutineParameter> parameters, string? returnType = null)
    {
        Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Kind = kind;
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ReturnType = returnType;
    }

    /// <summary>The schema that contains this routine.</summary>
    public string Schema { get; }

    /// <summary>The routine name.</summary>
    public string Name { get; }

    /// <summary>Whether this routine is a stored procedure or a function.</summary>
    public RoutineKind Kind { get; }

    /// <summary>The parameters accepted by this routine.</summary>
    public IReadOnlyList<RoutineParameter> Parameters { get; }

    /// <summary>The return type of the routine, if applicable.</summary>
    public string? ReturnType { get; }

    /// <summary>The fully qualified name of the routine (schema.name).</summary>
    public string QualifiedName => $"{Schema}.{Name}";
}

/// <summary>
/// Represents a parameter of a database routine.
/// </summary>
public class RoutineParameter
{
    public RoutineParameter(string name, string dataType, ParameterDirection direction, bool hasDefault = false)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
        Direction = direction;
        HasDefault = hasDefault;
    }

    /// <summary>The parameter name.</summary>
    public string Name { get; }

    /// <summary>The SQL data type of the parameter.</summary>
    public string DataType { get; }

    /// <summary>The direction of the parameter (input, output, or both).</summary>
    public ParameterDirection Direction { get; }

    /// <summary>Whether the parameter has a default value.</summary>
    public bool HasDefault { get; }
}

/// <summary>Specifies whether a relation is a table or a view.</summary>
public enum RelationKind
{
    Table,
    View
}

/// <summary>Specifies whether a routine is a stored procedure or a function.</summary>
public enum RoutineKind
{
    StoredProcedure,
    Function
}

/// <summary>Specifies the direction of a routine parameter.</summary>
public enum ParameterDirection
{
    Input,
    Output,
    InputOutput
}
