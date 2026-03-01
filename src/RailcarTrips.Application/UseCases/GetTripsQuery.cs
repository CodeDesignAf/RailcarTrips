using RailcarTrips.Application.Abstractions.Persistence;
using RailcarTrips.Application.DTOs;

namespace RailcarTrips.Application.UseCases;

public class GetTripsQuery : IGetTripsQuery
{
    private readonly ITripRepository _tripRepository;

    public GetTripsQuery(ITripRepository tripRepository)
    {
        _tripRepository = tripRepository;
    }

    public async Task<TripsPageDto> ExecuteAsync(
        int page,
        int pageSize,
        string? equipmentId,
        CancellationToken cancellationToken = default)
    {
        var safePage = page <= 0 ? 1 : page;
        var safePageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

        var result = await _tripRepository.GetPagedAsync(
            safePage,
            safePageSize,
            equipmentId,
            cancellationToken);

        return new TripsPageDto(
            safePage,
            safePageSize,
            result.TotalCount,
            result.Items);
    }
}
