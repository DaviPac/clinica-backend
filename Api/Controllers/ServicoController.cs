using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Clinica.Application.Interfaces;
using Clinica.Application.Features.Servicos.DTOs;
using Clinica.Domain.Enums;
using Clinica.Domain.Entities;
using Clinica.Api.Extensions;

namespace Clinica.Api.Controllers;

[ApiController]
[Route("servicos")]
public class ServicoController(IServicoService servicoService) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Criar(
        [FromBody] CriarServicoRequest req,
        [FromQuery(Name = "profissional_id")] string? profissionalIdQuery,
        CancellationToken ct
    )
    {
        var profissionalId = HttpContext.GetUserId();
        var role = HttpContext.GetRole();

        if (role == Role.ADMIN && int.TryParse(profissionalIdQuery, out int parsedId))
            profissionalId = parsedId;

        var result = await servicoService.CriarAsync(profissionalId, req, ct);
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        
        return Ok(ServicoToResponse(result.Value!));
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

        IEnumerable<Servico> servicos;
        if (role == Role.ADMIN && mostrarTodos)
            servicos = await servicoService.ListAllAsync(mostrarInativos, ct);
        else
            servicos = await servicoService.ListByProfissionalAsync(profissionalId, mostrarInativos, ct);

        return Ok(servicos.Select(ServicoToResponse));
    }

    [HttpPatch("{id}")]
    [Authorize]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarServicoRequest req, CancellationToken ct)
    {
        var profissionalId = HttpContext.GetUserId();
        var role = HttpContext.GetRole();

        var result = await (role == Role.ADMIN ? servicoService.AtualizarAsync(id, req, ct) :
            servicoService.AtualizarByProfissionalAsync(id, profissionalId, req, ct));
        
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);

        return Ok(ServicoToResponse(result.Value!));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Desativar(int id, CancellationToken ct)
    {
        var profissionalId = HttpContext.GetUserId();
        var role = HttpContext.GetRole();

        var result = await (role == Role.ADMIN ? servicoService.DesativarAsync(id, ct) :
            servicoService.DesativarByProfissionalAsync(id, profissionalId, ct));
        
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);

        return Ok(ServicoToResponse(result.Value!));
    }

    private static ServicoResponse ServicoToResponse(Servico s)
        => new(s.Id, s.Nome, s.ValorAtual, s.Ativo, s.IsPacote, s.ProfissionalId);
}