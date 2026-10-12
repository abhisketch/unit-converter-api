using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UnitConverter.Api.Models;

namespace UnitConverter.Tests;

public class ApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Convert_ValidRequest_Returns200WithResult()
    {
        var response = await _client.GetAsync("/api/convert?category=temperature&from=celsius&to=fahrenheit&value=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ConversionResponse>();
        Assert.NotNull(body);
        Assert.Equal(212, body.Result, precision: 5);
    }

    [Fact]
    public async Task Convert_UnknownUnit_Returns400()
    {
        var response = await _client.GetAsync("/api/convert?category=length&from=meter&to=parsec&value=1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Convert_MissingParameters_Returns400()
    {
        var response = await _client.GetAsync("/api/convert?category=length&from=meter");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Units_ReturnsAllCategories()
    {
        var categories = await _client.GetFromJsonAsync<List<CategoryResponse>>("/api/units");

        Assert.NotNull(categories);
        Assert.Contains(categories, c => c.Name == "length");
        Assert.Contains(categories, c => c.Name == "temperature");
        Assert.Contains(categories, c => c.Name == "mass");
    }
}
