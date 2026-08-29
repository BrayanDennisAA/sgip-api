using Sgip.Domain.Utils;

namespace Sgip.UnitTests;
 
public class FinancialCalculatorTests
{
    [Fact]
    public void CalculateFixedInstallment_ConMontoPlazoYTasaConocidos_CalculaCuotaCorrecta()
    {
        // Monto 10,000 / TEA 24% / 12 meses. TEM ~ 1.807% mensual.
        var tem = FinancialCalculator.CalculateTEM(0.24m);
        var installment = FinancialCalculator.CalculateFixedInstallment(10_000m, tem, 12);

        // La cuota francesa para estos parámetros ronda los 940.
        Assert.InRange(installment, 930m, 950m);
    }

    [Fact]
    public void GenerateFixedSchedule_GeneraNumeroDeCuotasIgualAlPlazoYSaldoFinalCero()
    {
        var schedule = FinancialCalculator.GenerateFixedSchedule(
            amount: 5_000m, teaRate: 0.24m, termMonths: 6, startDate: new DateTime(2026, 1, 15));

        Assert.Equal(6, schedule.Count);
        Assert.Equal(0m, schedule.Last().RemainingBalance);

        // Todas las cuotas (excepto redondeo en la última) deben ser iguales -> cuota fija
        var firstPayment = schedule.First().TotalPayment;
        Assert.All(schedule.Take(5), s => Assert.Equal(firstPayment, s.TotalPayment));
    }

    [Fact]
    public void GetPaymentDate_CuandoDiaBaseEs31YMesTiene30Dias_UsaDia30()
    {
        var startDate = new DateTime(2026, 1, 31);

        var april = FinancialCalculator.GetPaymentDate(startDate, 3); // abril tiene 30 días
        Assert.Equal(30, april.Day);
        Assert.Equal(4, april.Month);
    }
}