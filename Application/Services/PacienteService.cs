using System.Globalization;
using Clinica.Application.Common;
using Clinica.Application.DTOs;
using Clinica.Domain;

namespace Clinica.Application.Services;

public class PacienteService(IPacienteRepository repo) : IPacienteService
{
    public async Task<Result<Paciente>> CriarAsync(int profissionalId, CriarPacienteRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Nome))
            return Errors.ValidationFailed("Nome é obrigatório.");
        var cpf = req.Cpf;
        if (string.IsNullOrWhiteSpace(req.Cpf))
            cpf = null;
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
                return existente;
            }
        }
        var paciente = new Paciente
        {
            Nome = req.Nome,
            Cpf = cpf,
            Telefone = telefone,
            DataNascimento = dataNascimento
        };

        await repo.CreateAsync(paciente, ct);
        var vinculoResult = await repo.VincularProfissionalAsync(paciente.Id, profissionalId, ct);
        if (!vinculoResult.IsSuccess) return vinculoResult.Error!;
        return paciente;
    }
    public async Task<IEnumerable<Paciente>> ListByProfissionalAsync(int profissionalId, CancellationToken ct = default)
    {
        return await repo.ListByProfissionalAsync(profissionalId, ct);
    }
    public async Task<IEnumerable<Paciente>> ListAllAsync(CancellationToken ct = default)
    {
        return await repo.ListAllAsync(ct);
    }
    public async Task<Result<Paciente>> FindByIdAsync(int id, CancellationToken ct = default)
    {
        return await repo.FindByIdAsync(id, ct);
    }
    public async Task<Result<Paciente>> FindByIdAndProfissionalAsync(int id, int profissionalId, CancellationToken ct = default)
    {
        return await repo.FindByIdAndProfissionalAsync(id, profissionalId, ct);
    }
}