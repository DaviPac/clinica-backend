using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;
using Clinica.Application.Interfaces;
using Clinica.Application.Features.AcertosComissao.DTOs;
using Clinica.Domain.Entities;
using Clinica.Domain.Filters;
using Clinica.Domain.Enums;
using Clinica.Api.Extensions;

namespace Clinica.Api.Controllers;

[ApiController]
[Route("financeiro/acertos")]
public partial class AcertoComissaoController(IAcertoComissaoService acertoComissaoService) : ControllerBase
{
    [HttpPost]
    [Authorize(policy: "AdminOnly")]
    public async Task<IActionResult> CriarAcerto([FromBody] CriarAcertoComissaoRequest req, CancellationToken ct)
    {
        var result = await acertoComissaoService.CriarAsync(req, ct);
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return StatusCode(201, AcertoToResponse(result.Value!));
    }
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Listar(
        [FromQuery(Name = "profissional_id")] int? profissionalId,
        [FromQuery(Name = "periodo")] string? periodo,
        CancellationToken ct)
    {
        var role = HttpContext.GetRole();
        if (role != Role.ADMIN)
            profissionalId = HttpContext.GetUserId();
        if (periodo is not null && !PeriodoValido(periodo))
            return BadRequest(new { error = "periodo_de deve estar no formato YYYY-MM" });
        var filtro = new FiltroAcertoComissao
        {
            ProfissionalId = profissionalId,
            Periodo = periodo
        };

        var acertos = await acertoComissaoService.ListAsync(filtro, ct);
        return Ok(acertos.Select(AcertoToResponse));
    }

    private static bool PeriodoValido(string s) => MyRegex().IsMatch(s);

    private static AcertoComissaoResponse AcertoToResponse(AcertoComissao a)
        => new(
            a.Id,
            a.ProfissionalId,
            a.PeriodoReferencia,
            a.ValorPago,
            a.DataPagamento,
            a.Observacao
        );

    [GeneratedRegex(@"^\d{4}-(0[1-9]|1[0-2])$")]
    private static partial Regex MyRegex();
}