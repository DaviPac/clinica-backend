using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Clinica.Application.Interfaces;
using Clinica.Application.Features.Auth.DTOs;
using Clinica.Domain.Entities;
using Clinica.Api.Extensions;

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

        return CreatedAtAction(nameof(Me), null, UsuarioToResponse(result.Value!));
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
        return Ok(usuarios.Select(UsuarioToResponse));
    }

    [HttpPost("me/senha")]
    [Authorize]
    public async Task<IActionResult> MudarSenha([FromBody] MudarSenhaRequest req, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        var result = await authService.MudarSenhaAsync(userId, req.SenhaAntiga, req.NovaSenha, ct);
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return Accepted();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var id = HttpContext.GetUserId(); 

        var result = await authService.ObterUsuarioPorIdAsync(id, ct);
        
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);

        return Ok(UsuarioToResponse(result.Value!));
    }
    private static UsuarioResponse UsuarioToResponse(Usuario u) => new(
        u.Id,
        u.Nome,
        u.Email,
        u.Role.ToString(),
        u.Profissao,
        u.TaxaComissaoPadrao,
        u.CriadoEm
    );
}