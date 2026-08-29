using Sgip.Domain.Entities;
using Sgip.Domain.Enums;

namespace Sgip.Domain.Strategies;

public interface IInstallmentStrategy
{
    LoanType SupportedType { get; }

    List<PaymentSchedule> GenerateSchedule(
        decimal amount, decimal teaRate, int termMonths, DateTime startDate);
}