using Clinica.Application.Common;
using Clinica.Application.DTOs;
using Clinica.Domain;

namespace Clinica.Application.Services;

public interface IAcertoComissaoService
{
    Task<Result<AcertoComissao>> CriarAsync(CriarAcertoComissaoRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<AcertoComissao>> ListAsync(FiltroAcertoComissao filtro, CancellationToken ct = default);
}