using Sgip.Application.DTOs;

namespace Sgip.Application.Services.Interfaces;

public interface ITransactionService
{
    Task<TransactionResponse> CreateAsync(string idempotencyKey, CreateTransactionRequest request);
    Task<List<TransactionResponse>> GetAllAsync(TransactionFilter? filter);
    Task<TransactionResponse?> GetByIdAsync(Guid id);

    Task<TransactionResponse> CreateDisbursementTransactionAsync(string idempotencyKey, CreateTransactionRequest request);
}
