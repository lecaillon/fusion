using Fusion.Matchers;
using Fusion.Models;
using Fusion.Resolvers;

namespace Fusion.Tests;

/// <summary>
/// Shared test fixture that builds a small in-memory <see cref="DatabaseContext"/>
/// used by resolver tests.
/// </summary>
public class DatabaseContextFixture
{
    public DatabaseContextFixture()
    {
        var dboRelations = new List<Relation>
        {
            new("dbo", "Orders",      RelationKind.Table, new List<Column>
            {
                new("OrderId",   "int",      isNullable: false, ordinalPosition: 1),
                new("OrderDate", "datetime", isNullable: false, ordinalPosition: 2),
                new("CustomerId","int",      isNullable: false, ordinalPosition: 3),
            }),
            new("dbo", "OrderItems",  RelationKind.Table, new List<Column>
            {
                new("ItemId",    "int",      isNullable: false, ordinalPosition: 1),
                new("OrderId",   "int",      isNullable: false, ordinalPosition: 2),
                new("ProductId", "int",      isNullable: false, ordinalPosition: 3),
                new("Quantity",  "int",      isNullable: false, ordinalPosition: 4),
            }),
            new("dbo", "Customers",   RelationKind.Table, new List<Column>
            {
                new("CustomerId","int",      isNullable: false, ordinalPosition: 1),
                new("Name",      "nvarchar", isNullable: false, ordinalPosition: 2),
                new("Email",     "nvarchar", isNullable: true,  ordinalPosition: 3),
            }),
            new("dbo", "OrderSummary",RelationKind.View,  new List<Column>
            {
                new("OrderId",   "int",      isNullable: false, ordinalPosition: 1),
                new("Total",     "decimal",  isNullable: false, ordinalPosition: 2),
            }),
        };

        var dboRoutines = new List<Routine>
        {
            new("dbo", "GetOrderById",    RoutineKind.StoredProcedure, new List<RoutineParameter>
            {
                new("@OrderId", "int", ParameterDirection.Input),
            }),
            new("dbo", "GetCustomers",    RoutineKind.StoredProcedure, new List<RoutineParameter>()),
            new("dbo", "CalculateTotal",  RoutineKind.Function, new List<RoutineParameter>
            {
                new("@OrderId", "int", ParameterDirection.Input),
            }, returnType: "decimal"),
        };

        var salesRelations = new List<Relation>
        {
            new("sales", "Invoices", RelationKind.Table, new List<Column>
            {
                new("InvoiceId", "int",      isNullable: false, ordinalPosition: 1),
                new("Amount",    "decimal",  isNullable: false, ordinalPosition: 2),
            }),
        };

        var salesRoutines = new List<Routine>
        {
            new("sales", "GenerateInvoice", RoutineKind.StoredProcedure, new List<RoutineParameter>()),
        };

        Context = new DatabaseContext("TestDb", new List<Schema>
        {
            new("dbo",   dboRelations,   dboRoutines),
            new("sales", salesRelations, salesRoutines),
        });
    }

    public DatabaseContext Context { get; }
}

public class RelationResolverTests : IClassFixture<DatabaseContextFixture>
{
    private readonly DatabaseContext _ctx;
    private readonly RelationResolver _sut = new();

    public RelationResolverTests(DatabaseContextFixture fixture)
    {
        _ctx = fixture.Context;
    }

    [Fact]
    public void Resolve_NullPrefix_ReturnsAllRelations()
    {
        var result = _sut.Resolve(_ctx).ToList();
        Assert.Equal(5, result.Count); // 4 dbo + 1 sales
    }

    [Fact]
    public void Resolve_Prefix_FiltersCorrectly()
    {
        var result = _sut.Resolve(_ctx, "Order").ToList();
        Assert.Equal(3, result.Count); // Orders, OrderItems, OrderSummary
        Assert.All(result, r => Assert.StartsWith("Order", r.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Resolve_WithSchema_FiltersToSchema()
    {
        // Use the 3-arg overload: (context, schemaName, prefix)
        var result = _sut.Resolve(_ctx, "sales", null).ToList();
        Assert.Single(result);
        Assert.Equal("Invoices", result[0].Name);
    }

    [Fact]
    public void Resolve_WithSchemaAndPrefix_FiltersCorrectly()
    {
        var result = _sut.Resolve(_ctx, "dbo", "Cust").ToList();
        Assert.Single(result);
        Assert.Equal("Customers", result[0].Name);
    }

    [Fact]
    public void Resolve_NullContext_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Resolve(null!).ToList());
    }

    [Fact]
    public void Find_ByName_ReturnsRelation()
    {
        var result = _sut.Find(_ctx, "orders");
        Assert.NotNull(result);
        Assert.Equal("Orders", result.Name);
    }

    [Fact]
    public void Find_ByNameAndSchema_ReturnsRelation()
    {
        var result = _sut.Find(_ctx, "Invoices", "sales");
        Assert.NotNull(result);
        Assert.Equal("sales", result.Schema);
    }

    [Fact]
    public void Find_NotFound_ReturnsNull()
    {
        Assert.Null(_sut.Find(_ctx, "NonExistent"));
    }

    [Fact]
    public void Resolve_UsesSharedMatcher()
    {
        var matcher = new SqlIdentifierMatcher(caseSensitive: true);
        var caseSensitiveResolver = new RelationResolver(matcher);

        // With case-sensitive matching "order" should NOT match "Orders"
        var result = caseSensitiveResolver.Resolve(_ctx, "order").ToList();
        Assert.Empty(result);

        // But "Order" should
        result = caseSensitiveResolver.Resolve(_ctx, "Order").ToList();
        Assert.NotEmpty(result);
    }
}

public class ColumnResolverTests : IClassFixture<DatabaseContextFixture>
{
    private readonly DatabaseContext _ctx;
    private readonly ColumnResolver _sut = new();

    public ColumnResolverTests(DatabaseContextFixture fixture)
    {
        _ctx = fixture.Context;
    }

    [Fact]
    public void Resolve_FromRelation_NullPrefix_ReturnsAllColumns()
    {
        var orders = _ctx.Schemas[0].Relations.First(r => r.Name == "Orders");
        var result = _sut.Resolve(orders).ToList();
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Resolve_FromRelation_WithPrefix_FiltersColumns()
    {
        var orders = _ctx.Schemas[0].Relations.First(r => r.Name == "Orders");
        // "Order" matches both "OrderId" and "OrderDate"
        var result = _sut.Resolve(orders, "Order").ToList();
        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.StartsWith("Order", c.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Resolve_FromContext_ReturnsAllColumns()
    {
        var result = _sut.Resolve(_ctx).ToList();
        // Orders(3) + OrderItems(4) + Customers(3) + OrderSummary(2) + Invoices(2)
        Assert.Equal(14, result.Count);
    }

    [Fact]
    public void Resolve_NullRelation_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Resolve((Relation)null!).ToList());
    }

    [Fact]
    public void Find_ByName_ReturnsColumn()
    {
        var orders = _ctx.Schemas[0].Relations.First(r => r.Name == "Orders");
        var result = _sut.Find(orders, "orderid");
        Assert.NotNull(result);
        Assert.Equal("OrderId", result.Name);
    }

    [Fact]
    public void Find_NotFound_ReturnsNull()
    {
        var orders = _ctx.Schemas[0].Relations.First(r => r.Name == "Orders");
        Assert.Null(_sut.Find(orders, "NonExistent"));
    }
}

public class RoutineResolverTests : IClassFixture<DatabaseContextFixture>
{
    private readonly DatabaseContext _ctx;
    private readonly RoutineResolver _sut = new();

    public RoutineResolverTests(DatabaseContextFixture fixture)
    {
        _ctx = fixture.Context;
    }

    [Fact]
    public void Resolve_NullPrefix_ReturnsAllRoutines()
    {
        var result = _sut.Resolve(_ctx).ToList();
        Assert.Equal(4, result.Count); // 3 dbo + 1 sales
    }

    [Fact]
    public void Resolve_WithPrefix_FiltersCorrectly()
    {
        var result = _sut.Resolve(_ctx, "Get").ToList();
        Assert.Equal(2, result.Count); // GetOrderById, GetCustomers
        Assert.All(result, r => Assert.StartsWith("Get", r.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Resolve_WithSchemaAndPrefix_FiltersCorrectly()
    {
        var result = _sut.Resolve(_ctx, "dbo", "Get").ToList();
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.Equal("dbo", r.Schema));
    }

    [Fact]
    public void ResolveStoredProcedures_ReturnsOnlyProcedures()
    {
        var result = _sut.ResolveStoredProcedures(_ctx).ToList();
        Assert.Equal(3, result.Count); // GetOrderById, GetCustomers, GenerateInvoice
        Assert.All(result, r => Assert.Equal(RoutineKind.StoredProcedure, r.Kind));
    }

    [Fact]
    public void ResolveFunctions_ReturnsOnlyFunctions()
    {
        var result = _sut.ResolveFunctions(_ctx).ToList();
        Assert.Single(result);
        Assert.Equal("CalculateTotal", result[0].Name);
        Assert.Equal(RoutineKind.Function, result[0].Kind);
    }

    [Fact]
    public void Find_ByName_ReturnsRoutine()
    {
        var result = _sut.Find(_ctx, "getorderbyid");
        Assert.NotNull(result);
        Assert.Equal("GetOrderById", result.Name);
    }

    [Fact]
    public void Find_ByNameAndSchema_ReturnsRoutine()
    {
        var result = _sut.Find(_ctx, "GenerateInvoice", "sales");
        Assert.NotNull(result);
        Assert.Equal("sales", result.Schema);
    }

    [Fact]
    public void Find_NotFound_ReturnsNull()
    {
        Assert.Null(_sut.Find(_ctx, "NonExistent"));
    }

    [Fact]
    public void Resolve_NullContext_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Resolve(null!).ToList());
    }
}
