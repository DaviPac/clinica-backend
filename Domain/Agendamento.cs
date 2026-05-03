using System.Text.Json.Serialization;

namespace Api.Domain;

public class Agendamento
{
    public int Id { get; set; }
    [JsonPropertyName("paciente_id")]
    public int PacienteId { get; set; }
    [JsonIgnore]
    public Paciente? Paciente { get; set; }
    [JsonPropertyName("profissional_id")]
    public int ProfissionalId { get; set; }
    [JsonIgnore]
    public Usuario? Profissional { get; set; }
    [JsonPropertyName("servico_id")]
    public int ServicoId { get; set; }
    [JsonIgnore]
    public Servico? Servico { get; set; }
    [JsonPropertyName("data_hora_inicio")]
    public DateTimeOffset DataHoraInicio { get; set; }
    [JsonPropertyName("data_hora_fim")]
    public DateTimeOffset DataHoraFim { get; set; }
    [JsonPropertyName("valor_combinado")]
    public decimal ValorCombinado { get; set; }
    [JsonPropertyName("valor_pacote")]
    public decimal? ValorPacote { get; set; }
    [JsonPropertyName("percentual_comissao_momento")]
    public decimal PercentualComissaoMomento { get; set; }
    public StatusAgendamento Status { get; set; }
    [JsonPropertyName("pago_pelo_paciente")]
    public bool PagoPeloPaciente { get; set; }
    [JsonPropertyName("recorrencia_group_id")]
    public string? RecorrenciaGroupId { get; set; }
    [JsonPropertyName("criado_em")]
    public DateTime CriadoEm { get; set; }
}

public enum StatusAgendamento
{
    AGENDADO,
    REALIZADO,
    FALTA,
    CANCELADO
}