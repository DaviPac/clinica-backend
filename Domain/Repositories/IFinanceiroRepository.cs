using Clinica.Domain.ReadModels;

namespace Clinica.Domain.Repositories;

public interface IFinanceiroRepository
{
    Task<decimal> SaldoComissaoPendenteAsync(int profissionalId, string periodo, CancellationToken ct = default);
    Task<RelatorioFinanceiro> GetRelatorioFinanceiroAsync(string periodo, CancellationToken ct = default);
    Task<RelatorioSessoes?> GetRelatorioSessoesAsync(int profissionalId, DateOnly inicio, DateOnly fim, CancellationToken ct = default);
}