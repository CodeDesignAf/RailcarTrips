using RailcarTrips.Domain.Entities;
using RailcarTrips.Domain.Enums;
using RailcarTrips.Domain.Services;

namespace RailcarTrips.Domain.Tests;

public class TripProcessingEngineTests
{
    private static DateTime Utc(int year, int month, int day, int hour, int minute = 0) =>
        new(year, month, day, hour, minute, 0, DateTimeKind.Utc);

    [Fact]
    public void Process_W_then_Z_creates_closed_trip_with_hours()
    {
        var engine = new TripProcessingEngine();
        var events = new[]
        {
            new EquipmentEvent(1, "EQ-1", 10, EventCode.ReleasedW, Utc(2026, 1, 1, 8)),
            new EquipmentEvent(2, "EQ-1", 20, EventCode.PlacedZ, Utc(2026, 1, 1, 12))
        };

        var result = engine.Process(events);

        Assert.Single(result.Trips);
        var trip = result.Trips[0];
        Assert.Equal(TripStatus.Closed, trip.Status);
        Assert.Equal(10, trip.OriginCityId);
        Assert.Equal(20, trip.DestinationCityId);
        Assert.Equal(Utc(2026, 1, 1, 8), trip.StartUtc);
        Assert.Equal(Utc(2026, 1, 1, 12), trip.EndUtc);
        Assert.Equal(4m, trip.TotalTripHours);
        Assert.Equal(0, result.AnomaliesCount);
    }

    [Fact]
    public void Process_out_of_order_events_sorts_by_utc()
    {
        var engine = new TripProcessingEngine();
        var events = new[]
        {
            new EquipmentEvent(2, "EQ-2", 99, EventCode.PlacedZ, Utc(2026, 1, 2, 9)),
            new EquipmentEvent(1, "EQ-2", 50, EventCode.ReleasedW, Utc(2026, 1, 2, 7))
        };

        var result = engine.Process(events);

        Assert.Single(result.Trips);
        var trip = result.Trips[0];
        Assert.Equal(TripStatus.Closed, trip.Status);
        Assert.Equal(50, trip.OriginCityId);
        Assert.Equal(99, trip.DestinationCityId);
        Assert.Equal(Utc(2026, 1, 2, 7), trip.StartUtc);
        Assert.Equal(Utc(2026, 1, 2, 9), trip.EndUtc);
        Assert.Equal(2m, trip.TotalTripHours);
        Assert.Equal(0, result.AnomaliesCount);
    }

    [Fact]
    public void Process_Z_without_W_counts_anomaly_and_creates_no_trip()
    {
        var engine = new TripProcessingEngine();
        var events = new[]
        {
            new EquipmentEvent(1, "EQ-3", 21, EventCode.PlacedZ, Utc(2026, 1, 3, 10))
        };

        var result = engine.Process(events);

        Assert.Empty(result.Trips);
        Assert.Equal(1, result.AnomaliesCount);
    }

    [Fact]
    public void Process_W_W_Z_marks_first_invalid_and_closes_second_or_behaves_per_policy()
    {
        var engine = new TripProcessingEngine();
        var events = new[]
        {
            new EquipmentEvent(1, "EQ-4", 100, EventCode.ReleasedW, Utc(2026, 1, 4, 6)),
            new EquipmentEvent(2, "EQ-4", 200, EventCode.ReleasedW, Utc(2026, 1, 4, 7)),
            new EquipmentEvent(3, "EQ-4", 300, EventCode.PlacedZ, Utc(2026, 1, 4, 10))
        };

        var result = engine.Process(events);

        Assert.Equal(2, result.Trips.Count);

        var firstTrip = result.Trips[0];
        Assert.Equal(TripStatus.Invalid, firstTrip.Status);
        Assert.Equal(100, firstTrip.OriginCityId);
        Assert.Null(firstTrip.EndUtc);

        var secondTrip = result.Trips[1];
        Assert.Equal(TripStatus.Closed, secondTrip.Status);
        Assert.Equal(200, secondTrip.OriginCityId);
        Assert.Equal(300, secondTrip.DestinationCityId);
        Assert.Equal(Utc(2026, 1, 4, 7), secondTrip.StartUtc);
        Assert.Equal(Utc(2026, 1, 4, 10), secondTrip.EndUtc);
        Assert.Equal(3m, secondTrip.TotalTripHours);
    }

    [Fact]
    public void Process_Other_event_codes_are_ignored_without_anomalies()
    {
        var engine = new TripProcessingEngine();
        var events = new[]
        {
            new EquipmentEvent(1, "EQ-6", 10, EventCode.ReleasedW, Utc(2026, 1, 6, 8)),
            new EquipmentEvent(2, "EQ-6", 11, EventCode.Other, Utc(2026, 1, 6, 9)),
            new EquipmentEvent(3, "EQ-6", 20, EventCode.PlacedZ, Utc(2026, 1, 6, 12))
        };

        var result = engine.Process(events);

        Assert.Single(result.Trips);
        var trip = result.Trips[0];
        Assert.Equal(TripStatus.Closed, trip.Status);
        Assert.Equal(10, trip.OriginCityId);
        Assert.Equal(20, trip.DestinationCityId);
        Assert.Equal(Utc(2026, 1, 6, 8), trip.StartUtc);
        Assert.Equal(Utc(2026, 1, 6, 12), trip.EndUtc);
        Assert.Equal(4m, trip.TotalTripHours);
        Assert.Equal(0, result.AnomaliesCount);
    }

    [Fact]
    public void Process_W_without_Z_leaves_open_trip()
    {
        var engine = new TripProcessingEngine();
        var events = new[]
        {
            new EquipmentEvent(1, "EQ-5", 7, EventCode.ReleasedW, Utc(2026, 1, 5, 14))
        };

        var result = engine.Process(events);

        Assert.Single(result.Trips);
        var trip = result.Trips[0];
        Assert.Equal(TripStatus.Open, trip.Status);
        Assert.Null(trip.EndUtc);
        Assert.Null(trip.DestinationCityId);
        Assert.Null(trip.TotalTripHours);
        Assert.Equal(0, result.AnomaliesCount);
    }
}
