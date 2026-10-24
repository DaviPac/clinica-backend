namespace Clinica.Domain.Entities;

public class AnotacoesAgendamento
{
    public int Id { get; set; }
    public int AgendamentoId { get; set; }
    public Agendamento? Agendamento { get; set; }
    public required string Conteudo { get; set; }
}
