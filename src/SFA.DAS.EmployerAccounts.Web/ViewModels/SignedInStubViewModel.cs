using SFA.DAS.EmployerAccounts.Infrastructure;
using SFA.DAS.EmployerAccounts.Models.UserAccounts;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SFA.DAS.EmployerAccounts.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class SignedInStubViewModel
{
    private readonly ClaimsPrincipal _claimsPrinciple;

    public SignedInStubViewModel(HttpContext httpContext, string returnUrl)
    {
        _claimsPrinciple = httpContext.User;
        ReturnUrl = returnUrl;
    }

    public string StubEmail => _claimsPrinciple.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value;
    public string StubId => _claimsPrinciple.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;
    public string ReturnUrl { get; }

    public List<EmployerUserAccountItem> GetAccounts()
    {
        var associatedAccountsClaim = _claimsPrinciple.Claims.FirstOrDefault(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier))?.Value;
        if (string.IsNullOrEmpty(associatedAccountsClaim))
            return new List<EmployerUserAccountItem>();

        try
        {
            var accountsDictionary = JsonSerializer.Deserialize<Dictionary<string, EmployerUserAccountItem>>(associatedAccountsClaim);
            return accountsDictionary?.Values.ToList() ?? new List<EmployerUserAccountItem>();
        }
        catch (JsonException)
        {
            return new List<EmployerUserAccountItem>();
        }
    }
}
