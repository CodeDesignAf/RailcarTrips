namespace RailcarTrips.Client.Models;

public sealed record ImportResultDto(
    int Imported,
    int Duplicates,
    int TripsCreated,
    int TripsClosed,
    int IgnoredEventsCount,
    IReadOnlyDictionary<string, int>? IgnoredEventCodesSummary,
    int AnomaliesCount,
    IReadOnlyList<string>? Anomalies);
