using UnitConverter.Api.Domain;
using UnitConverter.Api.Services;

namespace UnitConverter.Tests;

public class ConversionServiceTests
{
    private readonly ConversionService _service = new(new UnitCatalog());

    [Theory]
    [InlineData("length", "meter", "foot", 1.0, 3.280840)]
    [InlineData("length", "kilometer", "mile", 1.0, 0.621371)]
    [InlineData("length", "inch", "centimeter", 1.0, 2.54)]
    [InlineData("mass", "kilogram", "pound", 1.0, 2.204623)]
    [InlineData("mass", "pound", "ounce", 1.0, 16.0)]
    [InlineData("temperature", "celsius", "fahrenheit", 100.0, 212.0)]
    [InlineData("temperature", "fahrenheit", "celsius", 32.0, 0.0)]
    [InlineData("temperature", "celsius", "kelvin", 0.0, 273.15)]
    [InlineData("temperature", "kelvin", "fahrenheit", 0.0, -459.67)]
    [InlineData("volume", "gallon", "liter", 1.0, 3.785412)]
    public void Convert_ReturnsExpectedResult(string category, string from, string to, double value, double expected)
    {
        var response = _service.Convert(category, from, to, value);

        Assert.Equal(expected, response.Result, precision: 5);
    }

    [Fact]
    public void Convert_SameUnit_ReturnsSameValue()
    {
        var response = _service.Convert("length", "meter", "meter", 42.5);

        Assert.Equal(42.5, response.Result);
    }

    [Fact]
    public void Convert_AcceptsSymbolsAliasesAndAnyCase()
    {
        var response = _service.Convert("LENGTH", "KM", "Miles", 10);

        Assert.Equal("kilometer", response.From);
        Assert.Equal("mile", response.To);
        Assert.Equal(6.213712, response.Result, precision: 5);
    }

    [Fact]
    public void Convert_NegativeZeroIsNormalised()
    {
        var response = _service.Convert("temperature", "fahrenheit", "celsius", 32);

        Assert.False(double.IsNegative(response.Result));
    }

    [Fact]
    public void Convert_UnknownCategory_Throws()
    {
        var ex = Assert.Throws<UnitConversionException>(() => _service.Convert("speed", "a", "b", 1));

        Assert.Contains("Unknown category", ex.Message);
    }

    [Fact]
    public void Convert_UnknownUnit_Throws()
    {
        var ex = Assert.Throws<UnitConversionException>(() => _service.Convert("length", "meter", "parsec", 1));

        Assert.Contains("Unknown unit", ex.Message);
    }

    [Fact]
    public void Convert_UnitFromAnotherCategory_Throws()
    {
        Assert.Throws<UnitConversionException>(() => _service.Convert("length", "kilogram", "meter", 1));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Convert_NonFiniteValue_Throws(double value)
    {
        Assert.Throws<UnitConversionException>(() => _service.Convert("length", "meter", "foot", value));
    }
}
