using Api.Application.Services;
using Api.Application.Common;
using Api.Application.DTOs;
using Api.Middleware;
using Microsoft.AspNetCore.Mvc;
using Api.Domain;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

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
        return Ok(result.Value);
    }
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Listar(
        [FromQuery(Name = "profissional_id")] string? profissionalIdQuery,
        CancellationToken ct,
        [FromQuery(Name = "todos")] bool mostrarTodos = false
    )
    {
        var profissionalId = HttpContext.GetUserId();
        var role = HttpContext.GetRole();
        if (role == Role.ADMIN && int.TryParse(profissionalIdQuery, out int parsedId))
            profissionalId = parsedId;
            
        IEnumerable<Paciente> pacientes;
        if (mostrarTodos && role == Role.ADMIN)
            pacientes = await pacienteService.ListAllAsync(ct);
        else
            pacientes = await pacienteService.ListByProfissionalAsync(profissionalId, ct);
        return Ok(pacientes);
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

        return Ok(paciente.Value);
    }
}