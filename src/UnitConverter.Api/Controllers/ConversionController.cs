using Microsoft.AspNetCore.Mvc;
using UnitConverter.Api.Domain;
using UnitConverter.Api.Models;
using UnitConverter.Api.Services;

namespace UnitConverter.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class ConversionController(IConversionService conversionService, IUnitCatalog catalog) : ControllerBase
{
    /// <summary>Lists every supported category and its units.</summary>
    [HttpGet("units")]
    [ProducesResponseType<IEnumerable<CategoryResponse>>(StatusCodes.Status200OK)]
    public IActionResult GetUnits()
    {
        var response = catalog.Categories.Select(c => new CategoryResponse(
            c.Name,
            c.BaseUnit,
            c.Units.Select(u => new UnitResponse(u.Name, u.Symbol)).ToList()));

        return Ok(response);
    }

    /// <summary>
    /// Converts a value from one unit to another within a category.
    /// Example: /api/convert?category=length&amp;from=meter&amp;to=foot&amp;value=10
    /// </summary>
    [HttpGet("convert")]
    [ProducesResponseType<ConversionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public IActionResult Convert([FromQuery] ConvertRequest request)
    {
        var result = conversionService.Convert(
            request.Category!, request.From!, request.To!, request.Value!.Value);

        return Ok(result);
    }
}
