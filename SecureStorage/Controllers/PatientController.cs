using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecureStorage.CQRS.Queries;

namespace SecureStorage.Controllers
{
    [ApiController]
    [Route("patient")]
    public class PatientController(IMediator mediator) : Controller
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetPatients(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { Message = "userId is required." });
            }

            try
            {
                var query = new GetPatientsQuery(userId);
                var patients = await _mediator.Send(query);

                return Ok(new { UserId = userId, Patients = patients });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error retrieving patients: {ex.Message}" });
            }
        }
    }

}