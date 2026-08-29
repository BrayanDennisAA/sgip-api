using Sgip.Domain.Entities;

namespace Sgip.Domain.Utils;

public static class FinancialCalculator
{
    /// <summary>
    /// Convierte una Tasa Efectiva Anual (TEA, ej. 0.24 = 24%) a
    /// Tasa Efectiva Mensual: TEM = (1 + TEA)^(1/12) - 1
    /// </summary>
    public static decimal CalculateTEM(decimal tea)
    {
        double tem = Math.Pow(1 + (double)tea, 1.0 / 12.0) - 1;
        return (decimal)tem;
    }

    /// <summary>
    /// Ajusta la fecha de pago: siempre el mismo día del mes de la fecha base.
    /// Si el mes destino no tiene ese día (ej. base día 31, mes de 30 días),
    /// se usa el último día disponible de ese mes.
    /// </summary>
    public static DateTime GetPaymentDate(DateTime startDate, int monthsAhead)
    {
        var targetMonth = startDate.AddMonths(monthsAhead);
        int lastDayOfMonth = DateTime.DaysInMonth(targetMonth.Year, targetMonth.Month);
        int day = Math.Min(startDate.Day, lastDayOfMonth);
        return new DateTime(targetMonth.Year, targetMonth.Month, day, 0, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Genera el cronograma completo para Sistema Francés (cuota constante).
    /// </summary>
    public static List<PaymentSchedule> GenerateFixedSchedule(
        decimal amount, decimal teaRate, int termMonths, DateTime startDate)
    {
        var tem = CalculateTEM(teaRate);
        var installment = CalculateFixedInstallment(amount, tem, termMonths);

        var schedule = new List<PaymentSchedule>();
        decimal balance = amount;

        for (int i = 1; i <= termMonths; i++)
        {
            decimal interest = Math.Round(balance * tem, 2, MidpointRounding.AwayFromZero);
            decimal principal = i == termMonths
                ? balance // última cuota: absorbe el redondeo acumulado
                : Math.Round(installment - interest, 2, MidpointRounding.AwayFromZero);

            balance = Math.Round(balance - principal, 2, MidpointRounding.AwayFromZero);

            schedule.Add(new PaymentSchedule(
                i,
                GetPaymentDate(startDate, i),
                i == termMonths ? principal + interest : installment,
                principal,
                interest,
                balance < 0 ? 0 : balance
            ));
        }

        return schedule;
    }

    /// <summary>
    /// Cuota fija (Sistema Francés):
    /// Cuota = Monto * [TEM * (1+TEM)^n] / [(1+TEM)^n - 1]
    /// </summary>
    public static decimal CalculateFixedInstallment(decimal amount, decimal tem, int termMonths)
    {
        if (tem == 0) return Math.Round(amount / termMonths, 2);

        double t = (double)tem;
        double factor = Math.Pow(1 + t, termMonths);
        double payment = (double)amount * (t * factor) / (factor - 1);
        return Math.Round((decimal)payment, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Genera el cronograma para Sistema Alemán (amortización constante).
    /// Amortización = Monto / n ; Interés = Saldo * TEM ; Cuota = Amortización + Interés
    /// </summary>
    public static List<PaymentSchedule> GenerateDecreasingSchedule(
        decimal amount, decimal teaRate, int termMonths, DateTime startDate)
    {
        var tem = CalculateTEM(teaRate);
        decimal amortization = Math.Round(amount / termMonths, 2, MidpointRounding.AwayFromZero);

        var schedule = new List<PaymentSchedule>();
        decimal balance = amount;

        for (int i = 1; i <= termMonths; i++)
        {
            decimal interest = Math.Round(balance * tem, 2, MidpointRounding.AwayFromZero);
            decimal principal = i == termMonths ? balance : amortization;
            balance = Math.Round(balance - principal, 2, MidpointRounding.AwayFromZero);

            schedule.Add(new PaymentSchedule
            (i,
                GetPaymentDate(startDate, i),
                principal + interest,
                principal,
                interest,
                balance < 0 ? 0 : balance
            ));
        }

        return schedule;
    }

    public static decimal GetBaseTeaForAmount(decimal amount)
    {
        if (amount < 5_000) return 0.24m;
        if (amount < 20_000) return 0.28m;
        return 0.35m;
    }

}
