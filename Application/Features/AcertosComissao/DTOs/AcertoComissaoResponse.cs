using System.Text.Json.Serialization;

namespace Clinica.Application.Features.AcertosComissao.DTOs;

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
