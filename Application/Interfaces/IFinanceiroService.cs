using Clinica.Application.Common;
using Clinica.Domain.ReadModels;

namespace Clinica.Application.Interfaces;

public interface IFinanceiroService
{
    Task<Result<decimal>> SaldoComissaoPendenteAsync(int profissionalId, string periodo, CancellationToken ct = default);
    Task<Result<RelatorioFinanceiro>> GetRelatorioFinanceiroAsync(string periodo, CancellationToken ct = default);
}