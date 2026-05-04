using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Clinica.Application.Interfaces;
using Clinica.Application.Features.Pacientes.DTOs;
using Clinica.Domain.Enums;
using Clinica.Domain.Entities;
using Clinica.Api.Extensions;

namespace Clinica.Api.Controllers;

[ApiController]
[Route("pacientes")]
public class PacienteController(IPacienteService pacienteService) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Criar(
        [FromBody] CriarPacienteRequest req,
        [FromQuery(Name = "profissional_id")] string? profissionalIdQuery,
        CancellationToken ct
    )
    {
        var profissionalId = HttpContext.GetUserId();
        var role = HttpContext.GetRole();

        if (role == Role.ADMIN && int.TryParse(profissionalIdQuery, out int parsedId))
            profissionalId = parsedId;

        var result = await pacienteService.CriarAsync(profissionalId, req, ct);
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return Ok(PacienteToResponse(result.Value!));
    }
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Listar(
        [FromQuery(Name = "profissional_id")] string? profissionalIdQuery,
        CancellationToken ct,
        [FromQuery(Name = "todos")] bool mostrarTodos = false,
        [FromQuery(Name = "inativos")] bool mostrarInativos = false
    )
    {
        var profissionalId = HttpContext.GetUserId();
        var role = HttpContext.GetRole();
        if (role == Role.ADMIN && int.TryParse(profissionalIdQuery, out int parsedId))
            profissionalId = parsedId;
            
        IEnumerable<Paciente> pacientes;
        if (mostrarTodos && role == Role.ADMIN)
            pacientes = await pacienteService.ListAllAsync(mostrarInativos, ct);
        else
            pacientes = await pacienteService.ListByProfissionalAsync(profissionalId, mostrarInativos, ct);
        return Ok(pacientes.Select(PacienteToResponse));
    }
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var profissionalId = HttpContext.GetUserId();
        var role = HttpContext.GetRole();

        var paciente = role == Role.ADMIN ? 
            await pacienteService.FindByIdAsync(id, ct) : 
            await pacienteService.FindByIdAndProfissionalAsync(id, profissionalId, ct);

        if (!paciente.IsSuccess)
            return this.HandleError(paciente.Error!);

        return Ok(PacienteToResponse(paciente.Value!));
    }
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> RemoverPorId(int id, CancellationToken ct)
    {
        var profissionalId = HttpContext.GetUserId();
        var role = HttpContext.GetRole();

        var result = role == Role.ADMIN ? 
            await pacienteService.DesativarAsync(id, ct) : 
            await pacienteService.RemoverVinculoAsync(id, profissionalId, ct);
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return NoContent();
    }
    [HttpPatch("{id}/ativar")]
    [Authorize(policy: "AdminOnly")]
    public async Task<IActionResult> AtivarPorId(int id, CancellationToken ct)
    {
        var result = await pacienteService.AtivarAsync(id, ct);
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return NoContent();
    }

    private static PacienteResponse PacienteToResponse(Paciente p) => new(
        p.Id,
        p.Nome,
        p.Cpf,
        p.Telefone,
        p.DataNascimento,
        p.Ativo,
        p.CriadoEm
    );
}