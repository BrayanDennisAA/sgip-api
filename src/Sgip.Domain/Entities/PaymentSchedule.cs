using Sgip.Domain.Enums;
using Sgip.Domain.Exceptions;

namespace Sgip.Domain.Entities;

public class PaymentSchedule
{
    public Guid Id { get; private set; }
    public Guid LoanId { get; private set; }
    public int PaymentNumber { get; private set; }
    public DateTime DueDate { get; private set; }
    public decimal TotalPayment { get; private set; }
    public decimal Principal { get; private set; }
    public decimal Interest { get; private set; }
    public decimal RemainingBalance { get; private set; }
    public PaymentStatus Status { get; private set; }

    public Loan? Loan { get; private set; }

    private PaymentSchedule() { }

    public PaymentSchedule(Guid loanId, int paymentNumber, DateTime dueDate, decimal totalPayment, decimal principal, decimal interest, decimal remainingBalance)
    {
        Id = Guid.NewGuid();
        LoanId = loanId;
        PaymentNumber = paymentNumber;
        DueDate = dueDate;
        TotalPayment = totalPayment;
        Principal = principal;
        Interest = interest;
        RemainingBalance = remainingBalance;
        Status = PaymentStatus.Pending;
    }

    public PaymentSchedule (int paymentNumber, DateTime dueDate, decimal totalPayment, decimal principal, decimal interest, decimal remainingBalance)
    {
        PaymentNumber = paymentNumber;
        DueDate = dueDate;
        TotalPayment = totalPayment;
        Principal = principal;
        Interest = interest;
        RemainingBalance = remainingBalance;
        Status = PaymentStatus.Pending;
    }

    public void MarkAsPaid()
    {
        if (Status == PaymentStatus.Paid)
            throw new BusinessRuleException("El pago ya ha sido marcado como pagado.");

        Status = PaymentStatus.Paid;
    }

    public void AgragateLoanId(Guid loanId)
    {
        LoanId = loanId;
    }
}
