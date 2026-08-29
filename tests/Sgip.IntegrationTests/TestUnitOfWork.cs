using Sgip.Application.Interfaces;
using Sgip.Infrastructure.Data;

namespace Sgip.IntegrationTests;

public class TestUnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public TestUnitOfWork(ApplicationDbContext context) => _context = context;

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

    public async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        // Sin BeginTransactionAsync real: solo ejecuta la operación tal cual.
        await operation();
    }
}