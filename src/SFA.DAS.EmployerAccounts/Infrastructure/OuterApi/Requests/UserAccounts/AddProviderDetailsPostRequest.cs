using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;

namespace SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.UserAccounts
{
    public class AddProviderDetailsPostRequest : IPostApiRequest
    {
        public string UserId { get; set; }
        public object Data { get; set; } = null;

        public AddProviderDetailsPostRequest(string userId, string correlationId, long accountId,
            string email, string firstName, string lastName)
        {
            UserId = userId;
            Data = new AddProviderDetailsRequestData
            {
                CorrelationId = correlationId,
                AccountId = accountId,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };
        }

        public string PostUrl => $"accountusers/{UserId}/add-provider-details-from-invitation";

        private sealed class AddProviderDetailsRequestData
        {
            public long AccountId { get; set; }
            public string CorrelationId { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
    }
}
