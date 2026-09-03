namespace Sgip.Domain.Common;

public enum ErrorType
{
    Validation,   // -> 422: la operación viola una regla de negocio
    Conflict,     // -> 409: el recurso no está en el estado correcto
    NotFound,     // -> 404: el recurso no existe
}

public sealed class Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }

    private Error(string code, string message, ErrorType type)
    {
        Code = code;
        Message = message;
        Type = type;
    }

    public static Error Validation(string message, string code = "validation_error")
        => new(code, message, ErrorType.Validation);

    public static Error Conflict(string message, string code = "conflict")
        => new(code, message, ErrorType.Conflict);

    public static Error NotFound(string message, string code = "not_found")
        => new(code, message, ErrorType.NotFound);
}