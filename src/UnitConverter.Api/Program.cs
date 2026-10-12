using UnitConverter.Api.Domain;
using UnitConverter.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<UnitConversionExceptionHandler>();

// The catalog is immutable reference data, so a singleton is safe.
builder.Services.AddSingleton<IUnitCatalog, UnitCatalog>();
builder.Services.AddScoped<IConversionService, ConversionService>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(); // Swagger UI at /swagger

// Opening the root URL lands on Swagger UI.
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapControllers();

app.Run();

// Exposed so integration tests can use WebApplicationFactory<Program>.
public partial class Program;
