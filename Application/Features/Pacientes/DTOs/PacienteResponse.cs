using System.Text.Json.Serialization;

namespace Clinica.Application.Features.Pacientes.DTOs;

public record PacienteResponse(
    int Id,
    string Nome,
    string? Cpf,
    string? Telefone,
    [property: JsonPropertyName("data_nascimento")]
    DateOnly? DataNascimento,
    [property: JsonPropertyName("criado_em")]
    DateTime CriadoEm
);