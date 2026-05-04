namespace Clinica.Domain.Entities;

public class Servico
{
    public int Id { get; set; }
    public int ProfissionalId { get; set; }
    public required Usuario Profissional { get; set; }
    public required string Nome { get; set; }
    public decimal ValorAtual { get; set; }
    public bool Ativo { get; set; } = true;
    public bool IsPacote { get; set; } = false;
}