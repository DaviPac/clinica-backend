namespace Clinica.Domain;

public class PacienteProfissional
{
    public int PacienteId { get; set; }
    public required Paciente Paciente { get; set; }

    public int ProfissionalId { get; set; }
    public required Usuario Profissional { get; set; }

    public DateTime CriadoEm { get; set; }
}