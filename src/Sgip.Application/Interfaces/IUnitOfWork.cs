namespace Sgip.Application.Interfaces;
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
    Task ExecuteInTransactionAsync(Func<Task> operation);
}