using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureStorage.Application.DTOs;
using SecureStorage.Application.Interfaces;

namespace SecureStorage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ConsentController : ControllerBase
    {
        private readonly IConsentService _consentService;

        public ConsentController(IConsentService consentService)
        {
            _consentService = consentService;
        }

        [HttpPost]
        public async Task<ActionResult<ConsentResult>> PostConsent([FromBody] ConsentDto request)
        {
            var response = await _consentService.CreateConsentAsync(request);
            return Ok(response);
        }


        [HttpGet("{patientId}")]
        public async Task<ActionResult<ConsentResult>> GetConsent(string patientId)
        {
            try
            {
                var response = await _consentService.GetConsentAsync(patientId);
                if (response == null)
                {
                    return NotFound(new { Message = "Consent not found for the specified patient." });
                }
                return Ok(response);
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                return NotFound();
            }
        }
    }
}