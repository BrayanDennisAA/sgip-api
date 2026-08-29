using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Sgip.Application.DTOs;
using Sgip.Application.Services;
using Sgip.Domain.Enums;
using Sgip.Domain.Exceptions;
using Sgip.Infrastructure.Data;
using Sgip.Infrastructure.Repositories;

namespace Sgip.IntegrationTests;

public class IdempotencyTests
{
    private static TransactionService CreateService(out ApplicationDbContext context)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        context = new ApplicationDbContext(options);
        var repo = new TransactionRepository(context);
        return new TransactionService(repo, NullLogger<TransactionService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_ConMismoIdempotencyKeyDosVeces_NoDuplicaYRetornaLaOriginal()
    {
        var service = CreateService(out var context);
        const string idempotencyKey = "pay-btn-click-abc123";

        var request = new CreateTransactionRequest
        {
            Type = TransactionType.Payment,
            Amount = 250m,
            Description = "Pago de cuota mensual"
        };

        // Simula el usuario presionando "Pagar" dos veces rápido
        var first = await service.CreateAsync(idempotencyKey, request);
        var second = await service.CreateAsync(idempotencyKey, request);

        // Debe ser la MISMA transacción (mismo Id), no una nueva
        Assert.Equal(first.Id, second.Id);
        Assert.False(first.WasDeduplicated);
        Assert.True(second.WasDeduplicated);

        // Y en la base de datos solo debe existir un único registro
        var count = await context.Transactions.CountAsync(t => t.IdempotencyKey == idempotencyKey);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CreateAsync_ConIdempotencyKeyVacio_LanzaBusinessRuleException()
    {
        var service = CreateService(out _);
        var request = new CreateTransactionRequest { Type = TransactionType.Payment, Amount = 100m };

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CreateAsync(string.Empty, request));
    }

    [Fact]
    public async Task CreateAsync_ConKeysDiferentes_CreaTransaccionesSeparadas()
    {
        var service = CreateService(out var context);
        var request = new CreateTransactionRequest { Type = TransactionType.Payment, Amount = 100m };

        var first = await service.CreateAsync("key-1", request);
        var second = await service.CreateAsync("key-2", request);

        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(2, await context.Transactions.CountAsync());
    }
}