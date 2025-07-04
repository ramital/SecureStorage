﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureStorage.Application.Interfaces;
using SecureStorage.Models;

namespace SecureStorage.API.Controllers;

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
    public async Task<IActionResult> LoginAsync([FromBody] LoginModel model)
    {
        var token = await _tokenService.GenerateTokenAsync(model.Username, model.Password);
       
        if (token != null)
        {
            return Ok(new{token, model.Username});
        }
        return Unauthorized();
    }
}
