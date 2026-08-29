using Microsoft.AspNetCore.Mvc;
using Sgip.Application.DTOs;
using Sgip.Application.Services.Interfaces;
using Sgip.Domain.Exceptions;

namespace Sgip.WebApi.Controllers;

[ApiController]
[Route("api/loans")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    /// <summary>Simula un préstamo sin guardarlo. Retorna cuota, TEA/TEM y cronograma.</summary>
    [HttpPost("simulate")]
    [ProducesResponseType(typeof(SimulateLoanResponse), StatusCodes.Status200OK)]
    public IActionResult Simulate([FromBody] SimulateLoanRequest request)
    {
        var simulationResult = _loanService.Simulate(request);
        return Ok(simulationResult);
    }

    // <summary>Crea una solicitud de préstamo (queda en Pending o se auto-aprueba).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateLoanRequest request)
    {
        var loan = await _loanService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
    }

    /// <summary>Lista préstamos, opcionalmente filtrados por userId.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<LoanResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? userId)
        => Ok(await _loanService.GetAllAsync(userId));

    /// <summary>Obtiene un préstamo por Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var loan = await _loanService.GetByIdAsync(id);
        return loan == null ? NotFound() : Ok(loan);
    }

    /// <summary>Obtiene el cronograma completo de pagos de un préstamo.</summary>
    [HttpGet("{id:guid}/schedule")]
    [ProducesResponseType(typeof(LoanDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSchedule(Guid id)
    {
        var loan = await _loanService.GetScheduleAsync(id);
        return loan == null ? NotFound() : Ok(loan);
    }

    /// <summary>Aprueba un préstamo Pending, lo pasa a Active y crea la transacción de desembolso.</summary>
    [HttpPatch("{id:guid}/approve")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid id)
    {
        try
        {
            var loan = await _loanService.ApproveAsync(id);
            return loan == null ? NotFound() : Ok(loan);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Rechaza un préstamo Pending.</summary>
    [HttpPatch("{id:guid}/reject")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(Guid id)
    {
        try
        {
            var loan = await _loanService.RejectAsync(id);
            return loan == null ? NotFound() : Ok(loan);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}