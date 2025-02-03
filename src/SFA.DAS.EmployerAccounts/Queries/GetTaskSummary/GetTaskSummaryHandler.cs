using System.Threading;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Queries.GetTaskSummary;

public class GetTaskSummaryHandler(
    IValidator<GetTaskSummaryQuery> validator,
    IEmployerAccountService employerAccountService,
    IEncodingService encodingService)
    : IRequestHandler<GetTaskSummaryQuery, GetTaskSummaryResponse>
{
    public async Task<GetTaskSummaryResponse> Handle(GetTaskSummaryQuery message, CancellationToken cancellationToken)
    {
        ValidateMessage(message);
        var taskSummary = await employerAccountService.GetTaskSummary(message.AccountId);

        if (taskSummary is { SingleAcceptedTransferPledgeApplicationIdWithNoApprentices: not null })
        {
            taskSummary.SingleAcceptedTransferPledgeApplicationHashedIdWithNoApprentices = encodingService.Encode(
                taskSummary.SingleAcceptedTransferPledgeApplicationIdWithNoApprentices.Value,
                EncodingType.PledgeApplicationId
            );
        }
        
        if (taskSummary is { SingleApprovedTransferApplicationId: not null })
        {
            taskSummary.SingleApprovedTransferApplicationHashedId = encodingService.Encode(taskSummary.SingleApprovedTransferApplicationId.Value, EncodingType.PledgeApplicationId);
        }

        return new GetTaskSummaryResponse
        {
            TaskSummary = taskSummary ?? new TaskSummary()
        };
    }

    private void ValidateMessage(GetTaskSummaryQuery message)
    {
        var validationResults = validator.Validate(message);

        if (!validationResults.IsValid())
        {
            throw new InvalidRequestException(validationResults.ValidationDictionary);
        }
    }
}