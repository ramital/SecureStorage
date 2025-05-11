using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureStorage.Models;
using SecureStorage.Services;


[AllowAnonymous]
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public AuthController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        var user = _tokenService.isValidUser(model.Username, model.Password);
       
        if (user.HasValue)
        {
            var token = _tokenService.GenerateToken(user.Value.Id,user.Value.Username);
            return Ok(token);
        }
        return Unauthorized();
    }
}
