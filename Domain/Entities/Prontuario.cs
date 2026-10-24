namespace Clinica.Domain.Entities;

public class Prontuario
{
    public int Id { get; set; }
    public int AgendamentoId { get; set; }
    public Agendamento? Agendamento { get; set; }
    public DateTime CriadoEm { get; set; }
    public ICollection<ProntuarioVersao> Versoes { get; set; } = [];
}
