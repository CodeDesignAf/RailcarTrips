using RailcarTrips.Domain.Entities;

namespace RailcarTrips.Domain.Services;

public class TripProcessingResult
{
    private readonly List<Trip> _trips = [];
    private readonly List<string> _anomalies = [];

    public IReadOnlyList<Trip> Trips => _trips;
    public int AnomaliesCount => _anomalies.Count;
    public IReadOnlyList<string> Anomalies => _anomalies;

    public void AddTrip(Trip trip)
    {
        ArgumentNullException.ThrowIfNull(trip);
        _trips.Add(trip);
    }

    public void AddAnomaly(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Anomaly message is required.", nameof(message));
        }

        _anomalies.Add(message);
    }
}
