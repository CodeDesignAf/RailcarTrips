using Microsoft.AspNetCore.Mvc;
using RailcarTrips.Application.UseCases;

namespace RailcarTrips.Api.Controllers;

[ApiController]
[Route("api/trips")]
public class TripsController : ControllerBase
{
    private readonly IGetTripsQuery _getTripsQuery;
    private readonly IGetTripEventsQuery _getTripEventsQuery;

    public TripsController(
        IGetTripsQuery getTripsQuery,
        IGetTripEventsQuery getTripEventsQuery)
    {
        _getTripsQuery = getTripsQuery;
        _getTripEventsQuery = getTripEventsQuery;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? equipmentId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _getTripsQuery.ExecuteAsync(page, pageSize, equipmentId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{tripId:long}/events")]
    public async Task<IActionResult> GetTripEvents(long tripId, CancellationToken cancellationToken = default)
    {
        var events = await _getTripEventsQuery.ExecuteAsync(tripId, cancellationToken);
        return Ok(events);
    }
}
