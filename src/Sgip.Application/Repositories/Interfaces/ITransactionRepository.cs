using Sgip.Application.DTOs;
using Sgip.Domain.Entities;

namespace Sgip.Application.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<Transaction?> GetByIdempotencyKeyAsync(string idempotencyKey);
    Task<List<Transaction>> GetAllAsync(TransactionFilter? filter = null);
    Task AddAsync(Transaction transaction);
    Task SaveChangesAsync();
}