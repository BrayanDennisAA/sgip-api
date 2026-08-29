using Sgip.Domain.Entities;

namespace Sgip.Application.Repositories.Interfaces;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id);
    Task<Loan?> GetByIdWithScheduleAsync(Guid id);
    Task<List<Loan>> GetAllAsync(string? userId = null);
    Task<int> CountActiveLoansAsync(string userId);
    Task<decimal> SumActiveMonthlyPaymentsAsync(string userId);
    Task AddAsync(Loan loan);
    Task UpdateAsync(Loan loan);
    Task SaveChangesAsync();
}
