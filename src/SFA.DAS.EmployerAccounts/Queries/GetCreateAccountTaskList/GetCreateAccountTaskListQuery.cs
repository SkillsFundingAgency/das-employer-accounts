namespace SFA.DAS.EmployerAccounts.Queries.GetCreateAccountTaskList;

public record GetCreateAccountTaskListQuery(long AccountId, string HashedAccountId, string UserRef)
    : IRequest<GetCreateAccountTaskListQueryResponse>;