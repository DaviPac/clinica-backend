using Clinica.Domain.Enums;
using Clinica.Domain.ReadModels;
using Clinica.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class FinanceiroRepository(AppDbContext db) : IFinanceiroRepository
{
    public async Task<decimal> SaldoComissaoPendenteAsync(int profissionalId, string periodo, CancellationToken ct = default)
    {
        var partes = periodo.Split('-');
        var ano = int.Parse(partes[0]);
        var mes = int.Parse(partes[1]);
        var inicio = new DateTimeOffset(ano, mes, 1, 0, 0, 0, TimeSpan.Zero);
        var fim = inicio.AddMonths(1);

        var saldoAgendamentos = await db.Agendamentos
            .Where(a =>
                a.ProfissionalId == profissionalId &&
                a.DataHoraInicio >= inicio &&
                a.DataHoraInicio < fim &&
                a.PagoPeloPaciente &&
                a.Status != StatusAgendamento.CANCELADO
            ).SumAsync(a => a.ProfissionalRecebe
                ? -(a.ValorCombinado * a.PercentualComissaoMomento / 100m)
                : a.ValorCombinado * (1 - a.PercentualComissaoMomento / 100m), ct);
        
        var saldoAcertos = await db.AcertosComissao
            .Where(a =>
                a.ProfissionalId == profissionalId &&
                a.PeriodoReferencia == periodo
            ).SumAsync(a => a.ProfissionalRecebe ?
                -a.ValorPago :
                a.ValorPago, ct);
        
        return saldoAgendamentos + saldoAcertos;
    }
    public async Task<RelatorioSessoes?> GetRelatorioSessoesAsync(int profissionalId, DateOnly inicio, DateOnly fim, CancellationToken ct = default)
    {
        var profissional = await db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == profissionalId, ct);
        if (profissional is null)
            return null;

        var inicioDt = new DateTimeOffset(inicio.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var fimDt = new DateTimeOffset(fim.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var sessoes = await db.Agendamentos
            .AsNoTracking()
            .Where(a =>
                a.ProfissionalId == profissionalId &&
                a.Status != StatusAgendamento.CANCELADO &&
                a.DataHoraInicio >= inicioDt &&
                a.DataHoraInicio < fimDt)
            .OrderBy(a => a.DataHoraInicio)
            .Select(a => new SessaoRelatorio(
                a.Id,
                a.DataHoraInicio,
                a.Paciente!.Nome,
                a.Servico!.Nome,
                a.Status,
                a.PagoPeloPaciente,
                a.ProfissionalRecebe,
                a.ValorCombinado,
                a.PercentualComissaoMomento,
                a.ValorCombinado * a.PercentualComissaoMomento / 100m,
                a.ValorCombinado * (1 - a.PercentualComissaoMomento / 100m),
                a.PagoPeloPaciente && !a.ProfissionalRecebe ? a.ValorCombinado : 0m,
                a.PagoPeloPaciente && a.ProfissionalRecebe ? a.ValorCombinado : 0m,
                a.PagoPeloPaciente && !a.ProfissionalRecebe
                    ? a.ValorCombinado * (1 - a.PercentualComissaoMomento / 100m)
                    : 0m,
                a.PagoPeloPaciente && a.ProfissionalRecebe
                    ? a.ValorCombinado * a.PercentualComissaoMomento / 100m
                    : 0m
            ))
            .ToListAsync(ct);

        var totais = new TotaisRelatorioSessoes(
            sessoes.Count,
            sessoes.Sum(s => s.ValorSessao),
            sessoes.Sum(s => s.RecebidoPelaClinica),
            sessoes.Sum(s => s.RecebidoPeloProfissional),
            sessoes.Sum(s => s.DevidoAoProfissional),
            sessoes.Sum(s => s.DevidoAClinica)
        );

        return new RelatorioSessoes(profissionalId, profissional.Nome, inicio, fim, sessoes, totais);
    }

    public async Task<RelatorioFinanceiro> GetRelatorioFinanceiroAsync(string periodo, CancellationToken ct = default)
    {
        var partes = periodo.Split('-');
        var ano = int.Parse(partes[0]);
        var mes = int.Parse(partes[1]);

        var inicio = new DateTimeOffset(ano, mes, 1, 0, 0, 0, TimeSpan.Zero);
        var fim = inicio.AddMonths(1);

        var profissionaisData = await db.Agendamentos
            .AsNoTracking()
            .Where(a =>
                a.PagoPeloPaciente &&
                a.Status != StatusAgendamento.CANCELADO &&
                a.DataHoraInicio >= inicio &&
                a.DataHoraInicio < fim)
            .GroupBy(a => new { a.ProfissionalId, a.Profissional!.Nome })
            .Select(g => new
            {
                ProfissionalId = g.Key.ProfissionalId,
                NomeProfissional = g.Key.Nome,
                TotalFaturado = g.Sum(a => a.ValorCombinado),
                ComissaoClinica = g.Sum(a => a.ValorCombinado * a.PercentualComissaoMomento / 100m),
                DevidoAoProfissional = g.Sum(a => a.ProfissionalRecebe
                    ? 0m
                    : a.ValorCombinado * (1 - a.PercentualComissaoMomento / 100m)),
                DevidoAClinica = g.Sum(a => a.ProfissionalRecebe
                    ? a.ValorCombinado * a.PercentualComissaoMomento / 100m
                    : 0m),
            })
            .OrderBy(x => x.NomeProfissional)
            .ToListAsync(ct);

        var acertosMap = await db.AcertosComissao
            .AsNoTracking()
            .Where(a => a.PeriodoReferencia == periodo)
            .GroupBy(a => a.ProfissionalId)
            .Select(g => new
            {
                ProfissionalId = g.Key,
                AoProfissional = g.Sum(a => a.ProfissionalRecebe ? a.ValorPago : 0m),
                AClinica = g.Sum(a => a.ProfissionalRecebe ? 0m : a.ValorPago)
            })
            .ToDictionaryAsync(x => x.ProfissionalId, x => x, ct);

        var profissionais = profissionaisData
            .Select(p =>
            {
                var acertos = acertosMap.GetValueOrDefault(p.ProfissionalId);
                var repassadoAoProfissional = acertos?.AoProfissional ?? 0m;
                var repassadoAClinica = acertos?.AClinica ?? 0m;
                return new ResumoComissaoProfissional(
                    p.ProfissionalId,
                    p.NomeProfissional,
                    p.TotalFaturado,
                    p.ComissaoClinica,
                    p.DevidoAoProfissional,
                    p.DevidoAClinica,
                    repassadoAoProfissional,
                    repassadoAClinica,
                    p.DevidoAoProfissional - repassadoAoProfissional,
                    p.DevidoAClinica - repassadoAClinica
                );
            })
            .ToList();

        var totalComissoes = profissionais.Sum(p => p.ComissaoClinica);

        var inicioMes = new DateOnly(ano, mes, 1);
        var fimMes = inicioMes.AddMonths(1);

        var totalDespesas = await db.DespesasClinica
            .AsNoTracking()
            .Where(d =>
                d.StatusPagamento &&
                d.DataVencimento >= inicioMes &&
                d.DataVencimento < fimMes)
            .SumAsync(d => d.Valor, ct);

        return new RelatorioFinanceiro(
            periodo,
            profissionais,
            totalComissoes,
            totalDespesas,
            totalComissoes - totalDespesas
        );
    }
}