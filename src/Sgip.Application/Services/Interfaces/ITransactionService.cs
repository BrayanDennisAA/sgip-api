using Sgip.Application.DTOs;

namespace Sgip.Application.Services.Interfaces;

public interface ITransactionService
{
    Task<TransactionResponse> CreateAsync(CreateTransactionRequest request);
    Task<List<TransactionResponse>> GetAllAsync(TransactionFilter? filter);
    Task<TransactionResponse?> GetByIdAsync(Guid id);

    Task<TransactionResponse> CreateDisbursementTransactionAsync(CreateTransactionRequest request);
}
