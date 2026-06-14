// VocaPlay.Api/Controllers/AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocaPlay.Application.Auth.Commands;
using VocaPlay.Application.Auth.DTOs;

namespace VocaPlay.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly RegisterCommandHandler _register;
    private readonly LoginCommandHandler _login;
    private readonly RefreshTokenCommandHandler _refresh;

    public AuthController(RegisterCommandHandler register, LoginCommandHandler login, RefreshTokenCommandHandler refresh)
    {
        _register = register;
        _login = login;
        _refresh = refresh;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request, CancellationToken ct)
    {
        var result = await _register.Handle(new RegisterCommand(request.Email, request.DisplayName, request.Password), ct);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request, CancellationToken ct)
    {
        var result = await _login.Handle(new LoginCommand(request.Email, request.Password), ct);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponseDto>> Refresh(RefreshTokenRequestDto request, CancellationToken ct)
    {
        var result = await _refresh.Handle(new RefreshTokenCommand(request.RefreshToken), ct);
        return Ok(result);
    }

    [HttpPost("logout")]
    public IActionResult Logout(RefreshTokenRequestDto request)
    {
        // Tokens are stateless JWTs; logout is a client-side token discard. No server-side revocation in v1.
        return NoContent();
    }
}
