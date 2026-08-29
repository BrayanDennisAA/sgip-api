using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Sgip.Application.DTOs;
using Sgip.Application.Services;
using Sgip.Domain.Enums;
using Sgip.Domain.Exceptions;
using Sgip.Domain.Strategies;
using Sgip.Infrastructure.Data;
using Sgip.Infrastructure.Repositories;

namespace Sgip.IntegrationTests;

public class LoanServiceTests
{
    private static LoanService CreateService(
        out ApplicationDbContext context)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        context = new ApplicationDbContext(options);

        var loanRepo = new LoanRepository(context);
        var transactionService = new TransactionService(
            new TransactionRepository(context), NullLogger<TransactionService>.Instance);

        var strategyFactory = new InstallmentStrategyFactory(new IInstallmentStrategy[]
        {
            new FixedInstallmentStrategy(),
            new DecreasingInstallmentStrategy()
        });


        return new LoanService(
            loanRepo,
            transactionService,
            strategyFactory,
            NullLogger<LoanService>.Instance);
    }

    [Theory]
    [InlineData(499)]      // por debajo del mínimo ($500)
    [InlineData(50_001)]   // por encima del máximo ($50,000)
    public async Task CreateAsync_ConMontoFueraDeRango_LanzaBusinessRuleException(decimal invalidAmount)
    {
        var service = CreateService(out _);
        var request = new CreateLoanRequest
        {
            UserId = "user-test",
            Amount = invalidAmount,
            Term = 12,
            LoanType = LoanType.Fixed,
            MonthlyIncome = 10_000
        };

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_ConMontoBajoYPocosPrestamos_SeAprueboAutomaticamente()
    {
        var service = CreateService(out _);
        var request = new CreateLoanRequest
        {
            UserId = "user-auto-approve",
            Amount = 5_000, // < 10,000
            Term = 12,
            LoanType = LoanType.Fixed,
            MonthlyIncome = 20_000
        };

        var loan = await service.CreateAsync(request);

        Assert.Equal("Approved", loan.Status);
    }
}