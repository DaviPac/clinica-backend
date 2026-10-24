using Clinica.Application.Common;
using Clinica.Application.Features.Agendamentos.DTOs;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Filters;
using Clinica.Domain.ReadModels;
using System.Text.Json;

namespace Clinica.Application.Interfaces;

public interface IAgendamentoService
{
    Task<Result> AtualizarAnotacoesPorAgendamentoIdAsync(JsonElement conteudo, int agendamentoId, int profissionalId, CancellationToken ct = default);
    Task<Result<Agendamento>> AtualizarPagamentoAsync(int id, bool pago, CancellationToken ct = default);
    Task<Result<Agendamento>> AtualizarPagamentoParaProfissionalAsync(int id, int profissionalId, bool pago, CancellationToken ct = default);
    Task<Result> AtualizarProntuarioPorAgendamentoIdAsync(JsonElement conteudo, int agendamentoId, int profissionalId, CancellationToken ct = default);
    Task<Result<Agendamento>> AtualizarStatusAsync(int id, StatusAgendamento status, CancellationToken ct = default);
    Task<Result<Agendamento>> AtualizarStatusParaProfissionalAsync(int id, int profissionalId, StatusAgendamento status, CancellationToken ct = default);
    Task<Result<Agendamento>> AtualizarValorCombinadoAsync(int id, decimal valorCombinado, CancellationToken ct = default);
    Task<Result<Agendamento>> AtualizarValorCombinadoParaProfissionalAsync(int id, int profissionalId, decimal valorCombinado, CancellationToken ct = default);
    Task<Result<Agendamento>> AtualizarValorCombinadoRecorrenteAsync(int id, decimal valorCombinado, CancellationToken ct = default);
    Task<Result<Agendamento>> AtualizarValorCombinadoRecorrenteParaProfissionalAsync(int id, int profissionalId, decimal valorCombinado, CancellationToken ct = default);
    Task<Result> CancelarRecorrenciaAsync(string recorrenciaGroupId, CancellationToken ct = default);
    Task<Result> CancelarRecorrenciaParaProfissionalAsync(string recorrenciaGroupId, int profissionalId, CancellationToken ct = default);
    Task<Result> CriarAnotacoesPorAgendamentoIdAsync(JsonElement conteudo, int agendamentoId, int profissionalId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Agendamento>>> CriarAsync(int profissionalId, CriarAgendamentoRequest req, CancellationToken ct = default);
    Task<Result> CriarProntuarioPorAgendamentoIdAsync(JsonElement conteudo, int agendamentoId, int profissionalId, CancellationToken ct = default);
    Task<Result> ExcluirAnotacoesPorAgendamentoIdAsync(int agendamentoId, int profissionalId, CancellationToken ct = default);
    Task<IReadOnlyList<Agendamento>> ListAsync(FiltroAgendamento filtro, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ProntuarioVersaoResumo>>> ListarVersoesProntuarioAsync(int agendamentoId, int profissionalId, CancellationToken ct = default);
    Task<Result<AnotacoesAgendamento>> ObterAnotacoesPorAgendamentoIdAsync(int agendamentoId, int profissionalId, CancellationToken ct = default);
    Task<Result<Agendamento>> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<Result<Agendamento>> ObterPorIdParaProfissionalAsync(int id, int profissionalId, CancellationToken ct = default);
    Task<Result<ProntuarioVersao>> ObterProntuarioPorAgendamentoIdAsync(int agendamentoId, int profissionalId, CancellationToken ct = default);
    Task<Result<ProntuarioVersao>> ObterVersaoProntuarioAsync(int agendamentoId, int versaoId, int profissionalId, CancellationToken ct = default);
    Task<Result> ReagendarAsync(int id, DateTimeOffset novoInicio, DateTimeOffset novoFim, CancellationToken ct = default);
    Task<Result> ReagendarParaProfissionalAsync(int id, int profissionalId, DateTimeOffset novoInicio, DateTimeOffset novoFim, CancellationToken ct = default);
    Task<Result> ReagendarRecorrenciaAsync(int id, DateTimeOffset novoInicio, DateTimeOffset novoFim, int intervaloSemanas, CancellationToken ct = default);
    Task<Result> ReagendarRecorrenciaParaProfissionalAsync(int id, int profissionalId, DateTimeOffset novoInicio, DateTimeOffset novoFim, int intervaloSemanas, CancellationToken ct = default);
}
