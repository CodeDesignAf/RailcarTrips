using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using RailcarTrips.Client.Models;

namespace RailcarTrips.Client.Services;

public sealed class RailcarTripsApiClient
{
    private readonly HttpClient _httpClient;

    public RailcarTripsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ImportResultDto> ImportEquipmentEventsAsync(IBrowserFile file, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(file);

        await using var fileStream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024, cancellationToken: ct);
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);

        var mediaType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType;
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        content.Add(fileContent, "File", file.Name);

        using var response = await _httpClient.PostAsync("api/import/equipment-events", content, ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await BuildErrorMessageAsync(response, ct));
        }

        var result = await response.Content.ReadFromJsonAsync<ImportResultDto>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("Import response was empty.");
    }

    public async Task<PagedResult<TripGridRowDto>> GetTripsAsync(
        int page,
        int pageSize,
        string? equipmentId,
        CancellationToken ct)
    {
        var encodedEquipmentId = string.IsNullOrWhiteSpace(equipmentId)
            ? string.Empty
            : $"&equipmentId={Uri.EscapeDataString(equipmentId.Trim())}";

        var requestUri = $"api/trips?page={page}&pageSize={pageSize}{encodedEquipmentId}";
        using var response = await _httpClient.GetAsync(requestUri, ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await BuildErrorMessageAsync(response, ct));
        }

        var apiPage = await response.Content.ReadFromJsonAsync<PagedResult<ApiTripDto>>(cancellationToken: ct);
        if (apiPage is null)
        {
            throw new InvalidOperationException("Trips response was empty.");
        }

        var rows = apiPage.Items
            .Select(item => new TripGridRowDto(
                item.Id,
                item.EquipmentId,
                ResolveCityLabel(item.OriginCityName, item.OriginCityId),
                item.DestinationCityId is null
                    ? "-"
                    : ResolveCityLabel(item.DestinationCityName, item.DestinationCityId.Value),
                item.StartUtc,
                item.EndUtc,
                item.TotalTripHours))
            .ToArray();

        return new PagedResult<TripGridRowDto>(apiPage.Page, apiPage.PageSize, apiPage.TotalCount, rows);
    }

    public async Task<IReadOnlyList<EquipmentEventDto>> GetTripEventsAsync(long tripId, CancellationToken ct)
    {
        using var response = await _httpClient.GetAsync($"api/trips/{tripId}/events", ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await BuildErrorMessageAsync(response, ct));
        }

        var result = await response.Content.ReadFromJsonAsync<IReadOnlyList<EquipmentEventDto>>(cancellationToken: ct);
        return result ?? Array.Empty<EquipmentEventDto>();
    }

    private static async Task<string> BuildErrorMessageAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(body))
        {
            return $"Request failed with status {(int)response.StatusCode} ({response.StatusCode}).";
        }

        try
        {
            var problem = JsonSerializer.Deserialize<ProblemDetailsLike>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (!string.IsNullOrWhiteSpace(problem?.Detail))
            {
                return problem.Detail!;
            }

            if (!string.IsNullOrWhiteSpace(problem?.Title))
            {
                return problem.Title!;
            }
        }
        catch (JsonException)
        {
            // Not a problem-details payload, return plain body below.
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return body;
        }

        return $"Request failed with status {(int)response.StatusCode} ({response.StatusCode}): {body}";
    }

    private static string ResolveCityLabel(string? cityName, int cityId) =>
        string.IsNullOrWhiteSpace(cityName) ? $"City #{cityId}" : cityName;

    private sealed record ApiTripDto(
        long Id,
        string EquipmentId,
        int OriginCityId,
        string OriginCityName,
        int? DestinationCityId,
        string? DestinationCityName,
        DateTime StartUtc,
        DateTime? EndUtc,
        decimal? TotalTripHours,
        string Status,
        string? InvalidReason);

    private sealed record ProblemDetailsLike(string? Title, string? Detail);
}
