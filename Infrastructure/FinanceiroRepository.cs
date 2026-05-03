using Clinica.Domain;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure;

public class FinanceiroRepository(AppDbContext db) : IFinanceiroRepository
{
    public async Task<decimal> SaldoComissaoPendenteAsync(int profissionalId, string periodo, CancellationToken ct = default)
    {
        var partes = periodo.Split('-');
        var ano = int.Parse(partes[0]);
        var mes = int.Parse(partes[1]);
        var inicio = new DateTimeOffset(ano, mes, 1, 0, 0, 0, TimeSpan.Zero);
        var fim = inicio.AddMonths(1);

        var bruto = await db.Agendamentos
            .Where(a =>
                a.ProfissionalId == profissionalId &&
                a.DataHoraInicio >= inicio &&
                a.DataHoraInicio < fim &&
                a.PagoPeloPaciente &&
                a.Status != StatusAgendamento.CANCELADO
            ).SumAsync(a => a.ValorCombinado * (1 - a.PercentualComissaoMomento / 100m), ct);
        
        var repassado = await db.AcertosComissao
            .Where(a =>
                a.ProfissionalId == profissionalId &&
                a.PeriodoReferencia == periodo
            ).SumAsync(a => a.ValorPago, ct);
        
        return bruto - repassado;
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
                TotalRecebido = g.Sum(a => a.ValorCombinado),
                ComissaoClinica = g.Sum(a => a.ValorCombinado * a.PercentualComissaoMomento / 100m),
                AReceber = g.Sum(a => a.ValorCombinado * (1 - a.PercentualComissaoMomento / 100m))
            })
            .OrderBy(x => x.NomeProfissional)
            .ToListAsync(ct);

        var acertosMap = await db.AcertosComissao
            .AsNoTracking()
            .Where(a => a.PeriodoReferencia == periodo)
            .GroupBy(a => a.ProfissionalId)
            .Select(g => new { ProfissionalId = g.Key, Total = g.Sum(a => a.ValorPago) })
            .ToDictionaryAsync(x => x.ProfissionalId, x => x.Total, ct);

        var profissionais = profissionaisData
            .Select(p =>
            {
                var repassado = acertosMap.GetValueOrDefault(p.ProfissionalId, 0m);
                return new ResumoComissaoProfissional(
                    p.ProfissionalId,
                    p.NomeProfissional,
                    p.TotalRecebido,
                    p.ComissaoClinica,
                    p.AReceber,
                    repassado,
                    p.AReceber - repassado
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