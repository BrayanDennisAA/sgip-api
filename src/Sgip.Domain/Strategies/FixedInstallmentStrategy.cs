using Sgip.Domain.Entities;
using Sgip.Domain.Enums;
using Sgip.Domain.Utils;

namespace Sgip.Domain.Strategies;

public class FixedInstallmentStrategy : IInstallmentStrategy
{
    public LoanType SupportedType => LoanType.Fixed;

    public List<PaymentSchedule> GenerateSchedule(decimal amount, decimal teaRate, int termMonths, DateTime startDate)
    {
        return FinancialCalculator.GenerateFixedSchedule(amount, teaRate, termMonths, startDate);
    }

}