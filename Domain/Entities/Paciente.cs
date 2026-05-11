namespace Clinica.Domain.Entities;

public class Paciente
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public string? Cpf { get; set; }
    public string? Telefone { get; set; }
    public DateOnly? DataNascimento { get; set; }
    public DateTime CriadoEm { get; set; }
    public bool Ativo { get; set; } = true;
    public string? EnderecoCompleto { get; set; }
    public string? Rg { get; set; }
    public ICollection<PacienteProfissional> ProfissionaisVinculados { get; set; } = [];
}