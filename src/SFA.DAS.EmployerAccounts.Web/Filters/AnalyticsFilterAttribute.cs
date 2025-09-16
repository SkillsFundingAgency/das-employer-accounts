using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SFA.DAS.EmployerAccounts.Infrastructure;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Responses.UserAccounts;
using SFA.DAS.EmployerAccounts.Web.Extensions;
using SFA.DAS.EmployerAccounts.Web.RouteValues;

namespace SFA.DAS.EmployerAccounts.Web.Filters;

public class AnalyticsFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        string hashedAccountId = null;
        string levyFlag = null;

        var controller = context.Controller as Controller;
        if (controller != null)
        {
            var user = controller.User;
            var userId = user?.GetUserId();

            if (controller.RouteData.Values.TryGetValue(RouteValueKeys.HashedAccountId, out var employerAccountId))
            {
                hashedAccountId = employerAccountId.ToString().ToUpper();

                var accountsJson = controller.User.Claims.FirstOrDefault(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier))?.Value;
                if (accountsJson is not null)
                {
                    var accounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerIdentifier>>(accountsJson);
                    levyFlag = accounts.TryGetValue(hashedAccountId, out var employer) ? employer.ApprenticeshipEmployerType.ToString() : null;
                }
            }

            controller.ViewBag.GaData = new GaData
            {
                UserId = userId,
                Acc = hashedAccountId, 
                LevyFlag = levyFlag
            };
        }
        base.OnActionExecuting(context);
    }

    public class GaData
    {
        public string DataLoaded { get; set; } = "dataLoaded";
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string Vpv { get; set; }
        public string Acc { get; set; }
        public string LevyFlag { get; set; }
        public IDictionary<string, string> Extras { get; set; } = new Dictionary<string, string>();
    }
}