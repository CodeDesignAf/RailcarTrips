using RailcarTrips.Domain.Enums;

namespace RailcarTrips.Domain.Entities;

public class EquipmentEvent
{
    public long Id { get; }
    public string EquipmentId { get; }
    public int CityId { get; }
    public EventCode Code { get; }
    public DateTime EventUtcTime { get; }
    public DateTime? EventLocalTime { get; }

    public EquipmentEvent(
        long id,
        string equipmentId,
        int cityId,
        EventCode code,
        DateTime eventUtcTime,
        DateTime? eventLocalTime = null)
    {
        if (string.IsNullOrWhiteSpace(equipmentId))
        {
            throw new ArgumentException("Equipment id is required.", nameof(equipmentId));
        }

        if (cityId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cityId), "City id must be greater than zero.");
        }

        if (eventUtcTime.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("EventUtcTime must have DateTimeKind.Utc.", nameof(eventUtcTime));
        }

        Id = id;
        EquipmentId = equipmentId;
        CityId = cityId;
        Code = code;
        EventUtcTime = eventUtcTime;
        EventLocalTime = eventLocalTime;
    }
}
