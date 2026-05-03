using System.Text.Json.Serialization;

namespace Clinica.Domain;

public class DespesaClinica
{
    public int Id { get; set; }
    public required string Descricao { get; set; }
    public decimal Valor { get; set; }
    [JsonPropertyName("data_vencimento")]
    public DateOnly DataVencimento { get; set; }
    [JsonPropertyName("status_pagamento")]
    public bool StatusPagamento { get; set; }
    public CategoriaDespesa Categoria { get; set; }
    [JsonPropertyName("criado_em")]
    public DateTime CriadoEm { get; set; }
}

public enum CategoriaDespesa
{
    FIXA,
    VARIAVEL
}