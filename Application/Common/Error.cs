namespace Clinica.Application.Common;

public enum ErrorType
{
    NotFound,
    Unauthorized,
    Validation,
    Conflict,
    Unknown
}

public record Error(string Id, ErrorType Type, string Description);

public static class Errors
{
    public static Error UnknownError { get; } = new("UnknownError", ErrorType.Unknown, "Erro desconhecido");

    public static Error AccountNotFound { get; } = new("UserNotFound", ErrorType.NotFound, "Conta não encontrada.");
    public static Error InvalidCredentials { get; } = new("InvalidCredentials", ErrorType.Unauthorized, "Credenciais inválidas.");
    public static Error EmailAlreadyInUse { get; } = new("EmailAlreadyInUse", ErrorType.Conflict, "Este e-mail já está em uso.");

    public static Error PatientNotFound { get; } = new("PatientNotFound", ErrorType.NotFound, "Paciente não encontrado.");
    public static Error PatientAlreadyLinked { get; } = new("PatientAlreadyLinked", ErrorType.Conflict, "Paciente já vinculado.");

    public static Error ServiceNotFound { get; } = new("ServiceNotFound", ErrorType.NotFound, "Serviço não encontrado.");

    public static Error ConflictingSchedule { get; } = new("ConflictingSchedule", ErrorType.Conflict, "Conflito de horário com agendamento existente.");
    public static Error ScheduleNotFound { get; } = new("ScheduleNotFound", ErrorType.NotFound, "Agendamento não encontrado");

    public static Error ExpenseNotFound { get; } = new("ExpenseNotFound", ErrorType.NotFound, "Despesa não encontrada.");

    public static Error MedicalRecordNotFound { get; } = new("MedicalRecordNotFound", ErrorType.NotFound, "Prontuário não encontrado.");

    public static Error MedicalRecordVersionNotFound { get; } = new("MedicalRecordVersionNotFound", ErrorType.NotFound, "Versão do prontuário não encontrada.");

    public static Error MedicalRecordAlreadyExists { get; } = new("MedicalRecordAlreadyExists", ErrorType.Conflict, "Este agendamento já possui prontuário.");

    public static Error NotesNotFound { get; } = new("NotesNotFound", ErrorType.NotFound, "Anotações não encontradas.");

    public static Error NotesAlreadyExist { get; } = new("NotesAlreadyExist", ErrorType.Conflict, "Este agendamento já possui anotações.");

    public static Error ValidationFailed(string description) => new("ValidationFailed", ErrorType.Validation, description);
}
