using Sgip.Application.DTOs;
using Sgip.Domain.Common;

namespace Sgip.Application.Services.Interfaces;

public interface ITransactionService
{
    Task<Result<TransactionResponse>> CreateAsync(string idempotencyKey, CreateTransactionRequest request);
    Task<List<TransactionResponse>> GetAllAsync(TransactionFilter? filter);
    Task<Result<TransactionResponse>> GetByIdAsync(Guid id);

    Task<TransactionResponse> CreateDisbursementTransactionAsync(string idempotencyKey, CreateTransactionRequest request);
}
