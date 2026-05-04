using System.Text.Json.Serialization;

namespace Clinica.Application.Features.Agendamentos.DTOs;

public record CriarAgendamentosResponse(
    [property: JsonPropertyName("recorrencia_group_id")]
    string RecorrenciaGroupId,
    int TotalCriados,
    IEnumerable<AgendamentoResponse> Agendamentos
);