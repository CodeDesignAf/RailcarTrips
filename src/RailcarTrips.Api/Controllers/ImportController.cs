using Microsoft.AspNetCore.Mvc;
using RailcarTrips.Application.DTOs;
using RailcarTrips.Application.UseCases;

namespace RailcarTrips.Api.Controllers;

[ApiController]
[Route("api/import")]
public class ImportController : ControllerBase
{
    private readonly ImportEquipmentEventsUseCase _useCase;

    public ImportController(ImportEquipmentEventsUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpPost("equipment-events")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImportResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ImportEquipmentEvents([FromForm] ImportEquipmentEventsRequest request, CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest("CSV file is required.");
        }

        try
        {
            await using var stream = request.File.OpenReadStream();
            var result = await _useCase.ExecuteAsync(stream, cancellationToken);
            return Ok(result);
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public class ImportEquipmentEventsRequest
{
    public IFormFile? File { get; set; }
}
