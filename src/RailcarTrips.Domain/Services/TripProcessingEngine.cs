using RailcarTrips.Domain.Entities;
using RailcarTrips.Domain.Enums;

namespace RailcarTrips.Domain.Services;

public class TripProcessingEngine
{
    private readonly ITripProcessingPolicy _policy;

    public TripProcessingEngine(ITripProcessingPolicy? policy = null)
    {
        _policy = policy ?? new DefaultTripProcessingPolicy();
    }

    public TripProcessingResult Process(IEnumerable<EquipmentEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        var result = new TripProcessingResult();

        var groupedEvents = events
            .GroupBy(e => e.EquipmentId)
            .Select(group => group.OrderBy(e => e.EventUtcTime).ThenBy(e => e.Id));

        foreach (var equipmentEvents in groupedEvents)
        {
            Trip? openTrip = null;

            foreach (var equipmentEvent in equipmentEvents)
            {
                switch (equipmentEvent.Code)
                {
                    case EventCode.ReleasedW:
                        if (openTrip is null)
                        {
                            openTrip = new Trip(
                                equipmentEvent.EquipmentId,
                                equipmentEvent.CityId,
                                equipmentEvent.EventUtcTime);
                            result.AddTrip(openTrip);
                        }
                        else
                        {
                            openTrip = _policy.OnReleasedWhileOpen(openTrip, equipmentEvent, result);
                        }
                        break;

                    case EventCode.PlacedZ:
                        if (openTrip is null)
                        {
                            _policy.OnPlacedWithoutOpen(equipmentEvent, result);
                        }
                        else
                        {
                            openTrip.Close(equipmentEvent.CityId, equipmentEvent.EventUtcTime);
                            openTrip = null;
                        }
                        break;

                    default:
                        result.AddAnomaly(
                            $"Equipment {equipmentEvent.EquipmentId}: Unsupported event code {equipmentEvent.Code}.");
                        break;
                }
            }
        }

        return result;
    }
}
