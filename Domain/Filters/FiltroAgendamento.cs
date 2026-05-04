using Clinica.Domain.Enums;

namespace Clinica.Domain.Filters;

public class FiltroAgendamento
{
    public int? ProfissionalId { get; set; }
    public StatusAgendamento? Status { get; set; }
    public bool ApenasAtrasados { get; set; }
    public bool PagamentoPendente { get; set; }
    public DateTimeOffset? De { get; set; }
    public DateTimeOffset? Ate { get; set; }
}