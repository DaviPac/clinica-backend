using System.Text.Json.Serialization;

namespace Clinica.Application.Features.Agendamentos.DTOs;

public record CriarAgendamentoRequest(
    [property: JsonPropertyName("paciente_id")]
    int PacienteId,
    [property: JsonPropertyName("servico_id")]
    int ServicoId,
    [property: JsonPropertyName("data_hora_inicio")]
    DateTimeOffset DataHoraInicio,
    [property: JsonPropertyName("duracao_minutos")]
    int DuracaoMinutos,
    [property: JsonPropertyName("valor_combinado")]
    decimal ValorCombinado,
    bool Recorrente,
    [property: JsonPropertyName("total_sessoes")]
    int TotalSessoes,
    [property: JsonPropertyName("intervalo_semanas")]
    int IntervaloSemanas,
    bool Pacote
);
