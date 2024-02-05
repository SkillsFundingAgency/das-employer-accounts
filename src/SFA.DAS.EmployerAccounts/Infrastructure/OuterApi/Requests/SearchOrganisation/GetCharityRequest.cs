using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;

namespace SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.SearchOrganisation
{
    public class GetCharityRequest : IGetApiRequest
    {
        private readonly int RegistrationNumber;

        public string GetUrl => $"SearchOrganisation/charities/{RegistrationNumber}";

        public GetCharityRequest(int registrationNumber)
        {
            RegistrationNumber = registrationNumber;
        }
    }
}