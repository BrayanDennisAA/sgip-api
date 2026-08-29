using Sgip.Domain.Entities;
using Sgip.Domain.Enums;
using Sgip.Domain.Utils;

namespace Sgip.Domain.Strategies;

public class DecreasingInstallmentStrategy : IInstallmentStrategy
{
    public LoanType SupportedType => LoanType.Decreasing;

    public List<PaymentSchedule> GenerateSchedule(decimal amount, decimal teaRate, int termMonths, DateTime startDate)
    {
        return FinancialCalculator.GenerateDecreasingSchedule(amount, teaRate, termMonths, startDate);
    }
}