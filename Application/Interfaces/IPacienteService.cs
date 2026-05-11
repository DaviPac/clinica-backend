using Clinica.Application.Common;
using Clinica.Application.Features.Pacientes.DTOs;
using Clinica.Domain.Entities;

namespace Clinica.Application.Interfaces;

public interface IPacienteService
{
    Task<Result<(Paciente paciente, bool existe)>> CriarAsync(int profissionalId, CriarPacienteRequest req, CancellationToken ct = default);
    Task<IEnumerable<Paciente>> ListByProfissionalAsync(int profissionalId, bool mostrarInativos, CancellationToken ct = default);
    Task<IEnumerable<Paciente>> ListAllAsync(bool mostrarInativos, CancellationToken ct = default);
    Task<Result<Paciente>> FindByIdAsync(int id, CancellationToken ct = default);
    Task<Result<Paciente>> FindByIdAndProfissionalAsync(int id, int profissionalId, CancellationToken ct = default);
    Task<Result> DesativarAsync(int id, CancellationToken ct = default);
    Task<Result> AtivarAsync(int id, CancellationToken ct = default);
    Task<Result> RemoverVinculoAsync(int pacienteId, int profissionalId, CancellationToken ct = default);
    Task<Result<Paciente>> AtualizarPacienteAsync(int pacienteId, AtualizarPacienteRequest req, CancellationToken ct = default);
    Task<Result<Paciente>> AtualizarPacientePorProfissionalAsync(int pacienteId, int profissionalId, AtualizarPacienteRequest req, CancellationToken ct = default);
}