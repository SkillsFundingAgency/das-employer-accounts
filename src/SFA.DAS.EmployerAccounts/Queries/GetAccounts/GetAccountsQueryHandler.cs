using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.Account;
using System.Threading;

namespace SFA.DAS.EmployerAccounts.Queries.GetAccounts
{
    public class GetAccountsQueryHandler : IRequestHandler<GetAccountsQuery, GetAccountsResponse>
    {
        private readonly IEmployerAccountRepository _employerAccountRepository;
        private readonly IValidator<GetAccountsQuery> _validator;

        public GetAccountsQueryHandler(
            IEmployerAccountRepository employerAccountRepository,
            IValidator<GetAccountsQuery> validator)
        {
            _employerAccountRepository = employerAccountRepository;
            _validator = validator;
        }

        public async Task<GetAccountsResponse> Handle(GetAccountsQuery message, CancellationToken cancellationToken)
        {
            var result = await _validator.ValidateAsync(message);

            if (!result.IsValid())
            {
                throw new InvalidRequestException(result.ValidationDictionary);
            }

            var employerAccount = await _employerAccountRepository.GetAllAccountsUpdates(message.SinceDate?.ToString("yyyy-MM-dd HH:mm:ss.fffff"), message.PageNumber, message.PageSize);
            return employerAccount;
        }
    }
}
