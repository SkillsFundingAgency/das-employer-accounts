using System.Diagnostics.CodeAnalysis;
using SFA.DAS.GovUK.Auth.Models;

namespace SFA.DAS.EmployerAccounts.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class SignInStubViewModel : StubAuthUserDetails
{
    public string ReturnUrl { get; set; }
}
