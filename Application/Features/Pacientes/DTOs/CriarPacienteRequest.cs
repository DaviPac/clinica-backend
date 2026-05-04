using System.Text.Json.Serialization;

namespace Clinica.Application.Features.Pacientes.DTOs;

public record CriarPacienteRequest(
    string  Nome,
    string? Cpf,
    string? Telefone,
    [property: JsonPropertyName("data_nascimento")]
    string? DataNascimento
);
