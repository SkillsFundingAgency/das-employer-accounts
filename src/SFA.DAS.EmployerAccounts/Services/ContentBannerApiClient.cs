﻿using System.Net.Http;
using System.Threading.Tasks;
using SFA.DAS.Authentication.Extensions.Legacy;
using SFA.DAS.EmployerAccounts.Interfaces;

namespace SFA.DAS.EmployerAccounts.Services
{
    public class ContentBannerApiClient : ApiClientBase, IContentBannerApiClient
    {
        public ContentBannerApiClient(HttpClient client) : base(client)
        {
        }

        public Task<string> GetBanner(int bannerId, bool useCDN)
        {
            string banner;
            if (useCDN)
            {
                banner = "<div class=\"info-summary\">" +
                         "<h2 class=\"heading-medium\">" +
                         "Coronavirus(COVID-19) : <a href = \"https://www.gov.uk/government/publications/coronavirus-covid-19-apprenticeship-programme-response/coronavirus-covid-19-guidance-for-apprentices-employers-training-providers-end-point-assessment-organisations-and-external-quality-assurance-pro\" " +
                         "target=\"_blank\">read our guidance</a> on the changes we're making to help your apprentices continue learning or " +
                         "<a href=\"https://help.apprenticeships.education.gov.uk/hc/en-gb/articles/360009509360-Pause-or-stop-an-apprenticeship\" target=\"_blank\">" +
                         "find out how you can pause your apprenticeships</a>.</h2></div>";
            }
            else
            {
                banner = "<div class=\"das-notification\">" +
                         "<p class=\"das-notification__heading govuk-!-margin-bottom-0\">" +
                         "Coronavirus(COVID-19):<a href = \"https://www.gov.uk/government/publications/coronavirus-covid-19-apprenticeship-programme-response/coronavirus-covid-19-guidance-for-apprentices-employers-training-providers-end-point-assessment-organisations-and-external-quality-assurance-pro\" " +
                         "target=\"_blank\" class=\"govuk-link\">read our guidance</a> on the changes we're making to help your apprentices continue learning or <a href=\"https://help.apprenticeships.education.gov.uk/hc/en-gb/articles/360009509360-Pause-or-stop-an-apprenticeship\" " +
                         "target=\"_blank\" class=\"govuk-link\">find out how you can pause your apprenticeships</a>.</p></div>";
            }
            return Task.FromResult(banner);
        }
    }
}