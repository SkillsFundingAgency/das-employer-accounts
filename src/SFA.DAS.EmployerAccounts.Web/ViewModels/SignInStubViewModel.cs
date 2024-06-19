using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerAccounts.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class SignInStubViewModel
{
    public string StubEmail { get; set; }
    public string StubId { get; set; }
    public string ReturnUrl { get; set; }
}
