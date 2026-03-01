using Microsoft.EntityFrameworkCore;
using RailcarTrips.Application.Abstractions.Persistence;
using RailcarTrips.Application.DTOs;
using RailcarTrips.Infrastructure.Data;

namespace RailcarTrips.Infrastructure.Repositories;

public class CityRepository : ICityRepository
{
    private readonly AppDbContext _dbContext;

    public CityRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CityDto?> GetByNameAsync(string cityName, CancellationToken cancellationToken = default)
    {
        var normalized = cityName.Trim();
        var entity = await _dbContext.Cities
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == normalized, cancellationToken);

        return entity is null
            ? null
            : new CityDto(entity.Id, entity.Name, entity.TimeZoneId);
    }

    public async Task<CityDto?> GetByIdAsync(int cityId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Cities
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == cityId, cancellationToken);

        return entity is null
            ? null
            : new CityDto(entity.Id, entity.Name, entity.TimeZoneId);
    }
}
