namespace SFA.DAS.EmployerAccounts.Api.Responses
{
    public record struct PageInfo(int TotalCount, int PageIndex, int PageSize, int TotalPages, bool HasPreviousPage, bool HasNextPage);
}
