
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Commands.SignEmployerAgreementWithOutAudit;

public class SignEmployerAgreementWithoutAuditCommandValidator(
    IEmployerAgreementRepository _employerAgreementRepository,
    IEncodingService _encodingService)
    : IValidator<SignEmployerAgreementWithoutAuditCommand>
{
    public ValidationResult Validate(SignEmployerAgreementWithoutAuditCommand query)
    {
        throw new NotImplementedException();
    }

    public async Task<ValidationResult> ValidateAsync(SignEmployerAgreementWithoutAuditCommand query)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrWhiteSpace(query.HashedAgreementId))
            validationResult.AddError(nameof(query.HashedAgreementId));

        if (string.IsNullOrWhiteSpace(query.CorrelationId))
            validationResult.AddError(nameof(query.CorrelationId));

        if (query.User == null)
            validationResult.AddError(nameof(query.User));

        if (!string.IsNullOrWhiteSpace(query.HashedAgreementId))
        {
            var agreementId = _encodingService.Decode(query.HashedAgreementId, EncodingType.AccountId);
            EmployerAgreementStatus? employerAgreementStatus = await _employerAgreementRepository.GetEmployerAgreementStatus(agreementId);

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
