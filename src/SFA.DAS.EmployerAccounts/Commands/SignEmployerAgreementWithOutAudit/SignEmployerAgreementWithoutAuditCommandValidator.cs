
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;

namespace SFA.DAS.EmployerAccounts.Commands.SignEmployerAgreementWithOutAudit;

public class SignEmployerAgreementWithoutAuditCommandValidator(
    IEmployerAgreementRepository _employerAgreementRepository)
    : IValidator<SignEmployerAgreementWithoutAuditCommand>
{
    public ValidationResult Validate(SignEmployerAgreementWithoutAuditCommand query)
    {
        throw new NotImplementedException();
    }

    public async Task<ValidationResult> ValidateAsync(SignEmployerAgreementWithoutAuditCommand query)
    {
        var validationResult = new ValidationResult();

        if (query.AgreementId <= 0)
            validationResult.AddError(nameof(query.AgreementId));

        if (string.IsNullOrWhiteSpace(query.CorrelationId))
            validationResult.AddError(nameof(query.CorrelationId));

        if (query.User == null)
            validationResult.AddError(nameof(query.User));

        if (query.AgreementId > 0)
        {
            EmployerAgreementStatus? employerAgreementStatus = await _employerAgreementRepository.GetEmployerAgreementStatus(query.AgreementId);

            if (employerAgreementStatus == null)
            {
                validationResult.AddError(nameof(employerAgreementStatus), "Agreement does not exist");
                return validationResult;
            }

            if (employerAgreementStatus == EmployerAgreementStatus.Signed ||
                employerAgreementStatus == EmployerAgreementStatus.Expired ||
                employerAgreementStatus == EmployerAgreementStatus.Superseded)
            {
                validationResult.AddError(nameof(employerAgreementStatus), $"Agreement status is {employerAgreementStatus}");
                return validationResult;
            }
        }

        return validationResult;
    }
}
