# Unit Conversion API

A small ASP.NET Core (.NET 10) Web API that converts numeric values between units of measurement.

Supported categories: **length**, **temperature**, **mass** (weight), and **volume** (added as a small extra).

## Run locally

Prerequisite: [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
dotnet run --project src/UnitConverter.Api
```

The API listens on `http://localhost:5080` and your browser opens **Swagger UI** (`/swagger`) automatically, where you can try every endpoint. If it does not open, browse to `http://localhost:5080/swagger`.

Or use curl:

```bash
curl "http://localhost:5080/api/convert?category=length&from=meter&to=foot&value=10"
curl "http://localhost:5080/api/units"
```

`src/UnitConverter.Api/UnitConverter.Api.http` has ready-made requests for VS Code (REST Client) and Visual Studio.

## Run the tests

```bash
dotnet test
```

## API

### `GET /api/convert`

| Query param | Description                                          |
|-------------|------------------------------------------------------|
| `category`  | `length`, `temperature`, `mass` or `volume`          |
| `from`      | Source unit (name, symbol or alias, case-insensitive) |
| `to`        | Target unit                                          |
| `value`     | Number to convert                                    |

Example: `/api/convert?category=temperature&from=celsius&to=fahrenheit&value=100`

```json
{ "category": "temperature", "from": "celsius", "to": "fahrenheit", "value": 100, "result": 212 }
```

Invalid input (missing parameter, unknown category or unit, non-finite number) returns **400** with an RFC 7807
`ProblemDetails` body; the message lists the supported categories or units.

### `GET /api/units`

Returns every category with its base unit and units, so clients can build dropdowns.

## Project structure

```
src/UnitConverter.Api
  Controllers/   HTTP layer only - binds input, returns results
  Services/      ConversionService (the maths) + exception -> 400 mapping
  Domain/        UnitDefinition and UnitCatalog (the hardcoded unit data)
  Models/        Request / response DTOs
tests/UnitConverter.Tests
  ConversionServiceTests   unit tests for conversion logic
  ApiTests                 end-to-end tests via WebApplicationFactory
```

## Design decisions and trade-offs

- **Convert through a base unit.** Each unit stores how to reach its category's base unit
  (`base = value * factor + offset`). Any unit converts to any other in the category via the base,
  so N units need N definitions, not N x N conversion pairs. This is what lets the catalog grow to hundreds of units.
- **Factor + offset covers temperature too.** Fahrenheit and Kelvin need an offset, so temperature needs no special
  code path in the service.
- **Adding a unit is one line of data** in `UnitCatalog`. Adding a category is one more `UnitCategory`.
  Duplicate names/symbols/aliases fail at startup, not at request time.
- **`IUnitCatalog` is an interface.** Data is hardcoded today (as requested); moving it to JSON or a database later
  means a new implementation, with no change to the service or controller.
- **Category is required.** Making callers state it avoids ambiguity when future units share a symbol
  (for example `oz` for weight vs. fluid).
- **Results are rounded to 6 decimal places** to hide floating-point noise (for example `212.00000000000003`).
  Calculations use `double`; `decimal` would be the next step if exact precision were required.
- **GET with query string** keeps the endpoint easy to call and cache, since a conversion is a pure, read-only lookup.
- **Deliberately simple layout:** one API project plus one test project, no extra layers or abstractions.
  Split further only when the codebase needs it.

## Possible next steps

- Load units from a config file or database, with a cache
- API versioning (`/api/v1`) and rate limiting
- Docker image and CI pipeline (`dotnet build` + `dotnet test`)
