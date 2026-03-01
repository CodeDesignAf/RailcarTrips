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
            query = query.Where(x => x.EquipmentId == equipmentId.Trim());
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.StartUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TripDto(
                x.Id,
                x.EquipmentId,
                x.OriginCityId,
                x.DestinationCityId,
                EnsureUtc(x.StartUtc),
                x.EndUtc.HasValue ? EnsureUtc(x.EndUtc.Value) : null,
                x.TotalTripHours,
                x.Status,
                x.InvalidReason))
            .ToListAsync(cancellationToken);

        return new PagedTripsResult(items, total);
    }

    public async Task<TripDto?> GetByIdAsync(long tripId, CancellationToken cancellationToken = default)
    {
        var item = await _dbContext.Trips
            .AsNoTracking()
            .Where(x => x.Id == tripId)
            .Select(x => new TripDto(
                x.Id,
                x.EquipmentId,
                x.OriginCityId,
                x.DestinationCityId,
                EnsureUtc(x.StartUtc),
                x.EndUtc.HasValue ? EnsureUtc(x.EndUtc.Value) : null,
                x.TotalTripHours,
                x.Status,
                x.InvalidReason))
            .FirstOrDefaultAsync(cancellationToken);

        return item;
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
