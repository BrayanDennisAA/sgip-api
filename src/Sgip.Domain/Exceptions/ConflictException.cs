/// <summary>
/// Conflicto de estado: la operación es válida en general, pero el recurso
/// no está en el estado correcto para permitirla ahora mismo. Mapea a HTTP 409.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}