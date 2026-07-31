using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Clinica.Application.Features.Usuarios.DTOs;
using Clinica.Api.Extensions;

namespace Clinica.Api.Controllers;

[ApiController]
[Route("usuarios")]
public class UsuarioController(IUsuarioService usuarioService) : ControllerBase
{
    [HttpGet]
    [Authorize(policy: "AdminOnly")]
    public async Task<IActionResult> ListarUsuarios(CancellationToken ct)
    {
        var usuarios = await usuarioService.ListarUsuariosAsync(ct);
        return Ok(usuarios.Select(UsuarioToResponse));
    }

    [HttpGet("{id}")]
    [Authorize(policy: "AdminOnly")]
    public async Task<IActionResult> ObterUsuarioPorId(int id, CancellationToken ct)
    {
        var result = await usuarioService.ObterUsuarioPorIdAsync(id, ct);
        if (!result.IsSuccess)
        {
            return this.HandleError(result.Error!);
        }
        return Ok(UsuarioToResponse(result.Value!));
    }

    [HttpPut("{id}")]
    [Authorize(policy: "AdminOnly")]
    public async Task<IActionResult> AtualizarUsuario(int id, [FromBody] AtualizarUsuarioRequest request)
    {
        var result = await usuarioService.AtualizarUsuarioPorIdAsync(id, request, CancellationToken.None);
        if (!result.IsSuccess)
        {
            return this.HandleError(result.Error!);
        }
        return Ok(UsuarioToResponse(result.Value!));
    }

    private static UsuarioResponse UsuarioToResponse(Usuario u) => new(
        u.Id,
        u.Nome,
        u.Email,
        u.Role.ToString(),
        u.Profissao,
        u.TaxaComissaoPadrao,
        u.CriadoEm,
        u.ProfissionalRecebe
    );
}