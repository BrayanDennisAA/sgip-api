using Microsoft.AspNetCore.Mvc;
using Sgip.Application.DTOs;
using Sgip.Application.Services.Interfaces;
using Sgip.Domain;
using Sgip.Domain.Enums;
using Sgip.Domain.Exceptions;
using Sgip.WebApi.Common;

namespace Sgip.WebApi.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Crea una transacción. Si idempotency_key ya existe, retorna la transacción
    /// original (200) en lugar de crear una nueva (201).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromBody] CreateTransactionRequest request)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Regla de negocio violada",
                detail: "El header 'Idempotency-Key' es requerido.");

        var result = await _transactionService.CreateAsync(idempotencyKey, request);

        return result.ToActionResult(tx =>
           tx.WasDeduplicated
               ? Ok(tx)                                                 // ya existía
               : CreatedAtAction(nameof(GetById), new { id = tx.Id }, tx)); // nueva

    }

    /// <summary>Lista transacciones con filtros opcionales por tipo, estado o préstamo.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TransactionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] TransactionType? type,
        [FromQuery] TransactionStatus? status,
        [FromQuery] Guid? loanId)
    {
        var filter = new TransactionFilter { Type = type, Status = status, LoanId = loanId };
        return Ok(await _transactionService.GetAllAsync(filter));
    }

    /// <summary>Obtiene una transacción por Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return (await _transactionService.GetByIdAsync(id)).ToActionResult();
    }
}