using System.Globalization;
using Clinica.Application.Common;
using Clinica.Application.Features.Pacientes.DTOs;
using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Clinica.Domain.Repositories;

namespace Clinica.Application.Features.Pacientes;

public class PacienteService(IPacienteRepository repo, IUnitOfWork unitOfWork) : IPacienteService
{
    public async Task<Result<(Paciente paciente, bool existe)>> CriarAsync(int profissionalId, CriarPacienteRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Nome))
            return Errors.ValidationFailed("Nome é obrigatório.");
        var cpf = req.Cpf;
        if (string.IsNullOrWhiteSpace(req.Cpf))
            cpf = null;
        if (cpf?.Length > 14)
            return Errors.ValidationFailed("CPF deve ter no máximo 14 dígitos.");
        var rg = req.Rg;
        if (string.IsNullOrWhiteSpace(req.Rg))
            rg = null;
        var enderecoCompleto = req.EnderecoCompleto;
        if (string.IsNullOrWhiteSpace(req.EnderecoCompleto))
            enderecoCompleto = null;
        var telefone = req.Telefone;
        if (string.IsNullOrWhiteSpace(req.Telefone))
            telefone = null;
        DateOnly? dataNascimento = null;
        if (!string.IsNullOrWhiteSpace(req.DataNascimento))
        {
            if (!DateOnly.TryParseExact(req.DataNascimento, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dataConvertida))
            {
                return Errors.ValidationFailed("A data deve estar no formato YYYY-MM-DD.");
            }
            dataNascimento = dataConvertida;
        }

        if (cpf is not null)
        {
            var existente = await repo.FindByCpfAsync(cpf, ct);
            if (existente.IsSuccess)
            {
                var vinculo = await repo.VincularProfissionalAsync(existente.Value!.Id, profissionalId, ct);
                if (!vinculo.IsSuccess) return vinculo.Error!;
                return (existente.Value, true);
            }
        }
        var paciente = new Paciente
        {
            Nome = req.Nome,
            Cpf = cpf,
            Rg = rg,
            EnderecoCompleto = enderecoCompleto,
            Telefone = telefone,
            DataNascimento = dataNascimento
        };

        await repo.CreateAsync(paciente, ct);
        var vinculoResult = await repo.VincularProfissionalAsync(paciente.Id, profissionalId, ct);
        if (!vinculoResult.IsSuccess) return vinculoResult.Error!;
        return (paciente, false);
    }
    public async Task<IEnumerable<Paciente>> ListByProfissionalAsync(int profissionalId, bool mostrarInativos, CancellationToken ct = default)
    {
        return await repo.ListByProfissionalAsync(profissionalId, mostrarInativos, ct);
    }
    public async Task<IEnumerable<Paciente>> ListAllAsync(bool mostrarInativos, CancellationToken ct = default)
    {
        return await repo.ListAllAsync(mostrarInativos, ct);
    }
    public async Task<Result<Paciente>> FindByIdAsync(int id, CancellationToken ct = default)
    {
        return await repo.FindByIdAsync(id, ct);
    }
    public async Task<Result<Paciente>> FindByIdAndProfissionalAsync(int id, int profissionalId, CancellationToken ct = default)
    {
        return await repo.FindByIdAndProfissionalAsync(id, profissionalId, ct);
    }
    public async Task<Result> DesativarAsync(int id, CancellationToken ct = default)
    {
        return await repo.SetAtivoAsync(id, false, ct);
    }
    public async Task<Result> AtivarAsync(int id, CancellationToken ct = default)
    {
        return await repo.SetAtivoAsync(id, true, ct);
    }
    public async Task<Result> RemoverVinculoAsync(int pacienteId, int profissionalId, CancellationToken ct = default)
    {
        return await repo.DeleteVinculoAsync(pacienteId, profissionalId, ct);
    }
    public async Task<Result<Paciente>> AtualizarPacienteAsync(int pacienteId, AtualizarPacienteRequest req, CancellationToken ct = default)
    {
        var result = await repo.FindByIdTrackingAsync(pacienteId, ct);
        if (!result.IsSuccess)
            return result.Error!;
        var paciente = result.Value!;
        if (!string.IsNullOrWhiteSpace(req.Cpf))
            paciente.Cpf = req.Cpf;
        if (!string.IsNullOrWhiteSpace(req.DataNascimento)) {
            if (!DateOnly.TryParseExact(req.DataNascimento, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dataConvertida))
            {
                return Errors.ValidationFailed("A data deve estar no formato YYYY-MM-DD.");
            }
            paciente.DataNascimento = dataConvertida;
        }
        if (!string.IsNullOrWhiteSpace(req.EnderecoCompleto))
            paciente.EnderecoCompleto = req.EnderecoCompleto;
        if (!string.IsNullOrWhiteSpace(req.Nome))
            paciente.Nome = req.Nome;
        if (!string.IsNullOrWhiteSpace(req.Rg))
            paciente.Rg = req.Rg;
        if (!string.IsNullOrWhiteSpace(req.Telefone))
            paciente.Telefone = req.Telefone;

        await unitOfWork.CommitAsync();
        return paciente;
    }
    public async Task<Result<Paciente>> AtualizarPacientePorProfissionalAsync(int pacienteId, int profissionalId, AtualizarPacienteRequest req, CancellationToken ct = default)
    {
        var result = await repo.FindByIdAndProfissionalTrackingAsync(pacienteId, profissionalId, ct);
        if (!result.IsSuccess)
            return result.Error!;
        var paciente = result.Value!;
        if (!string.IsNullOrWhiteSpace(req.Cpf))
            paciente.Cpf = req.Cpf;
        if (!string.IsNullOrWhiteSpace(req.DataNascimento)) {
            if (!DateOnly.TryParseExact(req.DataNascimento, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dataConvertida))
            {
                return Errors.ValidationFailed("A data deve estar no formato YYYY-MM-DD.");
            }
            paciente.DataNascimento = dataConvertida;
        }
        if (!string.IsNullOrWhiteSpace(req.EnderecoCompleto))
            paciente.EnderecoCompleto = req.EnderecoCompleto;
        if (!string.IsNullOrWhiteSpace(req.Nome))
            paciente.Nome = req.Nome;
        if (!string.IsNullOrWhiteSpace(req.Rg))
            paciente.Rg = req.Rg;
        if (!string.IsNullOrWhiteSpace(req.Telefone))
            paciente.Telefone = req.Telefone;

        await unitOfWork.CommitAsync();
        return paciente;
    }
}