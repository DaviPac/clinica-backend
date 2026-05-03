using Clinica.Application.Common;

namespace Clinica.Domain;

public interface IPacienteRepository
{
    Task<Result<Paciente>> FindByIdAsync(int id, CancellationToken ct = default);
    Task<Result<Paciente>> FindByCpfAsync(string cpf, CancellationToken ct = default);
    Task CreateAsync(Paciente paciente, CancellationToken ct = default);
    Task<Result> VincularProfissionalAsync(int pacienteId, int profissionalId, CancellationToken ct = default);
    Task<IEnumerable<Paciente>> ListByProfissionalAsync(int profissionalId, CancellationToken ct = default);
    Task<IEnumerable<Paciente>> ListAllAsync(CancellationToken ct = default);
    Task<Result<Paciente>> FindByIdIncludingProfissionaisAsync(int id, CancellationToken ct = default);
    Task<Result<Paciente>> FindByIdAndProfissionalAsync(int id, int profissionalId, CancellationToken ct = default);
}