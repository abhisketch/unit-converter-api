using System.ComponentModel.DataAnnotations;

namespace UnitConverter.Api.Models;

/// <summary>Query-string input for GET /api/convert.</summary>
public sealed class ConvertRequest
{
    [Required] public string? Category { get; init; }

    [Required] public string? From { get; init; }

    [Required] public string? To { get; init; }

    [Required] public double? Value { get; init; }
}

public sealed record ConversionResponse(string Category, string From, string To, double Value, double Result);

public sealed record CategoryResponse(string Name, string BaseUnit, IReadOnlyList<UnitResponse> Units);

public sealed record UnitResponse(string Name, string Symbol);
