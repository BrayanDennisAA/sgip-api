using Microsoft.EntityFrameworkCore;
using Sgip.Application.Interfaces;

namespace Sgip.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context) => _context = context;

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

    public async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
  
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteInTransactionAsync(
            operation: async () => await operation(),
            verifySucceeded: async () =>
            {
                // Opcional: verifica si la transacción tuvo éxito en caso de desconexión catastrófica
                return _context.Database.CurrentTransaction != null;
            }
        );
    }
}