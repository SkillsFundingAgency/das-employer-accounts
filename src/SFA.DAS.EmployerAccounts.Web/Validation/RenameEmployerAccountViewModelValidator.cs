using FluentValidation;
using SFA.DAS.InputValidation.Fluent.Extensions;

namespace SFA.DAS.EmployerAccounts.Web.Validation;

public sealed class RenameEmployerAccountViewModelValidator : AbstractValidator<RenameEmployerAccountViewModel>
{
    private const string ValidCharactersExpression = @"^[a-zA-Z0-9$@#()""'!,+\-=_:;.&€£*%\s\/\[\]]*$";
    
    public RenameEmployerAccountViewModelValidator()
    {
        RuleFor(r => r.NewName).Cascade(CascadeMode.Stop)
            .NotEqual(r => r.CurrentName)
            .WithMessage("You have entered your organisation name. If you want to use your organisation name select 'Yes, I want to use my organisation name as my employer account name'. If not, enter a new employer account name.")
            .NotEmpty()
            .WithMessage("Enter a name");
        
    RuleFor(x => x.NewName)
            .Matches(ValidCharactersExpression)
            .WithMessage("Account name must only include letters a to z, numbers 0 to 9, and special characters such as hyphens, spaces and apostrophes");
    }
}