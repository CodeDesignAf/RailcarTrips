using RailcarTrips.Application.DTOs;

namespace RailcarTrips.Application.UseCases;

public interface IGetTripsQuery
{
    Task<TripsPageDto> ExecuteAsync(
        int page,
        int pageSize,
        string? equipmentId,
        CancellationToken cancellationToken = default);
}
