using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace SFA.DAS.EmployerAccounts.Web.Helpers;

public interface IHtmlHelpers
{
    bool ViewExists(IHtmlHelper html, string viewName);
    string ReturnToHomePageButtonHref(string hashedAccountId);
    string ReturnToHomePageButtonText(string hashedAccountId);
    string ReturnToHomePageLinkText(string hashedAccountId);
}

public class HtmlHelpers(
    ILogger<HtmlHelpers> logger,
    ICompositeViewEngine compositeViewEngine)
    : IHtmlHelpers
{
    public static HtmlString SetZenDeskLabels(params string[] labels)
    {
        var keywords = string.Join(",", labels
            .Where(label => !string.IsNullOrEmpty(label))
            .Select(label => $"'{EscapeApostrophes(label)}'"));

        // when there are no keywords default to empty string to prevent zen desk matching articles from the url
        var apiCallString = "<script type=\"text/javascript\">zE('webWidget', 'helpCenter:setSuggestions', { labels: ["
                            + (!string.IsNullOrEmpty(keywords) ? keywords : "''")
                            + "] });</script>";

        return new HtmlString(apiCallString);
    }

    private static string EscapeApostrophes(string input)
    {
        return input.Replace("'", @"\'");
    }

    public bool ViewExists(IHtmlHelper html, string viewName)
    {
        var result = compositeViewEngine.FindView(html.ViewContext, viewName, false);

        return result.View != null;
    }

    public string ReturnToHomePageButtonHref(string hashedAccountId)
    {
        logger.LogDebug("ReturnToHomePageButtonHref :: Accountid : {AccountId}", hashedAccountId);

        return !string.IsNullOrEmpty(hashedAccountId) ? $"/accounts/{hashedAccountId}/teams" : "/";
    }

    public string ReturnToHomePageButtonText(string hashedAccountId)
    {
        logger.LogDebug("ReturnToHomePageButtonText :: AccountId : {AccountId} ", hashedAccountId);

        return !string.IsNullOrEmpty(hashedAccountId) ? "Go back to the account home page" : "Go back to the service home page";
    }

    public string ReturnToHomePageLinkText(string hashedAccountId)
    {
        logger.LogDebug("ReturnToHomePageLinkText :: AccountId : {AccountId}", hashedAccountId);

        return !string.IsNullOrEmpty(hashedAccountId) ? "Back to the homepage" : "Back";
    }
}
