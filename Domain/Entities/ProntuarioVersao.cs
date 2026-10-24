namespace Clinica.Domain.Entities;

public class ProntuarioVersao
{
    public int Id { get; set; }
    public int ProntuarioId { get; set; }
    public Prontuario? Prontuario { get; set; }
    public required string Conteudo { get; set; }
    public int AutorId { get; set; }
    public Usuario? Autor { get; set; }
    public DateTime CriadoEm { get; set; }
}
