using WordToExcel.App.Conversion;
using WordToExcel.App.Model;
using Xunit;

namespace WordToExcel.Tests.Unit;

public sealed class ValuePolicyTests
{
    [Fact]
    public void LeadingZeroIdentifierStaysExactText()
    {
        var result = new ValuePolicy().Plan("001234");

        Assert.Equal(ValueMode.Text, result.Mode);
        Assert.Equal("001234", result.ExpectedSourceText);
        Assert.Equal("001234", result.ExcelValue);
    }

    [Fact]
    public void MoreThanFifteenDigitsStayExactText()
    {
        var result = new ValuePolicy().Plan("1234567890123456");

        Assert.Equal(ValueMode.Text, result.Mode);
        Assert.Equal("1234567890123456", result.ExcelValue);
    }

    [Theory]
    [InlineData("=1+1")]
    [InlineData("+123")]
    [InlineData("-123")]
    [InlineData("@A1")]
    public void FormulaLikePrefixesStayText(string source)
    {
        var result = new ValuePolicy().Plan(source);

        Assert.Equal(ValueMode.Text, result.Mode);
        Assert.Equal(source, result.ExcelValue);
    }

    [Fact]
    public void SimplePositiveIntegerBecomesSafeNumber()
    {
        var result = new ValuePolicy().Plan("123456789012345");

        Assert.Equal(ValueMode.SafeNumber, result.Mode);
        Assert.Equal("123456789012345", result.ExpectedSourceText);
        Assert.Equal(123456789012345L, result.ExcelValue);
    }

    [Fact]
    public void StrictIsoDateBecomesSafeDate()
    {
        var result = new ValuePolicy().Plan("2026-09-11");

        Assert.Equal(ValueMode.SafeDate, result.Mode);
        Assert.Equal(new DateTime(2026, 9, 11), result.ExcelValue);
        Assert.Equal("yyyy-mm-dd", result.NumberFormat);
    }

    [Theory]
    [InlineData("1.25")]
    [InlineData("1,25")]
    [InlineData("09/11/2026")]
    public void LocaleOrMeaningAmbiguousValuesStayText(string source)
    {
        var result = new ValuePolicy().Plan(source);

        Assert.Equal(ValueMode.Text, result.Mode);
        Assert.Equal(source, result.ExcelValue);
    }

    [Fact]
    public void SignificantWhitespaceAndLineBreaksStayExactText()
    {
        const string source = "  код\nстрока  ";

        var result = new ValuePolicy().Plan(source);

        Assert.Equal(ValueMode.Text, result.Mode);
        Assert.Equal(source, result.ExpectedSourceText);
        Assert.Equal(source, result.ExcelValue);
    }
}
