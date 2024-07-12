﻿using FluentValidation;
using SFA.DAS.InputValidation.Fluent.Extensions;

namespace SFA.DAS.EmployerAccounts.Web.Validation;

public sealed class RenameEmployerAccountViewModelValidator : AbstractValidator<RenameEmployerAccountViewModel>
{
    public RenameEmployerAccountViewModelValidator()
    {
        RuleFor(r => r.NewName).NotEmpty().WithMessage("Enter a name");
        RuleFor(x => x.NewName).ValidFreeTextCharacters().WithMessage("Account Name must only include letters a to z, numbers 0 to 9, and special characters such as hyphens, spaces and apostrophes");
    }
}