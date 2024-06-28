using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;

namespace SFA.DAS.EmployerAccounts.Queries.GetEmployerAccountDetail;

public class GetEmployerAccountDetailByHashedIdHandler : IRequestHandler<GetEmployerAccountDetailByIdQuery, GetEmployerAccountDetailByIdResponse>
{
    private readonly IValidator<GetEmployerAccountDetailByIdQuery> _validator;
    private readonly IEmployerAccountRepository _employerAccountRepository;

    public GetEmployerAccountDetailByHashedIdHandler(IValidator<GetEmployerAccountDetailByIdQuery> validator, IEmployerAccountRepository employerAccountRepository)
    {
        _validator = validator;
        _employerAccountRepository = employerAccountRepository;
    }

    public async Task<GetEmployerAccountDetailByIdResponse> Handle(GetEmployerAccountDetailByIdQuery message, CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(message);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        var account = await _employerAccountRepository.GetAccountDetailById(message.AccountId);

        return new GetEmployerAccountDetailByIdResponse { Account = account };
    }
}