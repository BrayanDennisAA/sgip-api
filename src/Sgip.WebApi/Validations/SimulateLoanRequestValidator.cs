using FluentValidation;
using Sgip.Application.DTOs;

namespace Sgip.WebApi.Validations;

public class SimulateLoanRequestValidator : AbstractValidator<SimulateLoanRequest>
{
    public SimulateLoanRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto del préstamo debe ser mayor que cero.");

        RuleFor(x => x.Term)
            .GreaterThan(0).WithMessage("El plazo del préstamo debe ser mayor que cero.");

        RuleFor(x => x.LoanType)
            .IsInEnum().WithMessage("El tipo de préstamo no es válido.");
    }
}