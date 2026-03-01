namespace RailcarTrips.Infrastructure.Data.Entities;

public class EquipmentEventEntity
{
    public long Id { get; set; }
    public string EquipmentId { get; set; } = string.Empty;
    public int CityId { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime EventUtcTime { get; set; }
    public DateTime? EventLocalTime { get; set; }
    public string NaturalKeyHash { get; set; } = string.Empty;
}
