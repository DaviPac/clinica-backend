using Api.Application.Common;
using Api.Domain;

namespace Api.Application.Services;

public interface IFinanceiroService
{
    Task<Result<decimal>> SaldoComissaoPendenteAsync(int profissionalId, string periodo, CancellationToken ct = default);
    Task<Result<RelatorioFinanceiro>> GetRelatorioFinanceiroAsync(string periodo, CancellationToken ct = default);
}