using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureStorage.Models;
using SecureStorage.Services;

namespace SecureStorage.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class PhiController : ControllerBase
{
    private readonly IPhiService _phiService;

    public PhiController(IPhiService phiService)
    {
        _phiService = phiService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PhiData phiData)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _phiService.StorePhiDataAsync(phiData);
        return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { result.Message });
    }

    [HttpGet]
    public async Task<IActionResult> Get(string patientKey)
    {
        if (string.IsNullOrEmpty(patientKey))
            return BadRequest(new { Message = "PatientKey is required." });

        var result = await _phiService.RetrievePhiDataAsync(patientKey);
        return result.IsSuccess ? Ok(new { Data = result.Data }) : BadRequest(new { result.Message });
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] PhiData phiData)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _phiService.UpdatePhiDataAsync(phiData);
        return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { result.Message });
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(string patientKey)
    {
        if (string.IsNullOrEmpty(patientKey))
            return BadRequest(new { Message = "PatientKey is required." });

        var result = await _phiService.DeletePhiDataAsync(patientKey);
        return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { result.Message });
    }
}