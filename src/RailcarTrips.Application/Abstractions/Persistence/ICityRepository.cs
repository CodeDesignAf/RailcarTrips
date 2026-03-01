using RailcarTrips.Application.DTOs;

namespace RailcarTrips.Application.Abstractions.Persistence;

public interface ICityRepository
{
    Task<CityDto?> GetByNameAsync(string cityName, CancellationToken cancellationToken = default);
    Task<CityDto?> GetByIdAsync(int cityId, CancellationToken cancellationToken = default);
}
