using System.Text.Json.Serialization;
using Api.Domain;

namespace Api.Application.DTOs;

public record CriarPacienteRequest(
    string  Nome,
    string? Cpf,
    string? Telefone,
    [property: JsonPropertyName("data_nascimento")]
    string? DataNascimento
);
