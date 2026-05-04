using System.Text.RegularExpressions;
using Clinica.Application.Common;
using Clinica.Application.Interfaces;
using Clinica.Domain.ReadModels;
using Clinica.Domain.Repositories;

namespace Clinica.Application.Features.Financeiro;

public partial class FinanceiroService(IFinanceiroRepository repo) : IFinanceiroService
{
    public async Task<Result<decimal>> SaldoComissaoPendenteAsync(int profissionalId, string periodo, CancellationToken ct = default)
    {
        if (!PeriodoValido(periodo))
            return Errors.ValidationFailed("Periodo deve seguir o formato 'YYYY-MM'.");
        return await repo.SaldoComissaoPendenteAsync(profissionalId, periodo, ct);
    }
    public async Task<Result<RelatorioFinanceiro>> GetRelatorioFinanceiroAsync(string periodo, CancellationToken ct = default)
    {
        if (!PeriodoValido(periodo))
            return Errors.ValidationFailed("Periodo deve seguir o formato 'YYYY-MM'.");
        return await repo.GetRelatorioFinanceiroAsync(periodo, ct);
    }

    private static bool PeriodoValido(string s) => MyRegex().IsMatch(s);
    [GeneratedRegex(@"^\d{4}-(0[1-9]|1[0-2])$")]
    private static partial Regex MyRegex();
}