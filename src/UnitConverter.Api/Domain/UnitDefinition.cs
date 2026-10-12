namespace UnitConverter.Api.Domain;

/// <summary>
/// A unit expressed relative to its category's base unit:
/// <c>baseValue = value * ToBaseFactor + ToBaseOffset</c>.
/// Most units only need a factor; temperature scales also need an offset.
/// </summary>
public sealed record UnitDefinition(
    string Name,
    string Symbol,
    double ToBaseFactor,
    double ToBaseOffset = 0,
    IReadOnlyList<string>? Aliases = null);

public sealed record UnitCategory(string Name, string BaseUnit, IReadOnlyList<UnitDefinition> Units);
