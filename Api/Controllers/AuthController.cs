using Clinica.Application.Services;
using Clinica.Application.DTOs;
using Clinica.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Clinica.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("registrar")]
    [Authorize(policy: "AdminOnly")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioRequest req, CancellationToken ct)
    {
        var result = await authService.RegistrarAsync(req, ct);
        
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);

        return CreatedAtAction(nameof(Me), null, result.Value);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var result = await authService.LoginAsync(req, ct);

        if (!result.IsSuccess)
            return this.HandleError(result.Error!);

        return Ok(result.Value);
    }

    [HttpGet("usuarios")]
    [Authorize(policy: "AdminOnly")]
    public async Task<IActionResult> ListarUsuarios(CancellationToken ct)
    {
        var usuarios = await authService.ListarUsuariosAsync(ct);
        return Ok(usuarios);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var id = HttpContext.GetUserId(); 

        var result = await authService.ObterUsuarioPorIdAsync(id, ct);
        
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);

        return Ok(result.Value);
    }
}