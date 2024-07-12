using FluentValidation;
using HMRC.ESFA.Levy.Api.Types;
using SFA.DAS.InputValidation.Fluent.Extensions;

namespace SFA.DAS.EmployerAccounts.Web.Validation;

public sealed class OrganisationDetailsViewModelValidator : AbstractValidator<OrganisationDetailsViewModel>
{
    public OrganisationDetailsViewModelValidator()
    {
        RuleFor(r => r.Name).NotEmpty().WithMessage("Enter a name.....");
        RuleFor(x => x.Name).ValidFreeTextCharacters();
    }
}