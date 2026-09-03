using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Sgip.Application.DTOs;
using Sgip.Application.Services;
using Sgip.Domain.Common;
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
        var unitOfWork = new TestUnitOfWork(context);

        var strategyFactory = new InstallmentStrategyFactory(new IInstallmentStrategy[]
        {
            new FixedInstallmentStrategy(),
            new DecreasingInstallmentStrategy()
        });


        return new LoanService(
            loanRepo,
            transactionService,
            strategyFactory,
            unitOfWork,
            NullLogger<LoanService>.Instance);
    }

    [Fact]
    public async Task ApproveAsync_CambiaEstadoAActiveYCreaTransaccionDeDesembolsoJuntos()
    {
        var service = CreateService(out var context);

        var loan = await service.CreateAsync(new CreateLoanRequest
        {
            UserId = "user-approve-manual",
            Amount = 15_000, // >= 10,000 -> no se auto-aprueba, queda Pending
            Term = 24,
            LoanType = LoanType.Fixed,
            MonthlyIncome = 30_000
        });

        Assert.Equal("Pending", loan.Value!.Status);

        var approved = await service.ApproveAsync(loan.Value.Id);

        Assert.NotNull(approved);
        Assert.Equal("Approved", approved.Value!.Status);

        var disbursement = await context.Transactions
            .FirstOrDefaultAsync(t => t.LoanId == loan.Value.Id && t.Type == TransactionType.Disbursement);

        Assert.NotNull(disbursement);
        Assert.Equal(loan.Value.Amount, disbursement!.Amount);
    }

    // Pruebas Unitarias para validar reglas de negocio. (TODO: mover a un archivo de pruebas unitarias)

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

        var result = await service.CreateAsync(request);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Theory]
    [InlineData(5)]   // por debajo del mínimo (6 meses)
    [InlineData(61)]  // por encima del máximo (60 meses)
    public async Task CreateAsync_ConPlazoFueraDeRango_LanzaBusinessRuleException(int invalidTerm)
    {
        var service = CreateService(out _);
        var request = new CreateLoanRequest
        {
            UserId = "user-test",
            Amount = 5_000,
            Term = invalidTerm,
            LoanType = LoanType.Fixed,
            MonthlyIncome = 10_000
        };

        var result = await service.CreateAsync(request);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }
}