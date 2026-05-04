using System.Text.Json.Serialization;

namespace Clinica.Application.Features.Agendamentos.DTOs;

public record AtualizarValorCombinadoAgendamentoRequest(
    [property: JsonPropertyName("valor_combinado")]
    decimal ValorCombinado
);