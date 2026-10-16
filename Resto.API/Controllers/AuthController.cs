using Microsoft.AspNetCore.Mvc;
using Resto.API.Models;
using Resto.API.Services;

namespace Resto.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(
                ApiResponse<object>.Fail(
                    "Invalid login information."));

        var manager =
            await _authService.LoginAsync(
                request.Email,
                request.Password);

        if (manager == null)
        {
            return Unauthorized(
                ApiResponse<object>.Fail(
                    "Invalid email or password."));
        }

        var response = new LoginResponse
        {
            Id = manager.Id,
            FullName = manager.FullName,
            Email = manager.Email,
            Role = manager.Role
        };

        return Ok(
            ApiResponse<LoginResponse>.Ok(
                response,
                "Login successful."));
    }
}