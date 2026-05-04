using System.Text.Json.Serialization;

namespace Clinica.Application.Features.Auth.DTOs;

public record UsuarioResponse(
    int Id,
    string Nome,
    string Email,
    string Role,
    string? Profissao,
    [property: JsonPropertyName("taxa_comissao_padrao")]
    decimal TaxaComissaoPadrao,
    [property: JsonPropertyName("criado_em")]
    DateTime CriadoEm
);