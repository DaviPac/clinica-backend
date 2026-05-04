using Clinica.Domain.Entities;
using Clinica.Domain.Filters;

namespace Clinica.Domain.Repositories;

public interface IAcertoComissaoRepository
{
    Task CreateAsync(AcertoComissao acerto, CancellationToken ct = default);
    Task<IReadOnlyList<AcertoComissao>> ListAsync(FiltroAcertoComissao filtro, CancellationToken ct = default);
}