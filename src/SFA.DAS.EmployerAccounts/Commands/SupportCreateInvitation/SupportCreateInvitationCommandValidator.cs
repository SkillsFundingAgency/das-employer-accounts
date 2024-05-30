using System.Text.RegularExpressions;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Commands.SupportCreateInvitation;

public class SupportCreateInvitationCommandValidator : IValidator<SupportCreateInvitationCommand>
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IEncodingService _encodingService;

    public SupportCreateInvitationCommandValidator(IMembershipRepository membershipRepository, IEncodingService encodingService)
    {
        _membershipRepository = membershipRepository;
        _encodingService = encodingService;
    }

    public ValidationResult Validate(SupportCreateInvitationCommand item)
    {
        throw new NotImplementedException();
    }

    public async Task<ValidationResult> ValidateAsync(SupportCreateInvitationCommand item)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrEmpty(item.HashedAccountId))
        {
            validationResult.AddError(nameof(item.HashedAccountId), "No HashedAccountId supplied");
        }

        if (string.IsNullOrWhiteSpace(item.EmailOfPersonBeingInvited))
        {
            validationResult.AddError(nameof(item.EmailOfPersonBeingInvited), "Enter email address");
        }
        else if (!IsValidEmailFormat(item.EmailOfPersonBeingInvited))
        {
            validationResult.AddError(nameof(item.EmailOfPersonBeingInvited), "Enter a valid email address");
        }

        if (string.IsNullOrWhiteSpace(item.NameOfPersonBeingInvited))
        {
            validationResult.AddError(nameof(item.NameOfPersonBeingInvited), "Enter name");
        }
        
        if (string.IsNullOrWhiteSpace(item.SupportUserEmail))
        {
            validationResult.AddError(nameof(item.SupportUserEmail), "Specify support user email");
        }

        if (!validationResult.IsValid())
        {
            return validationResult;
        }

        if (!validationResult.IsValid())
        {
            return validationResult;
        }

        var accountId = _encodingService.Decode(item.HashedAccountId, EncodingType.AccountId);

        var existingTeamMember = await _membershipRepository.Get(accountId, item.EmailOfPersonBeingInvited);

        if (existingTeamMember != null && existingTeamMember.IsUser)
        {
            validationResult.AddError(nameof(item.EmailOfPersonBeingInvited), $"{item.EmailOfPersonBeingInvited} is already invited");
        }

        return validationResult;
    }

    private static bool IsValidEmailFormat(string email)
    {
        return Regex.IsMatch(email,
            @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
            @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
            RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
    }
}