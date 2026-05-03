using Api.Application.Common;
using Api.Application.DTOs;
using Api.Domain;

namespace Api.Application.Services;

public interface IPacienteService
{
    Task<Result<Paciente>> CriarAsync(int profissionalId, CriarPacienteRequest req, CancellationToken ct = default);
    Task<IEnumerable<Paciente>> ListByProfissionalAsync(int profissionalId, CancellationToken ct = default);
    Task<IEnumerable<Paciente>> ListAllAsync(CancellationToken ct = default);
    Task<Result<Paciente>> FindByIdAsync(int id, CancellationToken ct = default);
    Task<Result<Paciente>> FindByIdAndProfissionalAsync(int id, int profissionalId, CancellationToken ct = default);
}