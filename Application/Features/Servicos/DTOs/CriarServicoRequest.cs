using System.Text.Json.Serialization;

namespace Clinica.Application.Features.Servicos.DTOs;

public record CriarServicoRequest(
    string Nome,
    [property: JsonPropertyName("valor_atual")]
    decimal ValorAtual,
    bool Pacote
);
