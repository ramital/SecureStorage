using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureStorage.Application.CQRS.Queries;

namespace SecureStorage.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController(IMediator mediator) : Controller
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetPatients()
    {
        var userId = User.FindFirst("guid")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var query = new GetPatientsQuery(userId);
            var patients = await _mediator.Send(query);

            return Ok(new {  Patients = patients });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = $"Error retrieving patients: {ex.Message}" });
        }
    }
}