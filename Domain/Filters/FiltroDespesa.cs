using Clinica.Domain.Enums;

namespace Clinica.Domain.Filters;

public class FiltroDespesa
{
    public DateOnly? De { get; set; }
    public DateOnly? Ate { get; set; }
    public bool? Pago { get; set; }
    public CategoriaDespesa? Categoria { get; set; }
}