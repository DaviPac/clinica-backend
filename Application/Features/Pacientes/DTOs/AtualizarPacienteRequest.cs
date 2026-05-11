namespace Clinica.Application.Features.Pacientes.DTOs;

public record AtualizarPacienteRequest(
    string? Nome,
    string? Cpf,
    string? Telefone,
    string? DataNascimento,
    string? EnderecoCompleto,
    string? Rg
);
