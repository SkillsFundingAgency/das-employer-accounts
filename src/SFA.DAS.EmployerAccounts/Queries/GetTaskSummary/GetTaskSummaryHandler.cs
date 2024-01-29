using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerAccounts.Queries.GetAccountTasks;

namespace SFA.DAS.EmployerAccounts.Queries.GetTaskSummary
{
    public class GetTaskSummaryHandler : IRequestHandler<GetTaskSummaryQuery, GetTaskSummaryResponse>
    {
        private readonly IValidator<GetTaskSummaryQuery> _validator;
        private readonly IEmployerAccountService _employerAccountService;

        public GetTaskSummaryHandler(IValidator<GetTaskSummaryQuery> validator, IEmployerAccountService employerAccountService)
        {
            _validator = validator;
            _employerAccountService = employerAccountService;
        }

        public async Task<GetTaskSummaryResponse> Handle(GetTaskSummaryQuery message, CancellationToken cancellationToken)
        {
            ValidateMessage(message);
            var taskSummary = await _employerAccountService.GetTaskSummary(message.AccountId);

            return new GetTaskSummaryResponse
            {
                TaskSummary = taskSummary
            };
        }

        private void ValidateMessage(GetTaskSummaryQuery message)
        {
            var validationResults = _validator.Validate(message);

            if (!validationResults.IsValid())
            {
                throw new InvalidRequestException(validationResults.ValidationDictionary);
            }
        }
    }
}
