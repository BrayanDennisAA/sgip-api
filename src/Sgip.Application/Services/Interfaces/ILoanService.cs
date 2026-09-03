using Sgip.Application.DTOs;
using Sgip.Domain.Common;

namespace Sgip.Application.Services.Interfaces;

public interface ILoanService
{
    Result<SimulateLoanResponse> Simulate(SimulateLoanRequest request);
    Task<Result<LoanResponse>> CreateAsync(CreateLoanRequest request);
    Task<List<LoanResponse>> GetAllAsync(string? userId);
    Task<Result<LoanResponse>> GetByIdAsync(Guid id);
    Task<Result<LoanDetailResponse>> GetScheduleAsync(Guid id);
    Task<Result<LoanResponse>> ApproveAsync(Guid id);
    Task<Result<LoanResponse>> RejectAsync(Guid id);
}
