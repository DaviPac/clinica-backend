using Clinica.Application.Common;
using Clinica.Application.Features.Financeiro.DTOs;
using Clinica.Application.Interfaces;
using Clinica.Domain.Enums;
using Clinica.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Controllers;

[ApiController]
[Route("financeiro")]
public class FinanceiroController(IFinanceiroService financeiroService) : ControllerBase
{
    [HttpGet("saldo-a-receber")]
    [Authorize]
    public async Task<IActionResult> SaldoAReceber(
        [FromQuery] string periodo,
        [FromQuery(Name = "profissional_id")] string? profissionalIdQuery,
        CancellationToken ct
    )
    {
        var profissionalId = HttpContext.GetUserId();
        var role = HttpContext.GetRole();
        if (role == Role.ADMIN && profissionalIdQuery != null)
            if (!int.TryParse(profissionalIdQuery, out profissionalId))
                return this.HandleError(Errors.ValidationFailed("ID do profissional inválido."));

        var result = await financeiroService.SaldoComissaoPendenteAsync(profissionalId, periodo, ct);
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return Ok(new SaldoAReceberResponse(profissionalId, periodo, result.Value));
    }
    [HttpGet("relatorio")]
    [Authorize(policy: "AdminOnly")]
    public async Task<IActionResult> ObterRelatorio([FromQuery] string periodo, CancellationToken ct = default)
    {
        var result = await financeiroService.GetRelatorioFinanceiroAsync(periodo, ct);
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return Ok(result.Value);
    }
}