namespace Clinica.Domain.ReadModels;

public record ProntuarioVersaoResumo(
    int Id,
    DateTime CriadoEm,
    int AutorId,
    string NomeAutor
);
