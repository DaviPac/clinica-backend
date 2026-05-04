using Clinica.Domain.Enums;

namespace Clinica.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Email { get; set; }
    public required string SenhaHash { get; set; }
    public Role Role { get; set; }
    public string? Profissao { get; set; }
    public decimal TaxaComissaoPadrao { get; set; }
    public DateTime CriadoEm { get; set; }
    public ICollection<PacienteProfissional> PacientesAtendidos { get; set; } = [];
    public ICollection<Servico> Servicos { get; set; } = [];
}