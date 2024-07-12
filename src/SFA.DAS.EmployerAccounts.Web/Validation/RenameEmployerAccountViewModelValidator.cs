using FluentValidation;
using SFA.DAS.InputValidation.Fluent.Extensions;

namespace SFA.DAS.EmployerAccounts.Web.Validation;

public sealed class RenameEmployerAccountViewModelValidator : AbstractValidator<RenameEmployerAccountViewModel>
{
    public RenameEmployerAccountViewModelValidator()
    {
        RuleFor(r => r.NewName).NotEmpty().WithMessage("Enter a name, please.....");
        RuleFor(x => x.NewName).ValidFreeTextCharacters();
    }
}