﻿using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Authentication;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.Validation;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Models;
using Entity = SFA.DAS.Audit.Types.Entity;
using SFA.DAS.EmployerAccounts.Extensions;

namespace SFA.DAS.EmployerAccounts.Queries.GetAccountTeamMembers
{
    public class GetAccountTeamMembersHandler : IAsyncRequestHandler<GetAccountTeamMembersQuery, GetAccountTeamMembersResponse>
    {
        private readonly IValidator<GetAccountTeamMembersQuery> _validator;
        private readonly IEmployerAccountTeamRepository _repository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IMediator _mediator;
        private readonly IAuthenticationService _authenticationService;
        private readonly EmployerAccountsConfiguration _config;

        public GetAccountTeamMembersHandler(
            IValidator<GetAccountTeamMembersQuery> validator, 
            IEmployerAccountTeamRepository repository,
            IAuthenticationService authenticationService,
            IMediator mediator, 
            IMembershipRepository membershipRepository, EmployerAccountsConfiguration config)
        {
            _validator = validator;
            _repository = repository;
            _authenticationService = authenticationService;
            _mediator = mediator;
            _membershipRepository = membershipRepository;
            _config = config;
        }

        public async Task<GetAccountTeamMembersResponse> Handle(GetAccountTeamMembersQuery message)
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

            var accounts = await _repository.GetAccountTeamMembersForUserId(message.HashedAccountId, message.ExternalUserId);

            if (_authenticationService.IsSupportConsoleUser(_config.SupportConsoleUsers))
            {
                await AuditAccess(message);
            }

            return new GetAccountTeamMembersResponse { TeamMembers = accounts };
        }

        private async Task AuditAccess(GetAccountTeamMembersQuery message)
        {
            var caller = await _membershipRepository.GetCaller(message.HashedAccountId, message.ExternalUserId);

            await _mediator.SendAsync(new CreateAuditCommand
            {
                EasAuditMessage = new EasAuditMessage
                {
                    Category = "VIEW",
                    Description = $"Account {caller.AccountId} team members viewed",
                    AffectedEntity = new Entity { Type = "TeamMembers", Id = caller.AccountId.ToString() }
                }
            });
        }
    }
}