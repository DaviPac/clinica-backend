using System.Text.Json.Serialization;

namespace Clinica.Application.Features.Agendamentos.DTOs;

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