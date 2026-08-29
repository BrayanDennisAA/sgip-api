using Microsoft.Extensions.Logging;
using Sgip.Application.DTOs;
using Sgip.Application.Interfaces;
using Sgip.Application.Repositories.Interfaces;
using Sgip.Application.Services.Interfaces;
using Sgip.Domain.Entities;
using Sgip.Domain.Enums;
using Sgip.Domain.Exceptions;
using Sgip.Domain.Strategies;
using Sgip.Domain.Utils;

namespace Sgip.Application.Services;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly ITransactionService _transactionService;
    private readonly IInstallmentStrategyFactory _installmentStrategyFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoanService> _logger;

    private const decimal MinAmount = 500m;
    private const decimal MaxAmount = 50_000m;
    private const int MinTerm = 6;
    private const int MaxTerm = 60;
    private const int MaxActiveLoans = 3;
    private const decimal MaxDebtToIncomeRatio = 0.40m;
    private const decimal AutoApprovalAmountLimit = 10_000m;
    private const int AutoApprovalMaxActiveLoans = 2;

    public LoanService(
        ILoanRepository loanRepository,
        ITransactionService transactionService,
        IInstallmentStrategyFactory installmentStrategyFactory,
        IUnitOfWork unitOfWork,
        ILogger<LoanService> logger)
    {
        _loanRepository = loanRepository;
        _transactionService= transactionService;
        _installmentStrategyFactory = installmentStrategyFactory;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public SimulateLoanResponse Simulate(SimulateLoanRequest request)
    {
        ValidateAmountAndTerm(request.Amount, request.Term);

        var strategy = _installmentStrategyFactory.GetStrategy(request.LoanType);
        var tea = FinancialCalculator.GetBaseTeaForAmount(request.Amount);
        var schedule = strategy.GenerateSchedule(request.Amount, tea, request.Term, DateTime.UtcNow.Date);

        return new SimulateLoanResponse
        {
            Amount = request.Amount,
            Term = request.Term,
            TeaRate = tea,
            TemRate = FinancialCalculator.CalculateTEM(tea),
            LoanType = request.LoanType.ToString(),
            MonthlyPayment = schedule.First().TotalPayment,
            Schedule = schedule.Select(MapScheduleItem).ToList()
        };
    }

    public async Task<LoanResponse> CreateAsync(CreateLoanRequest request)
    {
        ValidateAmountAndTerm(request.Amount, request.Term);

        // Regla: máximo 3 préstamos activos (pending/approved/active) por cliente
        var activeCount = await _loanRepository.CountActiveLoansAsync(request.UserId);
        if (activeCount >= MaxActiveLoans)
            throw new BusinessRuleException(
                $"El cliente ya tiene {activeCount} préstamos activos. Máximo permitido: {MaxActiveLoans}.");

        var tea = FinancialCalculator.GetBaseTeaForAmount(request.Amount);
        var startDate = DateTime.UtcNow.Date;
        var strategy = _installmentStrategyFactory.GetStrategy(request.LoanType);
        var schedule = strategy.GenerateSchedule(request.Amount, tea, request.Term, startDate);
        var monthlyPayment = schedule.First().TotalPayment;

        // Regla: la suma de cuotas de todos sus préstamos no puede exceder el 40% de sus ingresos
        var currentMonthlyDebt = await _loanRepository.SumActiveMonthlyPaymentsAsync(request.UserId);
        var projectedDebt = currentMonthlyDebt + monthlyPayment;
        if (request.MonthlyIncome > 0 && projectedDebt > request.MonthlyIncome * MaxDebtToIncomeRatio)
        {
            throw new BusinessRuleException(
                "La cuota mensual proyectada excede el 40% de los ingresos declarados del cliente.");
        }

        var loan = new Loan(
            request.UserId,
            request.Amount,
            request.Term,
            tea,
            request.LoanType,
            monthlyPayment);

        foreach (var item in schedule)
        {
            item.AgragateLoanId(loan.Id);
            loan.PaymentSchedules.Add(item);
        }

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await _loanRepository.AddAsync(loan);
            if (request.Amount < AutoApprovalAmountLimit && activeCount < AutoApprovalMaxActiveLoans)
            {
                loan.Approve();
                await _loanRepository.UpdateAsync(loan);

                await _transactionService.CreateDisbursementTransactionAsync($"disbursement-{loan.Id}", new CreateTransactionRequest
                {
                    Type = TransactionType.Disbursement,
                    Amount = loan.Amount,
                    LoanId = loan.Id
                });

                _logger.LogInformation("Préstamo {LoanId} auto-aprobado (scoring simulado)", loan.Id);
            }

            await _unitOfWork.SaveChangesAsync();

        });

        return MapLoanResponse(loan);

    }

    public async Task<List<LoanResponse>> GetAllAsync(string? userId)
    {
        var loans = await _loanRepository.GetAllAsync(userId);
        return loans.Select(MapLoanResponse).ToList();
    }

    public async Task<LoanResponse?> GetByIdAsync(Guid id)
    {
        var loan = await _loanRepository.GetByIdAsync(id);
        return loan == null ? null : MapLoanResponse(loan);
    }

    public async Task<LoanDetailResponse?> GetScheduleAsync(Guid id)
    {
        var loan = await _loanRepository.GetByIdWithScheduleAsync(id);
        if (loan == null) return null;

        var response = MapLoanResponse(loan);
        return new LoanDetailResponse
        {
            Id = response.Id,
            UserId = response.UserId,
            Amount = response.Amount,
            Term = response.Term,
            InterestRate = response.InterestRate,
            LoanType = response.LoanType,
            Status = response.Status,
            MonthlyPayment = response.MonthlyPayment,
            CreatedAt = response.CreatedAt,
            UpdatedAt = response.UpdatedAt,
            Schedule = loan.PaymentSchedules
                .OrderBy(p => p.PaymentNumber)
                .Select(MapScheduleItem)
                .ToList()
        };
    }

    public async Task<LoanResponse?> ApproveAsync(Guid id)
    {
        var loan = await _loanRepository.GetByIdAsync(id);
        if (loan == null) return null;

        loan.Approve();
        
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await _loanRepository.UpdateAsync(loan);
            await _transactionService.CreateDisbursementTransactionAsync($"disbursement-{loan.Id}", new CreateTransactionRequest
            {
                Type = TransactionType.Disbursement,
                Amount = loan.Amount,
                LoanId = loan.Id,
                Description = $"Desembolso del préstamo {loan.Id}"
            });

            await _unitOfWork.SaveChangesAsync();
        });

        _logger.LogInformation("Préstamo {LoanId} aprobado manualmente", loan.Id);

        return MapLoanResponse(loan);
    }

    public async Task<LoanResponse?> RejectAsync(Guid id)
    {
        var loan = await _loanRepository.GetByIdAsync(id);
        if (loan == null) return null;

        loan.Reject();
        await _loanRepository.UpdateAsync(loan);
        await _loanRepository.SaveChangesAsync();

        _logger.LogInformation("Préstamo {LoanId} rechazado manualmente", loan.Id);

        return MapLoanResponse(loan);
    }

    private static void ValidateAmountAndTerm(decimal amount, int term)
    {
        //TODO: Obtener los valores de MinAmount, MaxAmount, MinTerm y MaxTerm desde parámetros.
        if (amount < MinAmount || amount > MaxAmount)
            throw new BusinessRuleException($"El monto debe estar entre {MinAmount:C} y {MaxAmount:C}.");

        if (term < MinTerm || term > MaxTerm)
            throw new BusinessRuleException($"El plazo debe estar entre {MinTerm} y {MaxTerm} meses.");
    }

    private static PaymentScheduleItemDto MapScheduleItem(PaymentSchedule p) => new()
    {
        PaymentNumber = p.PaymentNumber,
        DueDate = p.DueDate,
        TotalPayment = p.TotalPayment,
        Principal = p.Principal,
        Interest = p.Interest,
        RemainingBalance = p.RemainingBalance,
        Status = p.Status.ToString()
    };

    private static LoanResponse MapLoanResponse(Loan loan) => new()
    {
        Id = loan.Id,
        UserId = loan.UserId,
        Amount = loan.Amount,
        Term = loan.Term,
        InterestRate = loan.InterestRate,
        LoanType = loan.LoanType.ToString(),
        Status = loan.Status.ToString(),
        MonthlyPayment = loan.MonthlyPayment,
        CreatedAt = loan.CreatedAt,
        UpdatedAt = loan.UpdatedAt
    };
}