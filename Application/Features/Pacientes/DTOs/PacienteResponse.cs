using System.Text.Json.Serialization;

namespace Clinica.Application.Features.Pacientes.DTOs;

public record PacienteResponse(
    int Id,
    string Nome,
    string? Cpf,
    string? Telefone,
    DateOnly? DataNascimento,
    bool Ativo,
    DateTime CriadoEm,
    string? EnderecoCompleto,
    string? Rg
);