namespace Sgip.Domain.Exceptions;

/// <summary>
/// Representa una excepción que se lanza cuando se viola una regla de negocio en la aplicación.
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }

}
