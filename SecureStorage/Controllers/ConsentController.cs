using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureStorage.Models;
using SecureStorage.Services;

namespace SecureStorage.Controllers
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
        public async Task<ActionResult<ConsentResponse>> PostConsent([FromBody] ConsentRequest request)
        {
            var response = await _consentService.CreateConsentAsync(request);
            return Ok(response);
        }


        [HttpGet("{patientId}")]
        public async Task<ActionResult<ConsentResponse>> GetConsent(string patientId)
        {
            try
            {
                var response = await _consentService.GetConsentAsync(patientId);
                return Ok(response);
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                return NotFound();
            }
        }
    }
}