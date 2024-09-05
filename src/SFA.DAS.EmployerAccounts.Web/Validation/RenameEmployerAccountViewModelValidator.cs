using FluentValidation;
using SFA.DAS.InputValidation.Fluent.Extensions;

namespace SFA.DAS.EmployerAccounts.Web.Validation;

public enum RenameAccountType
{
    RenameAccount,
    RegistrationChangeAccountName
}

public sealed class RenameEmployerAccountViewModelValidator : AbstractValidator<RenameEmployerAccountViewModel>
{
    private const string RenameAccountMessage = "New account name must not be the same as current name";
    private const string RegistrationChangeAccountMessage = "You have entered your organisation name. If you want to use your organisation name select 'Yes, I want to use my organisation name as my employer account name'. If not, enter a new employer account name.";
    
    public RenameEmployerAccountViewModelValidator(RenameAccountType accountType)
    {
        var sameNameErrorMessage = accountType == RenameAccountType.RenameAccount ? RenameAccountMessage : RegistrationChangeAccountMessage;
        
        RuleFor(r => r.NewName)
            .Cascade(CascadeMode.Stop)
            .NotEqual(r => r.CurrentName)
            .WithMessage(sameNameErrorMessage)
            .NotEmpty()
            .WithMessage("Enter a name");

        RuleFor(x => x.NewName)
            .ValidFreeTextCharacters()
            .WithMessage("Account name must only include letters a to z, numbers 0 to 9, and special characters such as hyphens, spaces and apostrophes");
    }
}