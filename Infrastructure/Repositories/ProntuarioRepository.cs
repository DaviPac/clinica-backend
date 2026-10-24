using Clinica.Application.Common;
using Clinica.Domain.Entities;
using Clinica.Domain.ReadModels;
using Clinica.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class ProntuarioRepository(AppDbContext db) : IProntuarioRepository
{
    public async Task CreateAsync(Prontuario prontuario, CancellationToken ct = default)
    {
        db.Prontuarios.Add(prontuario);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsByAgendamentoIdAsync(int agendamentoId, CancellationToken ct = default)
    {
        return await db.Prontuarios.AnyAsync(p => p.AgendamentoId == agendamentoId, ct);
    }

    public async Task<Result<ProntuarioVersao>> GetVersaoAtualAsync(int agendamentoId, CancellationToken ct = default)
    {
        var versao = await db.ProntuarioVersoes
            .AsNoTracking()
            .Include(v => v.Autor)
            .Where(v => v.Prontuario!.AgendamentoId == agendamentoId)
            .OrderByDescending(v => v.Id)
            .FirstOrDefaultAsync(ct);
        if (versao is null)
        {
            return Errors.MedicalRecordNotFound;
        }
        return versao;
    }

    public async Task<Result<ProntuarioVersao>> GetVersaoAsync(int agendamentoId, int versaoId, CancellationToken ct = default)
    {
        var versao = await db.ProntuarioVersoes
            .AsNoTracking()
            .Include(v => v.Autor)
            .FirstOrDefaultAsync(v => v.Id == versaoId && v.Prontuario!.AgendamentoId == agendamentoId, ct);
        if (versao is null)
        {
            return Errors.MedicalRecordVersionNotFound;
        }
        return versao;
    }

    public async Task<IReadOnlyList<ProntuarioVersaoResumo>> ListVersoesAsync(int agendamentoId, CancellationToken ct = default)
    {
        return await db.ProntuarioVersoes
            .AsNoTracking()
            .Where(v => v.Prontuario!.AgendamentoId == agendamentoId)
            .OrderByDescending(v => v.Id)
            .Select(v => new ProntuarioVersaoResumo(v.Id, v.CriadoEm, v.AutorId, v.Autor!.Nome))
            .ToListAsync(ct);
    }

    public async Task<Result> AddVersaoAsync(int agendamentoId, string conteudo, int autorId, CancellationToken ct = default)
    {
        var prontuarioId = await db.Prontuarios
            .Where(p => p.AgendamentoId == agendamentoId)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync(ct);
        if (prontuarioId is null)
        {
            return Errors.MedicalRecordNotFound;
        }
        db.ProntuarioVersoes.Add(new ProntuarioVersao
        {
            ProntuarioId = prontuarioId.Value,
            Conteudo = conteudo,
            AutorId = autorId
        });
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
