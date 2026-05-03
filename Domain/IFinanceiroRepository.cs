namespace Clinica.Domain;

public interface IFinanceiroRepository
{
    Task<decimal> SaldoComissaoPendenteAsync(int profissionalId, string periodo, CancellationToken ct = default);
    Task<RelatorioFinanceiro> GetRelatorioFinanceiroAsync(string periodo, CancellationToken ct = default);
}