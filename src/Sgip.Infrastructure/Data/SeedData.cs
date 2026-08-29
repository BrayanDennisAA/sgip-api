using Microsoft.EntityFrameworkCore;
using Sgip.Domain;
using Sgip.Domain.Entities;
using Sgip.Domain.Enums;
using Sgip.Domain.Utils;

namespace Sgip.Infrastructure.Data;

public static class SeedData
{
    public static async Task ApplyAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Loans.AnyAsync()) return; // ya hay datos, no volver a sembrar

        var startDate = DateTime.UtcNow.Date.AddMonths(-1);
        var tea = FinancialCalculator.GetBaseTeaForAmount(8000);
        var schedule = FinancialCalculator.GenerateFixedSchedule(8000, tea, 12, startDate);

        var loan = new Loan
        (
            "user-123",
            8000,
            12,
            tea,
            LoanType.Fixed,
            schedule.First().TotalPayment,
            LoanStatus.Active
        );

        foreach (var item in schedule)
        {
            item.AgragateLoanId(loan.Id); 
            loan.PaymentSchedules.Add(item);
        }

        context.Loans.Add(loan);

        context.Transactions.Add(new Transaction
        (
             $"disbursement-{loan.Id}",
             TransactionType.Disbursement,
             loan.Amount,
             TransactionStatus.Completed,
             loan.Id,
             "Desembolso inicial (seed data)"
        ));

        await context.SaveChangesAsync();
    }
}