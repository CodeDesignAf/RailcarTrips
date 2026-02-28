using RailcarTrips.Domain.Entities;

namespace RailcarTrips.Domain.Services;

public class DefaultTripProcessingPolicy : ITripProcessingPolicy
{
    public Trip OnReleasedWhileOpen(Trip openTrip, EquipmentEvent releasedEvent, TripProcessingResult result)
    {
        ArgumentNullException.ThrowIfNull(openTrip);
        ArgumentNullException.ThrowIfNull(releasedEvent);
        ArgumentNullException.ThrowIfNull(result);

        openTrip.MarkInvalid("Received ReleasedW event while another trip was open.");
        result.AddAnomaly($"Equipment {releasedEvent.EquipmentId}: ReleasedW received while trip was open.");

        var newTrip = new Trip(releasedEvent.EquipmentId, releasedEvent.CityId, releasedEvent.EventUtcTime);
        result.AddTrip(newTrip);

        return newTrip;
    }

    public void OnPlacedWithoutOpen(EquipmentEvent placedEvent, TripProcessingResult result)
    {
        ArgumentNullException.ThrowIfNull(placedEvent);
        ArgumentNullException.ThrowIfNull(result);

        result.AddAnomaly($"Equipment {placedEvent.EquipmentId}: PlacedZ received without an open trip.");
    }
}
