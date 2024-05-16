using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Infrastructure;

namespace SFA.DAS.EmployerAccounts.Audit.MessageBuilders;

public class ChangedByMessageBuilder : IAuditMessageBuilder
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;

    public ChangedByMessageBuilder(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
    }

    public async Task Build(AuditMessage message)
    {
        message.ChangedBy = new Actor();
        SetOriginIpAddess(message.ChangedBy);
        await SetUserIdAndEmail(message.ChangedBy, message);
    }

    private void SetOriginIpAddess(Actor actor)
    {
        actor.OriginIpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString() == "::1"
            ? "127.0.0.1"
            : _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
    }

    private async Task SetUserIdAndEmail(Actor actor, AuditMessage message)
    {
        if (message.IsSupportRequest)
        {
            var supportUser = await _userRepository.GetByEmailAddress(message.SupportUserEmail);

            if (supportUser == null)
            {
                throw new NullReferenceException($"Unable to find the support user with the email '{message.SupportUserEmail}' to populate AuditMessage.");
            }
            
            actor.Id = supportUser.Ref.ToString();
            actor.EmailAddress = supportUser.Email;

            return;
        }

        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
        {
            return;
        }
        
        var userIdClaim = user.Claims.FirstOrDefault(c => c.Type.Equals(EmployerClaims.IdamsUserIdClaimTypeIdentifier, StringComparison.CurrentCultureIgnoreCase));
        if (userIdClaim == null)
        {
            throw new InvalidContextException($"User does not have claim {EmployerClaims.IdamsUserIdClaimTypeIdentifier} to populate AuditMessage.ChangedBy.Id");
        }

        actor.Id = userIdClaim.Value;

        var userEmailClaim = user.Claims.FirstOrDefault(c => c.Type.Equals(EmployerClaims.IdamsUserEmailClaimTypeIdentifier, StringComparison.CurrentCultureIgnoreCase));
        if (userEmailClaim == null)
        {
            throw new InvalidContextException($"User does not have claim {EmployerClaims.IdamsUserEmailClaimTypeIdentifier} to populate AuditMessage.ChangedBy.EmailAddress");
        }

        actor.EmailAddress = userEmailClaim.Value;
    }
}