using RailcarTrips.Application.DTOs;

namespace RailcarTrips.Application.Abstractions.Services;

public interface ICsvEquipmentEventReader
{
    Task<IReadOnlyList<CsvEquipmentEventRowDto>> ReadAsync(Stream stream, CancellationToken cancellationToken = default);
}
