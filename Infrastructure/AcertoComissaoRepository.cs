using Clinica.Domain;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure;

public class AcertoComissaoRepository(AppDbContext db) : IAcertoComissaoRepository
{
    public async Task CreateAsync(AcertoComissao acerto, CancellationToken ct = default)
    {
        db.AcertosComissao.Add(acerto);
        await db.SaveChangesAsync(ct);
    }
    public async Task<IReadOnlyList<AcertoComissao>> ListAsync(FiltroAcertoComissao filtro, CancellationToken ct = default)
    {
        var query = db.AcertosComissao.AsNoTracking().AsQueryable();

        if (filtro.ProfissionalId is int profId)
            query = query.Where(a => a.ProfissionalId == profId);

        if (filtro.Periodo is string de)
            query = query.Where(a => string.Compare(a.PeriodoReferencia, de) >= 0);

        return await query
            .OrderByDescending(a => a.PeriodoReferencia)
            .ThenByDescending(a => a.DataPagamento)
            .ToListAsync(ct);
    }
}