using System.Text.Json.Serialization;

namespace Clinica.Application;

public record SaldoAReceberResponse(
    [property: JsonPropertyName("profissional_id")]
    int ProfissionalId,
    string Periodo,
    [property: JsonPropertyName("saldo_a_receber")]
    decimal SaldoAReceber
);