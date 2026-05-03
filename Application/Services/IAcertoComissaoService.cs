using Api.Application.Common;
using Api.Application.DTOs;
using Api.Domain;

namespace Api.Application.Services;

public interface IAcertoComissaoService
{
    Task<Result<AcertoComissao>> CriarAsync(CriarAcertoComissaoRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<AcertoComissao>> ListAsync(FiltroAcertoComissao filtro, CancellationToken ct = default);
}