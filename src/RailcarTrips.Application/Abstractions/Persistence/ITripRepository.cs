using RailcarTrips.Domain.Entities;
using RailcarTrips.Application.DTOs;

namespace RailcarTrips.Application.Abstractions.Persistence;

public interface ITripRepository
{
    Task AddAsync(Trip trip, CancellationToken cancellationToken = default);
    Task UpdateAsync(Trip trip, CancellationToken cancellationToken = default);
    Task ReplaceForEquipmentAsync(
        string equipmentId,
        IReadOnlyList<Trip> trips,
        CancellationToken cancellationToken = default);
    Task<PagedTripsResult> GetPagedAsync(
        int page,
        int pageSize,
        string? equipmentId,
        CancellationToken cancellationToken = default);
    Task<TripDto?> GetByIdAsync(long tripId, CancellationToken cancellationToken = default);
}
