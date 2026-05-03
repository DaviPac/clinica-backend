using Api.Application.Common;
using Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure;

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

    public async Task<Result<Paciente>> FindByCpfAsync(string cpf, CancellationToken ct = default)
    {
        var paciente = await db.Pacientes
          .AsNoTracking()
          .FirstOrDefaultAsync(p => p.Cpf == cpf, ct);

        if (paciente is null) return Errors.PatientNotFound;

        return paciente;
    }

    public async Task<IEnumerable<Paciente>> ListByProfissionalAsync(int profissionalId, CancellationToken ct = default)
    {
        var pacientes = await db.Pacientes
          .AsNoTracking()
          .Where(p => p.ProfissionaisVinculados.Any(pp => pp.ProfissionalId == profissionalId))
          .ToListAsync(ct);

        return pacientes;
    }
    public async Task<IEnumerable<Paciente>> ListAllAsync(CancellationToken ct = default)
    {
        var pacientes = await db.Pacientes
          .AsNoTracking()
          .ToListAsync(ct);

        return pacientes;
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
}