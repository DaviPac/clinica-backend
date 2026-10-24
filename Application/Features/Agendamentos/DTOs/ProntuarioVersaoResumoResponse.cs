namespace Clinica.Application.Features.Agendamentos.DTOs;

public record ProntuarioVersaoResumoResponse(
    int VersaoId,
    int AutorId,
    string Autor,
    DateTime CriadoEm
);
