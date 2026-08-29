using Sgip.Domain.Enums;

namespace Sgip.Domain.Strategies;
public interface IInstallmentStrategyFactory
{
    IInstallmentStrategy GetStrategy(LoanType type);
}