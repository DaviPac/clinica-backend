using System.Text.Json.Serialization;

namespace Clinica.Application.Features.Servicos.DTOs;

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