using Clinica.Application.Common;
using Clinica.Application.Features.Agendamentos.DTOs;
using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Filters;
using Clinica.Domain.Repositories;

namespace Clinica.Application.Features.Agendamentos;

public class AgendamentoService(IAgendamentoRepository agendamentoRepository, IUsuarioRepository usuarioRepository) : IAgendamentoService
{
    public async Task<Result<IReadOnlyList<Agendamento>>> CriarAsync(int profissionalId, CriarAgendamentoRequest req, CancellationToken ct = default)
    {
        if (req.DuracaoMinutos <= 0)
            return Errors.ValidationFailed("Duração deve ser maior que 0");
        
        var dataHoraInicio = req.DataHoraInicio.ToUniversalTime();
        var fim = dataHoraInicio.AddMinutes(req.DuracaoMinutos);
        
        if (await agendamentoRepository.CheckConflictAsync(profissionalId, dataHoraInicio, fim, ct))
            return Errors.ConflictingSchedule;

        var profissionalResult = await usuarioRepository.FindByIdAsync(profissionalId, ct);
        if (!profissionalResult.IsSuccess)
            return profissionalResult.Error!;

        var taxa = profissionalResult.Value!.TaxaComissaoPadrao;

        var lote = new List<Agendamento>(req.TotalSessoes);

        if (!req.Recorrente)
        {
            var a = new Agendamento
            {
                PacienteId = req.PacienteId,
                ProfissionalId = profissionalId,
                ServicoId = req.ServicoId,
                DataHoraInicio = dataHoraInicio,
                DataHoraFim = fim,
                ValorCombinado = req.ValorCombinado,
                PercentualComissaoMomento = taxa,
                Status = StatusAgendamento.AGENDADO
            };
            await agendamentoRepository.CreateAsync(a, ct);
            lote.Add(a);
            return lote;
        }
        if (req.TotalSessoes < 2)
            return Errors.ValidationFailed("Total de sessões recorrentes deve ser maior que 1.");

        if (req.IntervaloSemanas < 1)
            return Errors.ValidationFailed("Intervalo semanal não pode ser menor que 1.");

        var groupId = Guid.NewGuid().ToString();
        var sessoesValores = new decimal[req.TotalSessoes];
            
        decimal? valorPacote = null;

        if (req.Pacote)
        {
            valorPacote = req.ValorCombinado;
            sessoesValores[0] = req.ValorCombinado;
        } else
        {
            for (int i = 0; i < req.TotalSessoes; i++)
                sessoesValores[i] = req.ValorCombinado;
        }

        var intervalos = new (DateTimeOffset start, DateTimeOffset end)[req.TotalSessoes];
        for (int i = 0; i < req.TotalSessoes; i++)
        {
            var start = dataHoraInicio.AddDays(i * req.IntervaloSemanas * 7);
            var end = start.AddMinutes(req.DuracaoMinutos);
            intervalos[i] = (start, end);
        }
        if (await agendamentoRepository.CheckAnyConflictAsync(profissionalId, intervalos, ct))
            return Errors.ConflictingSchedule;

        for (int i = 0; i < req.TotalSessoes; i++)
        {
            lote.Add(new Agendamento
            {
                PacienteId = req.PacienteId,
                ProfissionalId = profissionalId,
                ServicoId = req.ServicoId,
                DataHoraInicio = intervalos[i].start,
                DataHoraFim = intervalos[i].end,
                ValorCombinado = sessoesValores[i],
                ValorPacote = valorPacote,
                PercentualComissaoMomento = taxa,
                Status = StatusAgendamento.AGENDADO,
                RecorrenciaGroupId = groupId
            });
        }
        await agendamentoRepository.CreateManyAsync(lote, ct);
        return lote;
    }
    public async Task<Result<Agendamento>> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        return await agendamentoRepository.FindByIdAsync(id, ct);
    }
    public async Task<Result<Agendamento>> ObterPorIdParaProfissionalAsync(int id, int profissionalId, CancellationToken ct = default)
    {
        return await agendamentoRepository.FindByIdForProfissionalAsync(id, profissionalId, ct);
    }
    public async Task<IReadOnlyList<Agendamento>> ListAsync(FiltroAgendamento filtro, CancellationToken ct = default)
    {
        return await agendamentoRepository.ListAsync(filtro, ct);
    }
    public async Task<Result<Agendamento>> AtualizarStatusAsync(int id, StatusAgendamento status, CancellationToken ct = default)
    {
        return await agendamentoRepository.UpdateStatusAsync(id, status, ct);
    }
    public async Task<Result<Agendamento>> AtualizarStatusParaProfissionalAsync(int id, int profissionalId, StatusAgendamento status, CancellationToken ct = default)
    {
        return await agendamentoRepository.UpdateStatusForProfissionalAsync(id, profissionalId, status, ct);
    }
    public async Task<Result<Agendamento>> AtualizarPagamentoAsync(int id, bool pago, CancellationToken ct = default)
    {
        return await agendamentoRepository.UpdatePagamentoAsync(id, pago, ct);
    }
    public async Task<Result<Agendamento>> AtualizarPagamentoParaProfissionalAsync(int id, int profissionalId, bool pago, CancellationToken ct = default)
    {
        return await agendamentoRepository.UpdatePagamentoForProfissionalAsync(id, profissionalId, pago, ct);
    }
    public async Task<Result<Agendamento>> AtualizarValorCombinadoAsync(int id, decimal valorCombinado, CancellationToken ct = default)
    {
        if (valorCombinado < 0)
            return Errors.ValidationFailed("Valor não pode ser abaixo de 0.");

        return await agendamentoRepository.UpdateValorCombinadoAsync(id, valorCombinado, ct);
    }
    public async Task<Result<Agendamento>> AtualizarValorCombinadoParaProfissionalAsync(int id, int profissionalId, decimal valorCombinado, CancellationToken ct = default)
    {
        if (valorCombinado < 0)
            return Errors.ValidationFailed("Valor não pode ser abaixo de 0.");
            
        return await agendamentoRepository.UpdateValorCombinadoForProfissionalAsync(id, profissionalId, valorCombinado, ct);
    }
    public async Task<Result<Agendamento>> AtualizarValorCombinadoRecorrenteAsync(int id, decimal valorCombinado, CancellationToken ct = default)
    {
        if (valorCombinado < 0)
            return Errors.ValidationFailed("Valor não pode ser abaixo de 0.");
            
        return await agendamentoRepository.UpdateValorCombinadoRecorrenteAsync(id, valorCombinado, ct);
    }
    public async Task<Result<Agendamento>> AtualizarValorCombinadoRecorrenteParaProfissionalAsync(int id, int profissionalId, decimal valorCombinado, CancellationToken ct = default)
    {
        if (valorCombinado < 0)
            return Errors.ValidationFailed("Valor não pode ser abaixo de 0.");
            
        return await agendamentoRepository.UpdateValorCombinadoRecorrenteForProfissionalAsync(id, profissionalId, valorCombinado, ct);
    }
    public async Task<Result> CancelarRecorrenciaAsync(string recorrenciaGroupId, CancellationToken ct = default)
    {
        return await agendamentoRepository.CancelRecorrenciaAsync(recorrenciaGroupId, ct);
    }
    public async Task<Result> CancelarRecorrenciaParaProfissionalAsync(string recorrenciaGroupId, int profissionalId, CancellationToken ct = default)
    {
        return await agendamentoRepository.CancelRecorrenciaForProfissionalAsync(recorrenciaGroupId, profissionalId, ct);
    }
}