using Sgip.Domain.Enums;

namespace Sgip.Domain.Strategies;

public class InstallmentStrategyFactory : IInstallmentStrategyFactory
{
    private readonly IEnumerable<IInstallmentStrategy> _strategies;

    public InstallmentStrategyFactory(IEnumerable<IInstallmentStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IInstallmentStrategy GetStrategy(LoanType type)
    {
        var strategy = _strategies.FirstOrDefault(s => s.SupportedType == type);
        if (strategy == null)
            throw new NotSupportedException($"No hay estrategia registrada para el tipo de préstamo: {type}");

        return strategy;
    }
}