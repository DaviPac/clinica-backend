using Clinica.Application.Common;
using Clinica.Domain.Entities;
using Clinica.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories;

public class PacienteRepository(AppDbContext db) : IPacienteRepository
{
    public async Task CreateAsync(Paciente paciente, CancellationToken ct = default)
    {
        db.Pacientes.Add(paciente);
        await db.SaveChangesAsync(ct);
    }
    public async Task<Result> VincularProfissionalAsync(int pacienteId, int profissionalId, CancellationToken ct = default)
    {
        var jaVinculado = await db.PacienteProfissionais
          .AsNoTracking()
          .AnyAsync(pp => pp.PacienteId == pacienteId && pp.ProfissionalId == profissionalId, ct);

        if (jaVinculado) return Errors.PatientAlreadyLinked;
        var vinculo = new PacienteProfissional
        {
            PacienteId = pacienteId,
            ProfissionalId = profissionalId,
            Paciente = null!,
            Profissional = null!
        };

        db.PacienteProfissionais.Add(vinculo);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
    public async Task<Result<Paciente>> FindByIdAsync(int id, CancellationToken ct = default)
    {
        var paciente = await db.Pacientes
          .AsNoTracking()
          .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (paciente is null) return Errors.PatientNotFound;

        return paciente;
    }
    public async Task<Result<Paciente>> FindByIdTrackingAsync(int id, CancellationToken ct = default)
    {
        var paciente = await db.Pacientes.FirstOrDefaultAsync(p => p.Id == id, ct);

        if (paciente is null) return Errors.PatientNotFound;

        return paciente;
    }
    public async Task<Result<Paciente>> FindByCpfAsync(string cpf, CancellationToken ct = default)
    {
        var paciente = await db.Pacientes
          .AsNoTracking()
          .FirstOrDefaultAsync(p => p.Cpf == cpf, ct);

        if (paciente is null) return Errors.PatientNotFound;

        return paciente;
    }

    public async Task<IEnumerable<Paciente>> ListByProfissionalAsync(int profissionalId, bool showInativos, CancellationToken ct = default)
    {
        if (showInativos)
            return await db.Pacientes
                .AsNoTracking()
                .Where(p => p.ProfissionaisVinculados.Any(pp => pp.ProfissionalId == profissionalId))
                .OrderBy(p => p.Nome.ToLower())
                .ToListAsync(ct);
                
        return await db.Pacientes
            .AsNoTracking()
            .Where(p => p.Ativo && p.ProfissionaisVinculados.Any(pp => pp.ProfissionalId == profissionalId))
            .OrderBy(p => p.Nome.ToLower())
            .ToListAsync(ct);
    }
    public async Task<IEnumerable<Paciente>> ListAllAsync(bool showInativos, CancellationToken ct = default)
    {
        if (showInativos)
            return await db.Pacientes
                .AsNoTracking()
                .OrderBy(p => p.Nome.ToLower())
                .ToListAsync(ct);
        return await db.Pacientes
            .AsNoTracking()
            .Where(p => p.Ativo)
            .OrderBy(p => p.Nome.ToLower())
            .ToListAsync(ct);
    }
    public async Task<Result<Paciente>> FindByIdIncludingProfissionaisAsync(int id, CancellationToken ct = default)
    {
        var paciente = await db.Pacientes
          .AsNoTracking()
          .Include(p => p.ProfissionaisVinculados)
          .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (paciente is null)
            return Errors.PatientNotFound;
        return paciente;
    }
    public async Task<Result<Paciente>> FindByIdAndProfissionalAsync(int id, int profissionalId, CancellationToken ct = default)
    {
        var paciente = await db.Pacientes
          .AsNoTracking()
          .Where(p => p.ProfissionaisVinculados.Any(pp => pp.ProfissionalId == profissionalId))
          .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (paciente is null)
            return Errors.PatientNotFound;
        
        return paciente;
    }
    public async Task<Result<Paciente>> FindByIdAndProfissionalTrackingAsync(int id, int profissionalId, CancellationToken ct = default)
    {
        var paciente = await db.Pacientes
          .Where(p => p.ProfissionaisVinculados.Any(pp => pp.ProfissionalId == profissionalId))
          .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (paciente is null)
            return Errors.PatientNotFound;
        
        return paciente;
    }
    public async Task<Result> SetAtivoAsync(int id, bool ativo, CancellationToken ct = default)
    {
        var rows = await db.Pacientes
            .Where(p => p.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Ativo, ativo), ct);
        if (rows == 0)
            return Errors.PatientNotFound;
        return Result.Success();
    }
    public async Task<Result> DeleteVinculoAsync(int pacienteId, int profissionalId, CancellationToken ct = default)
    {
        var rows = await db.PacienteProfissionais
            .Where(pp => pp.PacienteId == pacienteId && pp.ProfissionalId == profissionalId)
            .ExecuteDeleteAsync(ct);
        if (rows == 0)
            return Errors.PatientNotFound;
        return Result.Success();
    }
}