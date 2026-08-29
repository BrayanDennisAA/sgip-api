using Sgip.Domain.Enums;

namespace Sgip.Application.DTOs;

public class SimulateLoanRequest
{
    public decimal Amount { get; set; }

    public int Term { get; set; }

    public LoanType LoanType { get; set; } = LoanType.Fixed;
}

public class CreateLoanRequest : SimulateLoanRequest
{
    public string UserId { get; set; } = string.Empty;

    // Ingresos mensuales del cliente, usados para la validación del 40%.
    // En un sistema real vendría del perfil del usuario;
    public decimal MonthlyIncome { get; set; } = 10000;
}

public class PaymentScheduleItemDto
{
    public int PaymentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal Principal { get; set; }
    public decimal Interest { get; set; }
    public decimal RemainingBalance { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class SimulateLoanResponse
{
    public decimal Amount { get; set; }
    public int Term { get; set; }
    public decimal TeaRate { get; set; }
    public decimal TemRate { get; set; }
    public string LoanType { get; set; } = string.Empty;
    public decimal MonthlyPayment { get; set; }
    public List<PaymentScheduleItemDto> Schedule { get; set; } = new();
}

public class LoanResponse
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Term { get; set; }
    public decimal InterestRate { get; set; }
    public string LoanType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal MonthlyPayment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class LoanDetailResponse : LoanResponse
{
    public List<PaymentScheduleItemDto> Schedule { get; set; } = new();
}