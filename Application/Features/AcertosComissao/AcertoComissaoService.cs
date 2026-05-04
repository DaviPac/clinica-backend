using System.Text.RegularExpressions;
using Clinica.Application.Common;
using Clinica.Application.Features.AcertosComissao.DTOs;
using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Clinica.Domain.Filters;
using Clinica.Domain.Repositories;

namespace Clinica.Application.Features.AcertosComissao;

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