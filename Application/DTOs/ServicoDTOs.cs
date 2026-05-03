using System.Text.Json.Serialization;
using Api.Domain;

namespace Api.Application.DTOs;

public record CriarServicoRequest(
    string Nome,
    [property: JsonPropertyName("valor_atual")]
    decimal ValorAtual,
    bool Pacote
);

public record AtualizarServicoRequest(
    string? Nome,
    [property: JsonPropertyName("valor_atual")]
    decimal? ValorAtual,
    bool? Pacote,
    bool? Ativo
);

public record ServicoResponse(
    int Id,
    string Nome,
    [property: JsonPropertyName("valor_atual")]
    decimal ValorAtual,
    bool Ativo,
    [property: JsonPropertyName("is_pacote")]
    bool IsPacote,
    [property: JsonPropertyName("profissional_id")]
    int ProfissionalId
);
