using Clinica.Application.Common;
using Clinica.Application.DTOs;
using Clinica.Domain;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure;

public class ServicoRepository(AppDbContext db) : IServicoRepository
{
    public async Task CreateAsync(Servico servico, CancellationToken ct = default)
    {
        db.Servicos.Add(servico);
        await db.SaveChangesAsync(ct);
    }
    public async Task<Result<Servico>> FindByIdAsync(int id, CancellationToken ct = default)
    {
        var servico = await db.Servicos
          .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (servico is null)
            return Errors.ServiceNotFound;

        return servico;
    }
    public async Task<Result<Servico>> FindByIdAndProfissionalAsync(int id, int profissionalId, CancellationToken ct = default)
    {
        var servico = await db.Servicos
          .FirstOrDefaultAsync(s => s.Id == id && s.ProfissionalId == profissionalId, ct);

        if (servico is null)
            return Errors.ServiceNotFound;

        return servico;
    }
    public async Task<IEnumerable<Servico>> ListAllAsync(bool mostrarInativos, CancellationToken ct = default)
    {
        return mostrarInativos ? await db.Servicos
          .AsNoTracking()
          .ToListAsync(ct) : await db.Servicos
          .AsNoTracking()
          .Where(s => s.Ativo == true)
          .ToListAsync(ct);
    }

    public async Task<IEnumerable<Servico>> ListByProfissionalAsync(int profissionalId, bool mostrarInativos, CancellationToken ct = default)
    {
        return mostrarInativos ? await db.Servicos
          .AsNoTracking()
          .Where(s => s.ProfissionalId == profissionalId)
          .ToListAsync(ct) : await db.Servicos
          .AsNoTracking()
          .Where(s => s.ProfissionalId == profissionalId && s.Ativo == true)
          .ToListAsync(ct);
    }

    public async Task<Result<Servico>> UpdateAsync(Servico servico, CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
        return servico;
    }
}