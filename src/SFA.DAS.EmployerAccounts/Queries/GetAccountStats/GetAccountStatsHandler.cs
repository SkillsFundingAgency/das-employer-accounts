﻿using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Queries.GetAccountStats;

public class GetAccountStatsHandler : IRequestHandler<GetAccountStatsQuery, GetAccountStatsResponse>
{
    private readonly IEmployerAccountRepository _repository;
    private readonly IEncodingService _encodingService;
    private readonly IValidator<GetAccountStatsQuery> _validator;

    public GetAccountStatsHandler(IEmployerAccountRepository repository, IEncodingService encodingService, IValidator<GetAccountStatsQuery> validator)
    {
        _repository = repository;
        _encodingService = encodingService;
        _validator = validator;
    }

    public async Task<GetAccountStatsResponse> Handle(GetAccountStatsQuery message, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(message);

        if (!validationResult.IsValid())
        {
            if (validationResult.IsUnauthorized)
            {
                throw new UnauthorizedAccessException("User not authorised");
            }

            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        var accoundId = _encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);

        var stats = await _repository.GetAccountStats(accoundId);

        return new GetAccountStatsResponse { Stats = stats };
    }
}