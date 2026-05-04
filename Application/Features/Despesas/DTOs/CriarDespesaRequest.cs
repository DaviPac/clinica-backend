using System.Text.Json.Serialization;
using Clinica.Domain.Enums;

namespace Clinica.Application.Features.Despesas.DTOs;

public record CriarDespesaRequest(
    string Descricao,
    decimal Valor,
    [property: JsonPropertyName("data_vencimento")]
    DateOnly DataVencimento,
    CategoriaDespesa Categoria
);
