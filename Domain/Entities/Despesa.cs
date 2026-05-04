using Clinica.Domain.Enums;

namespace Clinica.Domain.Entities;

public class DespesaClinica
{
    public int Id { get; set; }
    public required string Descricao { get; set; }
    public decimal Valor { get; set; }
    public DateOnly DataVencimento { get; set; }
    public bool StatusPagamento { get; set; }
    public CategoriaDespesa Categoria { get; set; }
    public DateTime CriadoEm { get; set; }
}