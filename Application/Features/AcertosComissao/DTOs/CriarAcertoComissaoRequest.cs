using System.Text.Json.Serialization;

namespace Clinica.Application.Features.AcertosComissao.DTOs;

public record CriarAcertoComissaoRequest(
    [property: JsonPropertyName("profissional_id")]
    int ProfissionalId,
    [property: JsonPropertyName("periodo_referencia")]
    string PeriodoReferencia, // "YYYY-MM"
    [property: JsonPropertyName("valor_pago")]
    decimal ValorPago,
    string? Observacao
);
