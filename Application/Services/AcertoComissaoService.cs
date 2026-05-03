using System.Text.RegularExpressions;
using Api.Application.Common;
using Api.Application.DTOs;
using Api.Domain;

namespace Api.Application.Services;

public partial class AcertoComissaoService(IAcertoComissaoRepository repo) : IAcertoComissaoService
{
    public async Task<Result<AcertoComissao>> CriarAsync(CriarAcertoComissaoRequest req, CancellationToken ct = default)
    {
        if (!IsValid(req.PeriodoReferencia))
            return Errors.ValidationFailed("Periodo deve ser no formato 'YYYY-MM'.");
        if (req.ValorPago <= 0)
            return Errors.ValidationFailed("Valor pago deve ser maior que 0.");
        
        var acerto = new AcertoComissao
        {
            ProfissionalId = req.ProfissionalId,
            PeriodoReferencia = req.PeriodoReferencia,
            ValorPago = req.ValorPago,
            DataPagamento = DateTimeOffset.UtcNow,
            Observacao = req.Observacao
        };
        await repo.CreateAsync(acerto, ct);
        return acerto;
    }
    public async Task<IReadOnlyList<AcertoComissao>> ListAsync(FiltroAcertoComissao filtro, CancellationToken ct = default)
    {
        return await repo.ListAsync(filtro, ct);
    }
    private static bool IsValid(string periodo) => MyRegex().IsMatch(periodo);

    [GeneratedRegex(@"^\d{4}-(0[1-9]|1[0-2])$")]
    private static partial Regex MyRegex();
}