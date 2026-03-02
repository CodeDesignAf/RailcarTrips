using Microsoft.EntityFrameworkCore;
using RailcarTrips.Application.Abstractions.Persistence;
using RailcarTrips.Application.DTOs;
using RailcarTrips.Domain.Entities;
using RailcarTrips.Infrastructure.Data;
using RailcarTrips.Infrastructure.Data.Entities;

namespace RailcarTrips.Infrastructure.Repositories;

public class TripRepository : ITripRepository
{
    private readonly AppDbContext _dbContext;

    public TripRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Trip trip, CancellationToken cancellationToken = default)
    {
        _dbContext.Trips.Add(Map(trip));
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Trip trip, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Trips
            .FirstOrDefaultAsync(x =>
                x.EquipmentId == trip.EquipmentId &&
                x.StartUtc == trip.StartUtc,
                cancellationToken);

        if (existing is null)
        {
            _dbContext.Trips.Add(Map(trip));
        }
        else
        {
            existing.OriginCityId = trip.OriginCityId;
            existing.DestinationCityId = trip.DestinationCityId;
            existing.EndUtc = trip.EndUtc;
            existing.TotalTripHours = trip.TotalTripHours;
            existing.Status = trip.Status.ToString();
            existing.InvalidReason = trip.InvalidReason;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplaceForEquipmentAsync(
        string equipmentId,
        IReadOnlyList<Trip> trips,
        CancellationToken cancellationToken = default)
    {
        var existingTrips = await _dbContext.Trips
            .Where(x => x.EquipmentId == equipmentId)
            .ToListAsync(cancellationToken);

        if (existingTrips.Count > 0)
        {
            _dbContext.Trips.RemoveRange(existingTrips);
        }

        _dbContext.Trips.AddRange(trips.Select(Map));
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedTripsResult> GetPagedAsync(
        int page,
        int pageSize,
        string? equipmentId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Trips.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(equipmentId))
        {
            var normalizedFilter = equipmentId.Trim();
            query = query.Where(x => EF.Functions.Like(x.EquipmentId, $"%{normalizedFilter}%"));
        }

        var total = await query.CountAsync(cancellationToken);
        var rawTrips = await query
            .OrderByDescending(x => x.StartUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var cityIds = rawTrips
            .SelectMany(trip => trip.DestinationCityId.HasValue
                ? new[] { trip.OriginCityId, trip.DestinationCityId.Value }
                : new[] { trip.OriginCityId })
            .Distinct()
            .ToArray();

        var cityLookup = await _dbContext.Cities
            .AsNoTracking()
            .Where(city => cityIds.Contains(city.Id))
            .ToDictionaryAsync(city => city.Id, city => city.Name, cancellationToken);

        var items = rawTrips
            .Select(trip => new TripDto(
                trip.Id,
                trip.EquipmentId,
                trip.OriginCityId,
                ResolveCityName(trip.OriginCityId, cityLookup),
                trip.DestinationCityId,
                trip.DestinationCityId.HasValue
                    ? ResolveCityName(trip.DestinationCityId.Value, cityLookup)
                    : null,
                EnsureUtc(trip.StartUtc),
                trip.EndUtc.HasValue ? EnsureUtc(trip.EndUtc.Value) : null,
                trip.TotalTripHours,
                trip.Status,
                trip.InvalidReason))
            .ToList();

        return new PagedTripsResult(items, total);
    }

    public async Task<TripDto?> GetByIdAsync(long tripId, CancellationToken cancellationToken = default)
    {
        var rawTrip = await _dbContext.Trips
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == tripId, cancellationToken);

        if (rawTrip is null)
        {
            return null;
        }

        var cityIds = rawTrip.DestinationCityId.HasValue
            ? new[] { rawTrip.OriginCityId, rawTrip.DestinationCityId.Value }
            : new[] { rawTrip.OriginCityId };

        var cityLookup = await _dbContext.Cities
            .AsNoTracking()
            .Where(city => cityIds.Contains(city.Id))
            .ToDictionaryAsync(city => city.Id, city => city.Name, cancellationToken);

        var item = new TripDto(
            rawTrip.Id,
            rawTrip.EquipmentId,
            rawTrip.OriginCityId,
            ResolveCityName(rawTrip.OriginCityId, cityLookup),
            rawTrip.DestinationCityId,
            rawTrip.DestinationCityId.HasValue
                ? ResolveCityName(rawTrip.DestinationCityId.Value, cityLookup)
                : null,
            EnsureUtc(rawTrip.StartUtc),
            rawTrip.EndUtc.HasValue ? EnsureUtc(rawTrip.EndUtc.Value) : null,
            rawTrip.TotalTripHours,
            rawTrip.Status,
            rawTrip.InvalidReason);

        return item;
    }

    private static string ResolveCityName(int cityId, IReadOnlyDictionary<int, string> cityLookup)
    {
        return cityLookup.TryGetValue(cityId, out var cityName)
            ? cityName
            : $"City #{cityId}";
    }

    private static TripEntity Map(Trip trip)
    {
        return new TripEntity
        {
            EquipmentId = trip.EquipmentId,
            OriginCityId = trip.OriginCityId,
            DestinationCityId = trip.DestinationCityId,
            StartUtc = trip.StartUtc,
            EndUtc = trip.EndUtc,
            TotalTripHours = trip.TotalTripHours,
            Status = trip.Status.ToString(),
            InvalidReason = trip.InvalidReason
        };
    }

    private static DateTime EnsureUtc(DateTime dateTime)
    {
        return dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }
}
