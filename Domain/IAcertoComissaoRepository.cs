namespace Clinica.Domain;

public interface IAcertoComissaoRepository
{
    Task CreateAsync(AcertoComissao acerto, CancellationToken ct = default);
    Task<IReadOnlyList<AcertoComissao>> ListAsync(FiltroAcertoComissao filtro, CancellationToken ct = default);
}