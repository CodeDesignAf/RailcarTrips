using System.Net;
using System.Net.Http.Json;
using System.Text;
using RailcarTrips.Application.DTOs;

namespace RailcarTrips.Api.IntegrationTests;

public class ImportAndTripsIntegrationTests : IClassFixture<RailcarTripsApiFactory>
{
    private readonly HttpClient _client;

    public ImportAndTripsIntegrationTests(RailcarTripsApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Import_then_get_trips_returns_at_least_one_trip()
    {
        const string csv = """
Equipment Id,Event Code,Event Time,City Id
EQ-100,W,2026-02-01 08:00:00,1
EQ-100,Z,2026-02-01 12:00:00,2
""";

        using var form = new MultipartFormDataContent();
        using var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        using var fileContent = new StreamContent(csvStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        form.Add(fileContent, "file", "events.csv");

        var importResponse = await _client.PostAsync("/api/import/equipment-events", form);
        Assert.Equal(HttpStatusCode.OK, importResponse.StatusCode);

        var importResult = await importResponse.Content.ReadFromJsonAsync<ImportResultDto>();
        Assert.NotNull(importResult);
        Assert.True(importResult.Imported + importResult.Duplicates >= 2);

        var tripsResponse = await _client.GetAsync("/api/trips?page=1&pageSize=10&equipmentId=EQ-100");
        Assert.Equal(HttpStatusCode.OK, tripsResponse.StatusCode);

        var trips = await tripsResponse.Content.ReadFromJsonAsync<TripsPageDto>();
        Assert.NotNull(trips);
        Assert.True(trips.TotalCount >= 1);
        Assert.NotEmpty(trips.Items);
    }
}
