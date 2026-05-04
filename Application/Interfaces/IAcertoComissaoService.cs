using Clinica.Application.Common;
using Clinica.Application.Features.AcertosComissao.DTOs;
using Clinica.Domain.Entities;
using Clinica.Domain.Filters;

namespace Clinica.Application.Interfaces;

public interface IAcertoComissaoService
{
    Task<Result<AcertoComissao>> CriarAsync(CriarAcertoComissaoRequest req, CancellationToken ct = default);
    Task<IReadOnlyList<AcertoComissao>> ListAsync(FiltroAcertoComissao filtro, CancellationToken ct = default);
}