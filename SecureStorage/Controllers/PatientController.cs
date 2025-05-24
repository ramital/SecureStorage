using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureStorage.CQRS.Queries;
using SecureStorage.Models;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SecureStorage.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientController(IMediator mediator) : Controller
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

            return Ok(new {  Patients = patients.OrderBy(q => q).ToList() });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = $"Error retrieving patients: {ex.Message}" });
        }
    }

}