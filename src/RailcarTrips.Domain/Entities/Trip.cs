using RailcarTrips.Domain.Enums;

namespace RailcarTrips.Domain.Entities;

public class Trip
{
    public long Id { get; private set; }
    public string EquipmentId { get; private set; }
    public int OriginCityId { get; private set; }
    public int? DestinationCityId { get; private set; }
    public DateTime StartUtc { get; private set; }
    public DateTime? EndUtc { get; private set; }
    public decimal? TotalTripHours { get; private set; }
    public TripStatus Status { get; private set; }
    public string? InvalidReason { get; private set; }

    public Trip(
        string equipmentId,
        int originCityId,
        DateTime startUtc,
        long id = 0)
    {
        if (string.IsNullOrWhiteSpace(equipmentId))
        {
            throw new ArgumentException("Equipment id is required.", nameof(equipmentId));
        }

        if (originCityId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(originCityId), "Origin city id must be greater than zero.");
        }

        if (startUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("StartUtc must have DateTimeKind.Utc.", nameof(startUtc));
        }

        Id = id;
        EquipmentId = equipmentId;
        OriginCityId = originCityId;
        StartUtc = startUtc;
        Status = TripStatus.Open;
    }

    public void Close(int destinationCityId, DateTime endUtc)
    {
        if (Status != TripStatus.Open)
        {
            throw new InvalidOperationException("Only open trips can be closed.");
        }

        if (destinationCityId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(destinationCityId), "Destination city id must be greater than zero.");
        }

        if (endUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("EndUtc must have DateTimeKind.Utc.", nameof(endUtc));
        }

        if (endUtc < StartUtc)
        {
            throw new ArgumentException("EndUtc cannot be earlier than StartUtc.", nameof(endUtc));
        }

        DestinationCityId = destinationCityId;
        EndUtc = endUtc;
        TotalTripHours = (decimal)(endUtc - StartUtc).TotalHours;
        Status = TripStatus.Closed;
        InvalidReason = null;
    }

    public void MarkInvalid(string? reason = null)
    {
        if (Status == TripStatus.Closed)
        {
            throw new InvalidOperationException("Closed trips cannot be marked invalid.");
        }

        Status = TripStatus.Invalid;
        InvalidReason = reason;
    }
}
