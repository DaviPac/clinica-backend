using System.Text.Json.Serialization;
using Clinica.Domain;

namespace Clinica.Application.DTOs;

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

public record AtualizarStatusAgendamentoRequest(
    StatusAgendamento Status
);

public record AtualizarPagamentoAgendamentoRequest(
    [property: JsonPropertyName("pago_pelo_paciente")]
    bool PagoPeloPaciente
);

public record AtualizarValorCombinadoAgendamentoRequest(
    [property: JsonPropertyName("valor_combinado")]
    decimal ValorCombinado
);

public record AgendamentoResponse(
    int Id,
    [property: JsonPropertyName("paciente_id")]
    int PacienteId,
    [property: JsonPropertyName("profissional_id")]
    int ProfissionalId,
    [property: JsonPropertyName("servico_id")]
    int ServicoId,
    [property: JsonPropertyName("data_hora_inicio")]
    DateTimeOffset DataHoraInicio,
    [property: JsonPropertyName("data_hora_fim")]
    DateTimeOffset DataHoraFim,
    [property: JsonPropertyName("valor_combinado")]
    decimal ValorCombinado,
    [property: JsonPropertyName("valor_pacote")]
    decimal? ValorPacote,
    [property: JsonPropertyName("percentual_comissao_momento")]
    decimal PercentualComissaoMomento,
    string Status,
    [property: JsonPropertyName("pago_pelo_paciente")]
    bool PagoPeloPaciente,
    [property: JsonPropertyName("recorrencia_group_id")]
    string? RecorrenciaGroupId,
    [property: JsonPropertyName("criado_em")]
    DateTime CriadoEm
);

public record CriarAgendamentosResponse(
    [property: JsonPropertyName("recorrencia_group_id")]
    string RecorrenciaGroupId,
    int TotalCriados,
    IEnumerable<AgendamentoResponse> Agendamentos
);
