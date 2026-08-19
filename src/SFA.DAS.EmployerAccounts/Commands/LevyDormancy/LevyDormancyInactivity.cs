using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Models.LevyDormancy;

namespace SFA.DAS.EmployerAccounts.Commands.LevyDormancy;

internal static class LevyDormancyInactivity
{
    public static bool HasBeenInactiveForAtLeast(
        LevyDormancyRequest request,
        int months,
        LevyDormancyConfiguration configuration,
        DateTime now)
    {
        if (request.LastLevyDeclarationDate.HasValue)
        {
            return now >= request.LastLevyDeclarationDate.Value.AddMonths(months);
        }

        return now >= request.CreatedOn.AddMonths(months - configuration.DormancyDetectionMonths);
    }
}
