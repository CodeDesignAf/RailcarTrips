namespace RailcarTrips.Infrastructure.Data.Entities;

public class TripEntity
{
    public long Id { get; set; }
    public string EquipmentId { get; set; } = string.Empty;
    public int OriginCityId { get; set; }
    public int? DestinationCityId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }
    public decimal? TotalTripHours { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? InvalidReason { get; set; }
}
