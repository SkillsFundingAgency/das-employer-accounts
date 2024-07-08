using FluentValidation;
using SFA.DAS.InputValidation.Fluent.Extensions;

namespace SFA.DAS.EmployerAccounts.Web.Validation;

public sealed class RenameEmployerAccountViewModelValidator : AbstractValidator<RenameEmployerAccountViewModel>
{
    public RenameEmployerAccountViewModelValidator()
    {
        RuleFor(x => x.NewName).ValidFreeTextCharacters().WithErrorCode("NewName").WithMessage("Account name must only include letters a to z, numbers 0 to 9, and special characters such as hyphens, spaces and apostrophes");
    }
}