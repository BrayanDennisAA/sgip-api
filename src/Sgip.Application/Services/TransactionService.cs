using Microsoft.Extensions.Logging;
using Sgip.Application.DTOs;
using Sgip.Application.Repositories.Interfaces;
using Sgip.Application.Services.Interfaces;
using Sgip.Domain;
using Sgip.Domain.Entities;
using Sgip.Domain.Exceptions;

namespace Sgip.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;

    private readonly ILogger<TransactionService> _logger;

    public TransactionService(ITransactionRepository transactionRepository, ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task<TransactionResponse> CreateAsync(CreateTransactionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            throw new BusinessRuleException("idempotency_key es requerido.");

        // 1) Si ya existe una transacción con esta clave, se retorna la original
        //    (nunca se crea una nueva ni se reprocesa).
        var existing = await _transactionRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey);
        if (existing != null)
        {
            _logger.LogInformation("Transacción con idempotency_key {IdempotencyKey} ya existe. Retornando la transacción existente.", request.IdempotencyKey);
            return MapResponse(existing, wasDeduplicated: true);
        }

        var transaction = new Transaction
        (
            request.IdempotencyKey,
            request.Type,
            request.Amount,
            request.LoanId
        );

        await _transactionRepository.AddAsync(transaction);
        await _transactionRepository.SaveChangesAsync();
        

        return MapResponse(transaction, wasDeduplicated: false);
    }

    public async Task<List<TransactionResponse>> GetAllAsync(TransactionFilter? filter)
    {
        var transactions = await _transactionRepository.GetAllAsync(filter);
        return transactions.Select(t => MapResponse(t, wasDeduplicated: false)).ToList();
    }

    public async Task<TransactionResponse?> GetByIdAsync(Guid id)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        return transaction == null ? null : MapResponse(transaction, wasDeduplicated: false);
    }

    public async Task<TransactionResponse> CreateDisbursementTransactionAsync(CreateTransactionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            throw new BusinessRuleException("idempotency_key es requerido.");

        // 1) Si ya existe una transacción con esta clave, se retorna la original
        //    (nunca se crea una nueva ni se reprocesa).
        var existing = await _transactionRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey);
        if (existing != null)
        {
            return MapResponse(existing, wasDeduplicated: true);
        }

        var transaction = new Transaction
        (
            request.IdempotencyKey,
            request.Type,
            request.Amount,
            TransactionStatus.Completed,
            request.LoanId,
            $"Desembolso automático del préstamo {request.LoanId}"
        );

        await _transactionRepository.AddAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        _logger.LogInformation("Transacción de desembolso creada con idempotency_key {IdempotencyKey} para el préstamo {LoanId}.", request.IdempotencyKey, request.LoanId);

        return MapResponse(transaction, wasDeduplicated: false);
    }

    private static TransactionResponse MapResponse(Transaction t, bool wasDeduplicated) => new()
    {
        Id = t.Id,
        IdempotencyKey = t.IdempotencyKey,
        Type = t.Type.ToString(),
        Amount = t.Amount,
        Status = t.Status.ToString(),
        LoanId = t.LoanId,
        Description = t.Description,
        CreatedAt = t.CreatedAt,
        WasDeduplicated = wasDeduplicated
    };
}