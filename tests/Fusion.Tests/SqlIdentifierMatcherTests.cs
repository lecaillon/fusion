using Fusion.Matchers;

namespace Fusion.Tests;

public class SqlIdentifierMatcherTests
{
    private readonly SqlIdentifierMatcher _sut = new();

    // ---------------------------------------------------------------------------
    // StartsWith
    // ---------------------------------------------------------------------------

    [Theory]
    [InlineData("orders",  "ord",   true)]
    [InlineData("orders",  "Orders",true)]   // case-insensitive by default
    [InlineData("orders",  "x",     false)]
    [InlineData("orders",  "",      true)]   // empty prefix matches everything
    [InlineData("orders",  null,    true)]   // null prefix matches everything
    public void StartsWith_CaseInsensitive_DefaultBehavior(string id, string? prefix, bool expected)
    {
        Assert.Equal(expected, _sut.StartsWith(id, prefix));
    }

    [Fact]
    public void StartsWith_CaseSensitive_RespectsCase()
    {
        var caseSensitive = new SqlIdentifierMatcher(caseSensitive: true);
        Assert.True(caseSensitive.StartsWith("Orders", "Ord"));
        Assert.False(caseSensitive.StartsWith("Orders", "ord"));
    }

    [Fact]
    public void StartsWith_StripsQuotesBeforeComparing()
    {
        Assert.True(_sut.StartsWith("[Order Details]", "order"));
        Assert.True(_sut.StartsWith("\"Orders\"", "ord"));
    }

    // ---------------------------------------------------------------------------
    // Contains
    // ---------------------------------------------------------------------------

    [Theory]
    [InlineData("order_items", "item",  true)]
    [InlineData("order_items", "ITEM",  true)]   // case-insensitive
    [InlineData("order_items", "xyz",   false)]
    [InlineData("order_items", "",      true)]
    [InlineData("order_items", null,    true)]
    public void Contains_DefaultBehavior(string id, string? fragment, bool expected)
    {
        Assert.Equal(expected, _sut.Contains(id, fragment));
    }

    // ---------------------------------------------------------------------------
    // Equals
    // ---------------------------------------------------------------------------

    [Theory]
    [InlineData("orders", "ORDERS", true)]   // case-insensitive default
    [InlineData("orders", "orders", true)]
    [InlineData("orders", "order",  false)]
    public void Equals_DefaultBehavior(string id, string other, bool expected)
    {
        Assert.Equal(expected, _sut.Equals(id, other));
    }

    [Fact]
    public void Equals_StripsQuotesOnBothSides()
    {
        Assert.True(_sut.Equals("[Orders]", "orders"));
        Assert.True(_sut.Equals("\"Orders\"", "[Orders]"));
    }

    // ---------------------------------------------------------------------------
    // IsValid
    // ---------------------------------------------------------------------------

    [Theory]
    [InlineData("orders",         true)]
    [InlineData("_temp",          true)]
    [InlineData("order_items_2",  true)]
    [InlineData("\"with space\"", true)]   // quoted — any content allowed
    [InlineData("[with space]",   true)]   // bracket-quoted
    [InlineData("",               false)]
    [InlineData(null,             false)]
    [InlineData("2bad",           false)]   // starts with digit
    [InlineData("has space",      false)]   // unquoted space
    [InlineData("\"\"",           false)]   // empty quoted identifier
    [InlineData("[]",             false)]   // empty bracket identifier
    public void IsValid_ReturnsExpected(string? id, bool expected)
    {
        Assert.Equal(expected, SqlIdentifierMatcher.IsValid(id));
    }

    // ---------------------------------------------------------------------------
    // StripQuotes
    // ---------------------------------------------------------------------------

    [Theory]
    [InlineData("\"orders\"",      "orders")]
    [InlineData("[Order Details]", "Order Details")]
    [InlineData("orders",          "orders")]   // no quotes — unchanged
    [InlineData("\"",              "\"")]        // single quote char — unchanged
    public void StripQuotes_ReturnsUnquotedIdentifier(string id, string expected)
    {
        Assert.Equal(expected, SqlIdentifierMatcher.StripQuotes(id));
    }

    [Fact]
    public void StripQuotes_NullArgument_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SqlIdentifierMatcher.StripQuotes(null!));
    }

    // ---------------------------------------------------------------------------
    // IsQuoted
    // ---------------------------------------------------------------------------

    [Theory]
    [InlineData("\"orders\"",  true)]
    [InlineData("[orders]",    true)]
    [InlineData("orders",      false)]
    [InlineData("\"orders",    false)]   // only opening quote
    public void IsQuoted_ReturnsExpected(string id, bool expected)
    {
        Assert.Equal(expected, SqlIdentifierMatcher.IsQuoted(id));
    }
}
