using UnitConverter.Api.Domain;
using UnitConverter.Api.Models;

namespace UnitConverter.Api.Services;

public interface IConversionService
{
    ConversionResponse Convert(string category, string from, string to, double value);
}

public sealed class ConversionService(IUnitCatalog catalog) : IConversionService
{
    private const int DecimalPlaces = 6;

    public ConversionResponse Convert(string category, string from, string to, double value)
    {
        if (!double.IsFinite(value))
        {
            throw new UnitConversionException("Value must be a finite number.");
        }

        if (!catalog.TryGetCategory(category, out var resolvedCategory) || resolvedCategory is null)
        {
            var known = string.Join(", ", catalog.Categories.Select(c => c.Name));
            throw new UnitConversionException($"Unknown category '{category}'. Supported categories: {known}.");
        }

        var source = ResolveUnit(resolvedCategory, from);
        var target = ResolveUnit(resolvedCategory, to);

        // Convert to the category's base unit, then to the target unit.
        var baseValue = value * source.ToBaseFactor + source.ToBaseOffset;
        var result = (baseValue - target.ToBaseOffset) / target.ToBaseFactor;

        if (!double.IsFinite(result))
        {
            throw new UnitConversionException("The result is out of range.");
        }

        // "+ 0.0" turns -0 into 0 so callers never see "-0".
        var rounded = Math.Round(result, DecimalPlaces, MidpointRounding.AwayFromZero) + 0.0;

        return new ConversionResponse(resolvedCategory.Name, source.Name, target.Name, value, rounded);
    }

    private UnitDefinition ResolveUnit(UnitCategory category, string unit)
    {
        if (catalog.TryGetUnit(category.Name, unit, out var definition) && definition is not null)
        {
            return definition;
        }

        var known = string.Join(", ", category.Units.Select(u => u.Name));
        throw new UnitConversionException(
            $"Unknown unit '{unit}' for category '{category.Name}'. Supported units: {known}.");
    }
}
