using System.Text.Json.Serialization;

namespace Api.Application.DTOs;

public record CriarAcertoComissaoRequest(
    [property: JsonPropertyName("profissional_id")]
    int ProfissionalId,
    [property: JsonPropertyName("periodo_referencia")]
    string PeriodoReferencia, // "YYYY-MM"
    [property: JsonPropertyName("valor_pago")]
    decimal ValorPago,
    string? Observacao
);

public record AcertoComissaoResponse(
    int Id,
    [property: JsonPropertyName("profissional_id")]
    int ProfissionalId,
    [property: JsonPropertyName("periodo_referencia")]
    string PeriodoReferencia,
    [property: JsonPropertyName("valor_pago")]
    decimal ValorPago,
    [property: JsonPropertyName("data_pagamento")]
    DateTimeOffset DataPagamento,
    string? Observacao
);
