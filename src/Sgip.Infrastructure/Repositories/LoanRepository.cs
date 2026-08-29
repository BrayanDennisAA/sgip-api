using Microsoft.EntityFrameworkCore;
using Sgip.Application.Repositories.Interfaces;
using Sgip.Domain.Entities;
using Sgip.Domain.Enums;
using Sgip.Infrastructure.Data;

namespace Sgip.Infrastructure.Repositories;

public class LoanRepository : ILoanRepository
{
    private readonly ApplicationDbContext _context;

    public LoanRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Loan?> GetByIdAsync(Guid id)
        => await _context.Loans.FirstOrDefaultAsync(l => l.Id == id);

    public async Task<Loan?> GetByIdWithScheduleAsync(Guid id)
        => await _context.Loans
            .Include(l => l.PaymentSchedules.OrderBy(p => p.PaymentNumber))
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<List<Loan>> GetAllAsync(string? userId = null)
    {
        var query = _context.Loans.AsQueryable();
        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(l => l.UserId == userId);

        return await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
    }

    public async Task<int> CountActiveLoansAsync(string userId)
        => await _context.Loans.CountAsync(l =>
            l.UserId == userId &&
            (l.Status == LoanStatus.Active || l.Status == LoanStatus.Approved || l.Status == LoanStatus.Pending));

    public async Task<decimal> SumActiveMonthlyPaymentsAsync(string userId)
        => await _context.Loans
            .Where(l => l.UserId == userId &&
                        (l.Status == LoanStatus.Active || l.Status == LoanStatus.Approved))
            .SumAsync(l => (decimal?)l.MonthlyPayment) ?? 0;

    public async Task AddAsync(Loan loan) => await _context.Loans.AddAsync(loan);

    public Task UpdateAsync(Loan loan)
    {
        _context.Loans.Update(loan);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}