using System.Text.Json.Serialization;

namespace Clinica.Domain;

public class Paciente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string? Telefone { get; set; }
    [JsonPropertyName("data_nascimento")]
    public DateOnly? DataNascimento { get; set; }
    public DateTime CriadoEm { get; set; }

    [JsonIgnore]
    public ICollection<PacienteProfissional> ProfissionaisVinculados { get; set; } = [];
}