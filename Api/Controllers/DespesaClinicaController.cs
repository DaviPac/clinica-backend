using Clinica.Application.Features.Despesas.DTOs;
using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Clinica.Api.Extensions;

namespace Clinica.Api.Controllers;

[ApiController]
[Route("financeiro/despesas")]
public class DespesaClinicaController(IDespesaClinicaService despesaClinicaService) : ControllerBase
{
    [HttpPost]
    [Authorize(policy: "AdminOnly")]
    public async Task<IActionResult> CriarDespesa([FromBody] CriarDespesaRequest req, CancellationToken ct)
    {
        var result = await despesaClinicaService.CriarDespesa(req, ct);
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return StatusCode(201, DespesaToResponse(result.Value!));
    }
    [HttpGet]
    [Authorize(policy: "AdminOnly")]
    public async Task<IActionResult> Listar(
        [FromQuery] DateOnly? de,
        [FromQuery] DateOnly? ate,
        [FromQuery(Name = "em_aberto")] bool? emAberto,
        [FromQuery] CategoriaDespesa? categoria,
        CancellationToken ct)
    {
        var filtro = new FiltroDespesa
        {
            De = de,
            Ate = ate,
            Pago = !emAberto,
            Categoria = categoria
        };

        var despesas = await despesaClinicaService.ListarAsync(filtro, ct);
        return Ok(despesas.Select(DespesaToResponse));
    }
    [HttpPatch("{id}/pagar")]
    [Authorize(policy: "AdminOnly")]
    public async Task<IActionResult> PagarDespesa(int id, CancellationToken ct)
    {
        var result = await despesaClinicaService.AtualizarPagamentoAsync(id, true, ct);
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return Ok(new MarcarDespesaPagoResponse(true));
    }

    private static DespesaResponse DespesaToResponse(DespesaClinica d)
        => new(d.Id, d.Descricao, d.Valor, d.DataVencimento, d.StatusPagamento, d.Categoria.ToString(), d.CriadoEm);
}