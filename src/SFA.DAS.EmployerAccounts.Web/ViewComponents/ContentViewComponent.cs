using SFA.DAS.EmployerAccounts.Queries.GetContent;

namespace SFA.DAS.EmployerAccounts.Web.ViewComponents;

public class ContentViewComponent(IMediator mediator, ILogger<ContentViewComponent> logger) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            logger.LogWarning("ContentViewComponent invoked with null or empty type");
            return Content(string.Empty);
        }

        try
        {
            var response = await mediator.Send(new GetContentRequest
            {
                ContentType = type
            });

            if (response.HasFailed)
            {
                logger.LogWarning("ContentViewComponent failed to retrieve content for type: {ContentType}", type);
                return Content(string.Empty);
            }

            return Content(response.Content ?? string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ContentViewComponent error retrieving content for type: {ContentType}", type);
            return Content(string.Empty);
        }
    }
}

