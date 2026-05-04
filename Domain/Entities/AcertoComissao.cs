namespace Clinica.Domain.Entities;

public class AcertoComissao
{
    public int Id { get; set; }

    public int ProfissionalId { get; set; }
    public Usuario? Profissional { get; set; }

    public required string PeriodoReferencia { get; set; }
    public decimal ValorPago { get; set; }
    public DateTimeOffset DataPagamento { get; set; }
    public string? Observacao { get; set; }
}