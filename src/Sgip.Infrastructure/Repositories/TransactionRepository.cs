using Microsoft.EntityFrameworkCore;
using Sgip.Application.DTOs;
using Sgip.Application.Repositories.Interfaces;
using Sgip.Domain.Entities;
using Sgip.Infrastructure.Data;

namespace Sgip.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
        => await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Transaction?> GetByIdempotencyKeyAsync(string idempotencyKey)
        => await _context.Transactions.FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey);

    public async Task<List<Transaction>> GetAllAsync(TransactionFilter? filter = null)
    {
        var query = _context.Transactions.AsQueryable();

        if (filter != null)
        {
            if (filter.Type.HasValue)
                query = query.Where(t => t.Type == filter.Type.Value);
            if (filter.Status.HasValue)
                query = query.Where(t => t.Status == filter.Status.Value);
            if (filter.LoanId.HasValue)
                query = query.Where(t => t.LoanId == filter.LoanId.Value);
        }

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async Task AddAsync(Transaction transaction) => await _context.Transactions.AddAsync(transaction);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}