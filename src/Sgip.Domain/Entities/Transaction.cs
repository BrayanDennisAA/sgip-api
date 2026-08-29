using Sgip.Domain.Enums;

namespace Sgip.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }

    public string IdempotencyKey { get; private set; } = string.Empty;

    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionStatus Status { get; private set; }

    public Guid? LoanId { get; private set; }

    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Loan? Loan { get; private set; }

    private Transaction() { }

    public Transaction(string idempotencyKey, TransactionType type, decimal amount, TransactionStatus status, Guid? loanId = null, string? description = null)
    {
        Id = Guid.NewGuid();
        IdempotencyKey = idempotencyKey;
        Type = type;
        Amount = amount;
        Status = status;
        LoanId = loanId;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public Transaction(string idempotencyKey, TransactionType type, decimal amount, Guid? loanId = null, string? description = null)
    {
        Id = Guid.NewGuid();
        IdempotencyKey = idempotencyKey;
        Type = type;
        Amount = amount;
        Status = TransactionStatus.Completed; // simplificado: se procesa síncronamente
        LoanId = loanId;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

}
