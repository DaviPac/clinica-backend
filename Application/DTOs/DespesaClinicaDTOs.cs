using System.Text.Json.Serialization;
using Clinica.Domain;

namespace Clinica.Application.DTOs;

public record DespesaClinicaDTO(
    string Descricao,
    decimal Valor,
    [property: JsonPropertyName("data_vencimento")]
    DateOnly DataVencimento,
    CategoriaDespesa Categoria
);

public record DespesaResponse(
    int Id,
    string Descricao,
    decimal Valor,
    [property: JsonPropertyName("data_vencimento")]
    DateOnly DataVencimento,
    [property: JsonPropertyName("status_pagamento")]
    bool StatusPagamento,
    CategoriaDespesa Categoria,
    [property: JsonPropertyName("criado_em")]
    DateTime CriadoEm
);

public record MarcarDespesaPagoResponse(
    bool Pago
);
