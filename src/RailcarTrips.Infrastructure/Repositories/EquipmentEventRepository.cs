using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RailcarTrips.Application.Abstractions.Persistence;
using RailcarTrips.Application.DTOs;
using RailcarTrips.Infrastructure.Data;
using RailcarTrips.Infrastructure.Data.Entities;

namespace RailcarTrips.Infrastructure.Repositories;

public class EquipmentEventRepository : IEquipmentEventRepository
{
    private readonly AppDbContext _dbContext;

    public EquipmentEventRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AddEquipmentEventsResultDto> AddRangeIgnoringDuplicatesAsync(
        IEnumerable<PersistableEquipmentEventDto> events,
        CancellationToken cancellationToken = default)
    {
        var inserted = 0;
        var duplicates = 0;
        var affectedEquipmentIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in events)
        {
            var entity = new EquipmentEventEntity
            {
                EquipmentId = item.EquipmentId,
                CityId = item.CityId,
                Code = item.EventCode,
                EventUtcTime = item.EventUtcTime,
                EventLocalTime = item.EventLocalTime,
                NaturalKeyHash = item.NaturalKeyHash
            };

            _dbContext.EquipmentEvents.Add(entity);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                inserted++;
                affectedEquipmentIds.Add(item.EquipmentId);
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                _dbContext.Entry(entity).State = EntityState.Detached;
                duplicates++;
            }
        }

        return new AddEquipmentEventsResultDto(inserted, duplicates, affectedEquipmentIds.ToArray());
    }

    public async Task<IReadOnlyList<EquipmentEventDto>> GetByEquipmentIdAsync(
        string equipmentId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.EquipmentEvents
            .AsNoTracking()
            .Where(x => x.EquipmentId == equipmentId)
            .OrderBy(x => x.EventUtcTime)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return rows.Select(Map).ToArray();
    }

    public async Task<IReadOnlyList<EquipmentEventDto>> GetByTripIdAsync(
        long tripId,
        CancellationToken cancellationToken = default)
    {
        var trip = await _dbContext.Trips
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == tripId, cancellationToken);

        if (trip is null)
        {
            return [];
        }

        var query = _dbContext.EquipmentEvents
            .AsNoTracking()
            .Where(x => x.EquipmentId == trip.EquipmentId && x.EventUtcTime >= trip.StartUtc);

        if (trip.EndUtc.HasValue)
        {
            query = query.Where(x => x.EventUtcTime <= trip.EndUtc.Value);
        }

        var rows = await query
            .OrderBy(x => x.EventUtcTime)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return rows.Select(Map).ToArray();
    }

    private static EquipmentEventDto Map(EquipmentEventEntity entity)
    {
        return new EquipmentEventDto(
            entity.Id,
            entity.EquipmentId,
            entity.CityId,
            entity.Code,
            EnsureUtc(entity.EventUtcTime),
            entity.EventLocalTime,
            entity.NaturalKeyHash);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqliteException sqlite && sqlite.SqliteErrorCode == 19;
    }

    private static DateTime EnsureUtc(DateTime dateTime)
    {
        return dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }
}
