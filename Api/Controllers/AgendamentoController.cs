using Clinica.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using System.Text.Json;
using Clinica.Application.Interfaces;
using Clinica.Application.Features.Agendamentos.DTOs;
using Clinica.Domain.Enums;
using Clinica.Domain.Entities;
using Clinica.Domain.Filters;
using Clinica.Api.Extensions;

namespace Clinica.Api.Controllers;

[ApiController]
[Route("agendamentos")]
public class AgendamentoController(IAgendamentoService agendamentoService) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Criar(
        [FromBody] CriarAgendamentoRequest req,
        [FromQuery(Name = "profissional_id")] string? profissionalIdQuery,
        CancellationToken ct
    )
    {
        var profissionalId = HttpContext.GetUserId();
        var role = HttpContext.GetRole();

        if (role == Role.ADMIN && int.TryParse(profissionalIdQuery, out int parsedId))
            profissionalId = parsedId;

        var result = await agendamentoService.CriarAsync(profissionalId, req, ct);
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);

        return StatusCode(201, result.Value!.Count == 1 ?
            AgendamentoToResponse(result.Value[0]) :
            AgendamentosToCriarAgendamentosResponse(result.Value));
    }
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Listar(
        [FromQuery] bool? todos,
        [FromQuery] string? filtro,
        [FromQuery] string? de,
        [FromQuery] string? ate,
        [FromQuery(Name = "paciente_id")] string? pacienteIdQuery,
        [FromQuery(Name = "profissional_id")] string? profissionalIdQuery,
        CancellationToken ct)
    {
        var profissionalId = HttpContext.GetUserId();
        var isAdmin = HttpContext.GetRole() == Role.ADMIN;
        var mostrarTodos = todos ?? false;

        var filtroAg = new FiltroAgendamento();
        if (isAdmin && int.TryParse(profissionalIdQuery, out int parsedId))
            profissionalId = parsedId;

        if (!isAdmin || !mostrarTodos)
            filtroAg.ProfissionalId = profissionalId;

        if (!string.IsNullOrWhiteSpace(pacienteIdQuery) && int.TryParse(pacienteIdQuery, out int pacienteId))
            filtroAg.PacienteId = pacienteId;

        switch (filtro)
        {
            case "pendente":
                filtroAg.Status = StatusAgendamento.AGENDADO;
                filtroAg.ApenasAtrasados = true;
                break;

            case "pagamento_pendente":
                filtroAg.Status = StatusAgendamento.REALIZADO;
                filtroAg.PagamentoPendente = true;
                break;

            case "prontuario_pendente":
                filtroAg.Status = StatusAgendamento.REALIZADO;
                filtroAg.ProntuarioPendente = true;
                break;

            default:
                (filtroAg.De, filtroAg.Ate) = ParseFiltrosPeriodo(de, ate);
                break;
        }

        var agendamentos = await agendamentoService.ListAsync(filtroAg, ct);
        return Ok(agendamentos.Select(AgendamentoToResponse));
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
    {
        var role = HttpContext.GetRole();
        Result<Agendamento> result;
        if (role == Role.ADMIN)
            result = await agendamentoService.ObterPorIdAsync(id, ct);
        else
            result = await agendamentoService.ObterPorIdParaProfissionalAsync(id, HttpContext.GetUserId(), ct);

        if (!result.IsSuccess)
            return this.HandleError(result.Error!);

        return Ok(AgendamentoToResponse(result.Value!));
    }
    [HttpPatch("{id}/status")]
    [Authorize]
    public async Task<IActionResult> AtualizarStatus(int id, [FromBody] AtualizarStatusAgendamentoRequest req, CancellationToken ct)
    {
        var role = HttpContext.GetRole();
        Result<Agendamento> result;
        if (role == Role.ADMIN)
            result = await agendamentoService.AtualizarStatusAsync(id, req.Status, ct);
        else
            result = await agendamentoService.AtualizarStatusParaProfissionalAsync(id, HttpContext.GetUserId(), req.Status, ct);
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return Ok(AgendamentoToResponse(result.Value!));
    }
    [HttpPatch("{id}/pagamento")]
    [Authorize]
    public async Task<IActionResult> AtualizarPagamento(int id, [FromBody] AtualizarPagamentoAgendamentoRequest req, CancellationToken ct)
    {
        var role = HttpContext.GetRole();
        Result<Agendamento> result;
        if (role == Role.ADMIN)
            result = await agendamentoService.AtualizarPagamentoAsync(id, req.PagoPeloPaciente, ct);
        else
            result = await agendamentoService.AtualizarPagamentoParaProfissionalAsync(id, HttpContext.GetUserId(), req.PagoPeloPaciente, ct);
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return Ok(AgendamentoToResponse(result.Value!));
    }
    [HttpPatch("{id}/valor-combinado")]
    [Authorize]
    public async Task<IActionResult> AtualizarValorCombinado(
        int id,
        [FromBody] AtualizarValorCombinadoAgendamentoRequest req,
        [FromQuery] bool? recorrente,
        CancellationToken ct
    )
    {
        var role = HttpContext.GetRole();
        Result<Agendamento> result;
        if (role == Role.ADMIN)
            if (recorrente == true)
                result = await agendamentoService.AtualizarValorCombinadoRecorrenteAsync(id, req.ValorCombinado, ct);
            else
                result = await agendamentoService.AtualizarValorCombinadoAsync(id, req.ValorCombinado, ct);
        else
            if (recorrente == true)
                result = await agendamentoService.AtualizarValorCombinadoRecorrenteParaProfissionalAsync(id, HttpContext.GetUserId(), req.ValorCombinado, ct);
            else
                result = await agendamentoService.AtualizarValorCombinadoParaProfissionalAsync(id, HttpContext.GetUserId(), req.ValorCombinado, ct);
        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return Ok(AgendamentoToResponse(result.Value!));
    }
    [HttpDelete("recorrencia/{recorrenciaGroupId}")]
    [Authorize]
    public async Task<IActionResult> CancelarRecorrencia(string recorrenciaGroupId, CancellationToken ct)
    {
        var role = HttpContext.GetRole();
        Result result;
        if (role == Role.ADMIN)
            result = await agendamentoService.CancelarRecorrenciaAsync(recorrenciaGroupId, ct);
        else
            result = await agendamentoService.CancelarRecorrenciaParaProfissionalAsync(recorrenciaGroupId, HttpContext.GetUserId(), ct);

        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return NoContent();
    }

    [HttpPatch("{id}/reagendar")]
    [Authorize]
    public async Task<IActionResult> Reagendar(
        int id,
        [FromBody] ReagendarAgendamentoRequest req,
        CancellationToken ct
    )
    {
        var role = HttpContext.GetRole();
        Result result;
        if (role == Role.ADMIN)
            if (req.ReagendarRecorrencia)
                result = await agendamentoService.ReagendarRecorrenciaAsync(id, req.NovoInicio, req.NovoFim, req.IntervaloSemanas, ct);
            else
                result = await agendamentoService.ReagendarAsync(id, req.NovoInicio, req.NovoFim, ct);
        else
            if (req.ReagendarRecorrencia)
                result = await agendamentoService.ReagendarRecorrenciaParaProfissionalAsync(id, HttpContext.GetUserId(), req.NovoInicio, req.NovoFim, req.IntervaloSemanas, ct);
            else
                result = await agendamentoService.ReagendarParaProfissionalAsync(id, HttpContext.GetUserId(), req.NovoInicio, req.NovoFim, ct);

        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return NoContent();
    }

    [HttpGet("{id}/prontuario")]
    [Authorize]
    public async Task<IActionResult> ObterProntuario(int id, CancellationToken ct)
    {
        var result = await agendamentoService.ObterProntuarioPorAgendamentoIdAsync(id, HttpContext.GetUserId(), ct);

        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return Ok(VersaoToResponse(result.Value!));
    }

    [HttpGet("{id}/prontuario/versoes")]
    [Authorize]
    public async Task<IActionResult> ListarVersoesProntuario(int id, CancellationToken ct)
    {
        var result = await agendamentoService.ListarVersoesProntuarioAsync(id, HttpContext.GetUserId(), ct);

        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return Ok(result.Value!.Select(v => new ProntuarioVersaoResumoResponse(v.Id, v.AutorId, v.NomeAutor, v.CriadoEm)));
    }

    [HttpGet("{id}/prontuario/versoes/{versaoId}")]
    [Authorize]
    public async Task<IActionResult> ObterVersaoProntuario(int id, int versaoId, CancellationToken ct)
    {
        var result = await agendamentoService.ObterVersaoProntuarioAsync(id, versaoId, HttpContext.GetUserId(), ct);

        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return Ok(VersaoToResponse(result.Value!));
    }

    [HttpPut("{id}/prontuario")]
    [Authorize]
    public async Task<IActionResult> AtualizarProntuario(
        int id,
        [FromBody] SalvarDocumentoRequest req,
        CancellationToken ct
    )
    {
        var result = await agendamentoService.AtualizarProntuarioPorAgendamentoIdAsync(req.Conteudo, id, HttpContext.GetUserId(), ct);

        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return NoContent();
    }

    [HttpPost("{id}/prontuario")]
    [Authorize]
    public async Task<IActionResult> CriarProntuario(
        int id,
        [FromBody] SalvarDocumentoRequest req,
        CancellationToken ct
    )
    {
        var result = await agendamentoService.CriarProntuarioPorAgendamentoIdAsync(req.Conteudo, id, HttpContext.GetUserId(), ct);

        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return StatusCode(201);
    }

    [HttpGet("{id}/anotacoes")]
    [Authorize]
    public async Task<IActionResult> ObterAnotacoes(int id, CancellationToken ct)
    {
        var result = await agendamentoService.ObterAnotacoesPorAgendamentoIdAsync(id, HttpContext.GetUserId(), ct);

        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return Ok(new DocumentoResponse(JsonSerializer.Deserialize<JsonElement>(result.Value!.Conteudo)));
    }

    [HttpPut("{id}/anotacoes")]
    [Authorize]
    public async Task<IActionResult> AtualizarAnotacoes(
        int id,
        [FromBody] SalvarDocumentoRequest req,
        CancellationToken ct
    )
    {
        var result = await agendamentoService.AtualizarAnotacoesPorAgendamentoIdAsync(req.Conteudo, id, HttpContext.GetUserId(), ct);

        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return NoContent();
    }

    [HttpPost("{id}/anotacoes")]
    [Authorize]
    public async Task<IActionResult> CriarAnotacoes(
        int id,
        [FromBody] SalvarDocumentoRequest req,
        CancellationToken ct
    )
    {
        var result = await agendamentoService.CriarAnotacoesPorAgendamentoIdAsync(req.Conteudo, id, HttpContext.GetUserId(), ct);

        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return StatusCode(201);
    }

    [HttpDelete("{id}/anotacoes")]
    [Authorize]
    public async Task<IActionResult> ExcluirAnotacoes(int id, CancellationToken ct)
    {
        var result = await agendamentoService.ExcluirAnotacoesPorAgendamentoIdAsync(id, HttpContext.GetUserId(), ct);

        if (!result.IsSuccess)
            return this.HandleError(result.Error!);
        return NoContent();
    }

    private static ProntuarioVersaoResponse VersaoToResponse(ProntuarioVersao versao)
    {
        return new ProntuarioVersaoResponse(
            versao.Id,
            JsonSerializer.Deserialize<JsonElement>(versao.Conteudo),
            versao.AutorId,
            versao.Autor?.Nome,
            versao.CriadoEm
        );
    }

    private static (DateTimeOffset? De, DateTimeOffset? Ate) ParseFiltrosPeriodo(string? de, string? ate)
    {
        return (ParseDe(de), ParseAte(ate));
    }

    private static DateTimeOffset? ParseDe(string? s)
    {
        if (string.IsNullOrEmpty(s))
            return null;

        if (!DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
            return null;

        // Meia-noite UTC do dia
        return new DateTimeOffset(data.Year, data.Month, data.Day, 0, 0, 0, TimeSpan.Zero);
    }

    private static DateTimeOffset? ParseAte(string? s)
    {
        if (string.IsNullOrEmpty(s))
            return null;

        if (!DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
            return null;

        // 23:59:59 UTC do dia
        return new DateTimeOffset(data.Year, data.Month, data.Day, 23, 59, 59, TimeSpan.Zero);
    }

    private static readonly TimeSpan FusoBrasilia = TimeSpan.FromHours(-3);

    private static AgendamentoResponse AgendamentoToResponse(Agendamento a)
        => new(
            Id: a.Id,
            PacienteId: a.PacienteId,
            Paciente: a.Paciente?.Nome,
            ProfissionalId: a.ProfissionalId,
            Profissional: a.Profissional?.Nome,
            ServicoId: a.ServicoId,
            Servico: a.Servico?.Nome,
            DataHoraInicio: a.DataHoraInicio.ToOffset(FusoBrasilia),
            DataHoraFim: a.DataHoraFim.ToOffset(FusoBrasilia),
            ValorCombinado: a.ValorCombinado,
            ValorPacote: a.ValorPacote,
            PercentualComissaoMomento: a.PercentualComissaoMomento,
            Status: a.Status.ToString(),
            PagoPeloPaciente: a.PagoPeloPaciente,
            RecorrenciaGroupId: a.RecorrenciaGroupId,
            CriadoEm: a.CriadoEm,
            ProfissionalRecebe: a.ProfissionalRecebe,
            PossuiProntuario: a.Prontuario is not null);

    private static CriarAgendamentosResponse AgendamentosToCriarAgendamentosResponse(IEnumerable<Agendamento> ags)
        => new(ags.First().RecorrenciaGroupId!, ags.Count(), ags.Select(AgendamentoToResponse));
}
