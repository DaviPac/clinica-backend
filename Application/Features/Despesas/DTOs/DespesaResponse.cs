using System.Text.Json.Serialization;

namespace Clinica.Application.Features.Despesas.DTOs;

public record DespesaResponse(
    int Id,
    string Descricao,
    decimal Valor,
    [property: JsonPropertyName("data_vencimento")]
    DateOnly DataVencimento,
    [property: JsonPropertyName("status_pagamento")]
    bool StatusPagamento,
    string Categoria,
    [property: JsonPropertyName("criado_em")]
    DateTime CriadoEm
);