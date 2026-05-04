using Clinica.Domain.Enums;

namespace Clinica.Domain.Entities;

public class Agendamento
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public Paciente? Paciente { get; set; }
    public int ProfissionalId { get; set; }
    public Usuario? Profissional { get; set; }
    public int ServicoId { get; set; }
    public Servico? Servico { get; set; }
    public DateTimeOffset DataHoraInicio { get; set; }
    public DateTimeOffset DataHoraFim { get; set; }
    public decimal ValorCombinado { get; set; }
    public decimal? ValorPacote { get; set; }
    public decimal PercentualComissaoMomento { get; set; }
    public StatusAgendamento Status { get; set; }
    public bool PagoPeloPaciente { get; set; }
    public string? RecorrenciaGroupId { get; set; }
    public DateTime CriadoEm { get; set; }
}