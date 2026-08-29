
using Sgip.Domain;
using Sgip.Domain.Enums;

namespace Sgip.Application.DTOs;

public class CreateTransactionRequest
{
    // TODO: quitar debe venir del header de la petición, no del body. Se recibe explícitamente para no requerir autenticación/perfil (ver README).
    public string IdempotencyKey { get; set; } = string.Empty;

    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public Guid? LoanId { get; set; }

    public string? Description { get; set; }
}

public class TransactionResponse
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? LoanId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    // True si esta respuesta corresponde a una transacción ya existente
    // (petición duplicada detectada por idempotency_key), no una nueva.
    public bool WasDeduplicated { get; set; }
}

public class TransactionFilter
{
    public TransactionType? Type { get; set; }
    public TransactionStatus? Status { get; set; }
    public Guid? LoanId { get; set; }
}
