using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;

namespace SFA.DAS.EmployerAccounts.Queries.GetAccountById
{

    public class GetAccountByIdHandler : IRequestHandler<GetAccountByIdQuery, GetAccountByIdResponse>
    {
        private readonly IEmployerAccountRepository _employerAccountRepository;
        private readonly IValidator<GetAccountByIdQuery> _validator;

        public GetAccountByIdHandler(
            IEmployerAccountRepository employerAccountRepository,
            IValidator<GetAccountByIdQuery> validator)
        {
            _employerAccountRepository = employerAccountRepository;
            _validator = validator;
        }

        public async Task<GetAccountByIdResponse> Handle(GetAccountByIdQuery message, CancellationToken cancellationToken)
        {
            var result = _validator.Validate(message);

            if (!result.IsValid())
            {
                throw new InvalidRequestException(result.ValidationDictionary);
            }

            var employerAccount = await _employerAccountRepository.GetAccountById(message.AccountId);

            return new GetAccountByIdResponse
            {
                Account = employerAccount
            };
        }
    }
}