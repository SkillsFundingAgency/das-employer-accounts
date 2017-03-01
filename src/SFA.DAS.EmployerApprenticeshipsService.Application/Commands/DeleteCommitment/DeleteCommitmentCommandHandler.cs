﻿using System;
using System.Threading.Tasks;

using MediatR;

using SFA.DAS.Commitments.Api.Client;
using SFA.DAS.EAS.Application.Validation;

namespace SFA.DAS.EAS.Application.Commands.DeleteCommitment
{
    public class DeleteCommitmentCommandHandler : AsyncRequestHandler<DeleteCommitmentCommand>
    {
        private readonly IValidator<DeleteCommitmentCommand> _validator;

        private readonly ICommitmentsApi _commitmentsService;

        public DeleteCommitmentCommandHandler(ICommitmentsApi commitmentsApi, IValidator<DeleteCommitmentCommand> validator)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
            if (commitmentsApi == null)
                throw new ArgumentNullException(nameof(commitmentsApi));

            _validator = validator;
            _commitmentsService = commitmentsApi;
        }

        protected override async Task HandleCore(DeleteCommitmentCommand message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid())
            {
                throw new InvalidRequestException(validationResult.ValidationDictionary);
            }

            await _commitmentsService.DeleteEmployerCommitment(message.AccountId, message.CommitmentId, message.UserId);
        }
    }
}
