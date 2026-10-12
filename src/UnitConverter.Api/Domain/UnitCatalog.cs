namespace UnitConverter.Api.Domain;

public interface IUnitCatalog
{
    IReadOnlyList<UnitCategory> Categories { get; }

    bool TryGetCategory(string category, out UnitCategory? result);

    bool TryGetUnit(string category, string unit, out UnitDefinition? result);
}

/// <summary>
/// Hardcoded unit data. To add a unit, add one line below.
/// To add a category, add another <see cref="UnitCategory"/>.
/// To load data from a database or file later, implement <see cref="IUnitCatalog"/> differently.
/// </summary>
public sealed class UnitCatalog : IUnitCatalog
{
    private readonly Dictionary<string, UnitCategory> _categories;
    private readonly Dictionary<string, Dictionary<string, UnitDefinition>> _unitLookup;

    public UnitCatalog()
    {
        Categories = BuildCategories();

        _categories = Categories.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
        _unitLookup = Categories.ToDictionary(
            c => c.Name,
            c => BuildUnitIndex(c),
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<UnitCategory> Categories { get; }

    public bool TryGetCategory(string category, out UnitCategory? result) =>
        _categories.TryGetValue(category.Trim(), out result);

    public bool TryGetUnit(string category, string unit, out UnitDefinition? result)
    {
        result = null;
        return _unitLookup.TryGetValue(category.Trim(), out var units)
               && units.TryGetValue(unit.Trim(), out result);
    }

    // Every name, symbol and alias points at its unit. Duplicates throw at startup (fail fast).
    private static Dictionary<string, UnitDefinition> BuildUnitIndex(UnitCategory category)
    {
        var index = new Dictionary<string, UnitDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (var unit in category.Units)
        {
            var keys = new[] { unit.Name, unit.Symbol }
                .Concat(unit.Aliases ?? Array.Empty<string>())
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var key in keys)
            {
                index.Add(key, unit);
            }
        }

        return index;
    }

    private static UnitDefinition U(
        string name, string symbol, double factor, double offset = 0, string[]? aliases = null) =>
        new(name, symbol, factor, offset, aliases);

    private static List<UnitCategory> BuildCategories() =>
    [
        new("length", "meter",
        [
            U("millimeter", "mm", 0.001, aliases: ["millimeters", "millimetre", "millimetres"]),
            U("centimeter", "cm", 0.01, aliases: ["centimeters", "centimetre", "centimetres"]),
            U("meter", "m", 1, aliases: ["meters", "metre", "metres"]),
            U("kilometer", "km", 1000, aliases: ["kilometers", "kilometre", "kilometres"]),
            U("inch", "in", 0.0254, aliases: ["inches"]),
            U("foot", "ft", 0.3048, aliases: ["feet"]),
            U("yard", "yd", 0.9144, aliases: ["yards"]),
            U("mile", "mi", 1609.344, aliases: ["miles"]),
        ]),

        new("mass", "kilogram",
        [
            U("milligram", "mg", 0.000001, aliases: ["milligrams"]),
            U("gram", "g", 0.001, aliases: ["grams"]),
            U("kilogram", "kg", 1, aliases: ["kilograms", "kilo", "kilos"]),
            U("tonne", "t", 1000, aliases: ["tonnes", "metric ton", "metric tons"]),
            U("ounce", "oz", 0.028349523125, aliases: ["ounces"]),
            U("pound", "lb", 0.45359237, aliases: ["pounds", "lbs"]),
            U("stone", "st", 6.35029318, aliases: ["stones"]),
        ]),

        // Base unit is celsius. Fahrenheit: C = (F - 32) * 5/9 = F * 5/9 - 160/9
        new("temperature", "celsius",
        [
            U("celsius", "C", 1, aliases: ["°C", "centigrade"]),
            U("fahrenheit", "F", 5.0 / 9.0, -160.0 / 9.0, aliases: ["°F"]),
            U("kelvin", "K", 1, -273.15, aliases: ["kelvins"]),
        ]),

        new("volume", "liter",
        [
            U("milliliter", "ml", 0.001, aliases: ["milliliters", "millilitre", "millilitres"]),
            U("liter", "l", 1, aliases: ["liters", "litre", "litres"]),
            U("gallon", "gal", 3.785411784, aliases: ["gallons", "us gallon"]),
            U("fluid ounce", "fl oz", 0.0295735295625, aliases: ["fluid ounces", "floz"]),
        ]),
    ];
}
