using Clinica.Application.Common;
using Clinica.Domain.Entities;
using Clinica.Domain.Enums;
using Clinica.Domain.Filters;
using Clinica.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class AgendamentoRepository(AppDbContext db) : IAgendamentoRepository
{
    public async Task CreateAsync(Agendamento agendamento, CancellationToken ct = default)
    {
        db.Agendamentos.Add(agendamento);
        await db.SaveChangesAsync(ct);
    }
    public async Task CreateManyAsync(IEnumerable<Agendamento> lote, CancellationToken ct = default)
    {
        db.Agendamentos.AddRange(lote);
        await db.SaveChangesAsync(ct);
    }
    public async Task<bool> CheckConflictAsync(int profissionalId, DateTimeOffset inicio, DateTimeOffset fim, CancellationToken ct = default)
    {
        return await db.Agendamentos.AnyAsync(a => 
            a.ProfissionalId == profissionalId &&
            a.Status != StatusAgendamento.CANCELADO &&
            a.DataHoraInicio < fim &&
            a.DataHoraFim > inicio,
            ct
        );
    }
    public async Task<bool> CheckAnyConflictAsync(
        int profissionalId, 
        IEnumerable<(DateTimeOffset Inicio, DateTimeOffset Fim)> intervalos, 
        CancellationToken ct = default
    )
    {
        var lista = intervalos.ToList();
        if (lista.Count == 0) return false;
        var minInicio = lista.Min(x => x.Inicio);
        var maxFim = lista.Max(x => x.Fim);

        var candidatos = await db.Agendamentos
            .Where(a => a.ProfissionalId == profissionalId &&
                        a.Status != StatusAgendamento.CANCELADO &&
                        a.DataHoraInicio < maxFim &&
                        a.DataHoraFim > minInicio)
            .Select(a => new { a.DataHoraInicio, a.DataHoraFim })
            .ToListAsync(ct);

        return lista.Any(intervalo =>
            candidatos.Any(c => 
                c.DataHoraInicio < intervalo.Fim && 
                c.DataHoraFim > intervalo.Inicio));
    }
    public async Task<Result<Agendamento>> FindByIdAsync(int id, CancellationToken ct = default)
    {
        var agendamento = await db.Agendamentos.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (agendamento is null)
            return Errors.ScheduleNotFound;

        return agendamento;
    }
    public async Task<Result<Agendamento>> FindByIdForProfissionalAsync(int id, int profissionalId, CancellationToken ct = default)
    {
        var agendamento = await db.Agendamentos.FirstOrDefaultAsync(a => a.Id == id && a.ProfissionalId == profissionalId, ct);
        if (agendamento is null)
            return Errors.ScheduleNotFound;

        return agendamento;
    }

    public async Task<IReadOnlyList<Agendamento>> ListAsync(FiltroAgendamento filtro, CancellationToken ct = default)
    {
        var query = db.Agendamentos.AsNoTracking().AsQueryable();

        if (filtro.ProfissionalId is int profId)
            query = query.Where(a => a.ProfissionalId == profId);

        if (filtro.PacienteId is int pacId)
            query = query.Where(a => a.PacienteId == pacId);

        if (filtro.Status is StatusAgendamento status)
            query = query.Where(a => a.Status == status);

        if (filtro.ApenasAtrasados)
        {
            var agora = DateTimeOffset.UtcNow;
            query = query.Where(a => a.DataHoraFim < agora);
        }

        if (filtro.PagamentoPendente)
            query = query.Where(a => !a.PagoPeloPaciente);

        if (filtro.De is DateTimeOffset de)
            query = query.Where(a => a.DataHoraInicio >= de);

        if (filtro.Ate is DateTimeOffset ate)
            query = query.Where(a => a.DataHoraInicio < ate);

        return await query
            .OrderBy(a => a.DataHoraInicio)
            .ToListAsync(ct);
    }
    public async Task<Result<Agendamento>> UpdateStatusAsync(int id, StatusAgendamento status, CancellationToken ct = default)
    {
        var agendamento = await db.Agendamentos.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (agendamento is null)
            return Errors.ScheduleNotFound;

        agendamento.Status = status;
        await db.SaveChangesAsync(ct);
        return agendamento;
    }
    public async Task<Result<Agendamento>> UpdateStatusForProfissionalAsync(int id, int profissionalId, StatusAgendamento status, CancellationToken ct = default)
    {
        var agendamento = await db.Agendamentos.FirstOrDefaultAsync(a => a.Id == id && a.ProfissionalId == profissionalId, ct);
        if (agendamento is null)
            return Errors.ScheduleNotFound;

        agendamento.Status = status;
        await db.SaveChangesAsync(ct);
        return agendamento;
    }
    public async Task<Result<Agendamento>> UpdatePagamentoAsync(int id, bool pago, CancellationToken ct = default)
    {
        var agendamento = await db.Agendamentos
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (agendamento is null)
            return Errors.ScheduleNotFound;

        // Caso pacote: tem regras especiais de propagação no grupo de recorrência
        if (agendamento.ValorPacote is not null)
        {
            if (agendamento.RecorrenciaGroupId is null)
                return Errors.UnknownError;

            var groupId = agendamento.RecorrenciaGroupId;

            if (pago)
            {
                // MARCAR COMO PAGO: se não há outros pendentes com valor > 0, marca todos do grupo
                var pendentes = await db.Agendamentos
                    .CountAsync(a =>
                        a.RecorrenciaGroupId == groupId &&
                        a.Id != id &&
                        a.ValorCombinado > 0 &&
                        !a.PagoPeloPaciente,
                        ct);

                if (pendentes == 0)
                {
                    await db.Agendamentos
                        .Where(a => a.RecorrenciaGroupId == groupId)
                        .ExecuteUpdateAsync(s => s.SetProperty(a => a.PagoPeloPaciente, true), ct);

                    return await db.Agendamentos.AsNoTracking().FirstAsync(a => a.Id == id, ct);
                }
                // se houver pendentes, cai pro fallback no final
            }
            else
            {
                // DESMARCAR: este agendamento + todos do grupo com valor zero (sessões "grátis" do pacote)
                await db.Agendamentos
                    .Where(a => a.Id == id ||
                            (a.RecorrenciaGroupId == groupId && a.ValorCombinado == 0))
                    .ExecuteUpdateAsync(s => s.SetProperty(a => a.PagoPeloPaciente, false), ct);

                return await db.Agendamentos.AsNoTracking().FirstAsync(a => a.Id == id, ct);
            }
        }

        // Fallback: atualiza só o agendamento atual
        // - Não é pacote, OU
        // - É pacote marcando como pago mas ainda há outros pendentes
        await db.Agendamentos
            .Where(a => a.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.PagoPeloPaciente, pago), ct);

        return await db.Agendamentos.AsNoTracking().FirstAsync(a => a.Id == id, ct);
    }
    public async Task<Result<Agendamento>> UpdatePagamentoForProfissionalAsync(int id, int profissionalId, bool pago, CancellationToken ct = default)
    {
        var agendamento = await db.Agendamentos
            .FirstOrDefaultAsync(a => a.Id == id && a.ProfissionalId == profissionalId, ct);

        if (agendamento is null)
            return Errors.ScheduleNotFound;

        // Caso pacote: tem regras especiais de propagação no grupo de recorrência
        if (agendamento.ValorPacote is not null)
        {
            if (agendamento.RecorrenciaGroupId is null)
                return Errors.UnknownError;

            var groupId = agendamento.RecorrenciaGroupId;

            if (pago)
            {
                // MARCAR COMO PAGO: se não há outros pendentes com valor > 0, marca todos do grupo
                var pendentes = await db.Agendamentos
                    .CountAsync(a =>
                        a.RecorrenciaGroupId == groupId &&
                        a.Id != id &&
                        a.ValorCombinado > 0 &&
                        !a.PagoPeloPaciente,
                        ct);

                if (pendentes == 0)
                {
                    await db.Agendamentos
                        .Where(a => a.RecorrenciaGroupId == groupId)
                        .ExecuteUpdateAsync(s => s.SetProperty(a => a.PagoPeloPaciente, true), ct);

                    return await db.Agendamentos.AsNoTracking().FirstAsync(a => a.Id == id, ct);
                }
                // se houver pendentes, cai pro fallback no final
            }
            else
            {
                // DESMARCAR: este agendamento + todos do grupo com valor zero (sessões "grátis" do pacote)
                await db.Agendamentos
                    .Where(a => a.Id == id ||
                            (a.RecorrenciaGroupId == groupId && a.ValorCombinado == 0))
                    .ExecuteUpdateAsync(s => s.SetProperty(a => a.PagoPeloPaciente, false), ct);

                return await db.Agendamentos.AsNoTracking().FirstAsync(a => a.Id == id, ct);
            }
        }

        // Fallback: atualiza só o agendamento atual
        // - Não é pacote, OU
        // - É pacote marcando como pago mas ainda há outros pendentes
        await db.Agendamentos
            .Where(a => a.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.PagoPeloPaciente, pago), ct);

        return await db.Agendamentos.AsNoTracking().FirstAsync(a => a.Id == id, ct);
    }
    public async Task<Result<Agendamento>> UpdateValorCombinadoAsync(int id, decimal valorCombinado, CancellationToken ct = default)
    {
        var agendamento = await db.Agendamentos.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (agendamento is null)
            return Errors.ScheduleNotFound;
        
        agendamento.ValorCombinado = valorCombinado;
        await db.SaveChangesAsync(ct);
        return agendamento;
    }
    public async Task<Result<Agendamento>> UpdateValorCombinadoForProfissionalAsync(int id, int profissionalId, decimal valorCombinado, CancellationToken ct = default)
    {
        var agendamento = await db.Agendamentos.FirstOrDefaultAsync(a => a.Id == id && a.ProfissionalId == profissionalId, ct);
        if (agendamento is null)
            return Errors.ScheduleNotFound;
        
        agendamento.ValorCombinado = valorCombinado;
        await db.SaveChangesAsync(ct);
        return agendamento;
    }
    public async Task<Result<Agendamento>> UpdateValorCombinadoRecorrenteAsync(int id, decimal valorCombinado, CancellationToken ct = default)
    {
        var agendamento = await db.Agendamentos.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (agendamento is null)
            return Errors.ScheduleNotFound;
        if (agendamento.RecorrenciaGroupId is null)
            return Errors.ValidationFailed("Agendamento não pertence a uma recorrência.");
        if (agendamento.ValorPacote is not null)
            return Errors.ValidationFailed("Não é possível alterar valor de recorrência de um pacote.");
        var valorArredondado = Math.Round(valorCombinado, 2, MidpointRounding.AwayFromZero);
        var recorrenciaGroupId = agendamento.RecorrenciaGroupId;
        await db.Agendamentos
            .Where(a => a.RecorrenciaGroupId == recorrenciaGroupId && !a.PagoPeloPaciente)
            .ExecuteUpdateAsync(
                s => s.SetProperty(a => a.ValorCombinado, valorArredondado),
                ct);
        agendamento.ValorCombinado = valorArredondado;
        return agendamento;
    }
    public async Task<Result<Agendamento>> UpdateValorCombinadoRecorrenteForProfissionalAsync(int id, int profissionalId, decimal valorCombinado, CancellationToken ct = default)
    {
        var agendamento = await db.Agendamentos.FirstOrDefaultAsync(a => a.Id == id && a.ProfissionalId == profissionalId, ct);
        if (agendamento is null)
            return Errors.ScheduleNotFound;
        if (agendamento.RecorrenciaGroupId is null)
            return Errors.ValidationFailed("Agendamento não pertence a uma recorrência.");
        if (agendamento.ValorPacote is not null)
            return Errors.ValidationFailed("Não é possível alterar valor de recorrência de um pacote.");
        var valorArredondado = Math.Round(valorCombinado, 2, MidpointRounding.AwayFromZero);
        var recorrenciaGroupId = agendamento.RecorrenciaGroupId;
        await db.Agendamentos
            .Where(a => a.ProfissionalId == profissionalId && a.RecorrenciaGroupId == recorrenciaGroupId && !a.PagoPeloPaciente)
            .ExecuteUpdateAsync(
                s => s.SetProperty(a => a.ValorCombinado, valorArredondado),
                ct);
        agendamento.ValorCombinado = valorArredondado;
        return agendamento;
    }
    public async Task<Result> CancelRecorrenciaAsync(string recorrenciaGroupId, CancellationToken ct = default)
    {
        var rowsAffected = await db.Agendamentos
            .Where(a =>
                a.RecorrenciaGroupId == recorrenciaGroupId&&
                a.DataHoraInicio > DateTimeOffset.UtcNow &&
                a.Status == StatusAgendamento.AGENDADO
            )
            .ExecuteUpdateAsync(
                s => s.SetProperty(a => a.Status, StatusAgendamento.CANCELADO),
                ct);

        if (rowsAffected == 0)
            return Errors.ScheduleNotFound;
        
        return Result.Success();
    }
    public async Task<Result> CancelRecorrenciaForProfissionalAsync(string recorrenciaGroupId, int profissionalId, CancellationToken ct = default)
    {
        var rowsAffected = await db.Agendamentos
            .Where(a =>
                a.RecorrenciaGroupId == recorrenciaGroupId &&
                a.ProfissionalId == profissionalId &&
                a.DataHoraInicio > DateTimeOffset.UtcNow &&
                a.Status == StatusAgendamento.AGENDADO
            )
            .ExecuteUpdateAsync(
                s => s.SetProperty(a => a.Status, StatusAgendamento.CANCELADO),
                ct);

        if (rowsAffected == 0)
            return Errors.ScheduleNotFound;
        
        return Result.Success();
    }
}