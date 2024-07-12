using FluentValidation;
using HMRC.ESFA.Levy.Api.Types;
using SFA.DAS.InputValidation.Fluent.Extensions;

namespace SFA.DAS.EmployerAccounts.Web.Validation;

public sealed class OrganisationDetailsViewModelValidator : AbstractValidator<OrganisationDetailsViewModel>
{
    public OrganisationDetailsViewModelValidator()
    {
        RuleFor(r => r.Name).NotEmpty().WithMessage("Enter a name");
        RuleFor(x => x.Name).ValidFreeTextCharacters().WithMessage("Account Name must only include letters a to z, numbers 0 to 9, and special characters such as hyphens, spaces and apostrophes"); ;
    }
}