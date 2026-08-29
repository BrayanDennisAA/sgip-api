using Sgip.Application.DTOs;

namespace Sgip.Application.Services.Interfaces;

public interface ILoanService
{
    SimulateLoanResponse Simulate(SimulateLoanRequest request);
    Task<LoanResponse> CreateAsync(CreateLoanRequest request);
    Task<List<LoanResponse>> GetAllAsync(string? userId);
    Task<LoanResponse?> GetByIdAsync(Guid id);
    Task<LoanDetailResponse?> GetScheduleAsync(Guid id);
    Task<LoanResponse?> ApproveAsync(Guid id);
    Task<LoanResponse?> RejectAsync(Guid id);
}
