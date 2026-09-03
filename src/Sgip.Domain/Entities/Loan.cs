using Sgip.Domain.Common;
using Sgip.Domain.Enums;
using Sgip.Domain.Exceptions;

namespace Sgip.Domain.Entities;

public class Loan
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = String.Empty;
    public decimal Amount { get; private set; }
    public int Term { get; private set; }
    public decimal InterestRate { get; private set; }
    public LoanType LoanType { get; private set; }
    public LoanStatus Status { get; private set; }
    public decimal MonthlyPayment { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<PaymentSchedule> PaymentSchedules { get; private set; } = new List<PaymentSchedule>();
    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();

    private Loan() { }

    public Loan(string userId, decimal amount, int term, decimal interestRate, LoanType loanType, decimal monthlyPayment)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Amount = amount;
        Term = term;
        InterestRate = interestRate;
        LoanType = loanType;
        MonthlyPayment = monthlyPayment;

        Status = LoanStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
    }

    public Loan(string userId, decimal amount, int term, decimal interestRate, LoanType loanType, decimal monthlyPayment, LoanStatus status)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Amount = amount;
        Term = term;
        InterestRate = interestRate;
        LoanType = loanType;
        MonthlyPayment = monthlyPayment;

        Status = status;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = null;
    }

    public Result Approve()
    {
        if (Status != LoanStatus.Pending)
            return Result.Failure(Error.Conflict("Solo se pueden aprobar préstamos que estén en estado pendiente."));

        Status = LoanStatus.Approved;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Reject()
    {
        if (Status != LoanStatus.Pending)
            Result.Failure(Error.Conflict("Solo se pueden rechazar préstamos que estén en estado pendiente."));

        Status = LoanStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
