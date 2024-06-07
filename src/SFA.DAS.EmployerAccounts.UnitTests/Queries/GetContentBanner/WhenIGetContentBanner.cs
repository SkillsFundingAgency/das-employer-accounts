using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Infrastructure;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Models.UserAccounts;
using SFA.DAS.EmployerAccounts.Queries.GetContent;
using SFA.DAS.Testing.AutoFixture;
using ValidationResult = SFA.DAS.EmployerAccounts.Validation.ValidationResult;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetContentBanner;

public class WhenIGetContentBanner : QueryBaseTest<GetContentRequestHandler, GetContentRequest, GetContentResponse>
{
    public override GetContentRequest Query { get; set; }
    public override GetContentRequestHandler RequestHandler { get; set; }
    public override Mock<IValidator<GetContentRequest>> RequestValidator { get; set; }

    private Mock<IContentApiClient> _contentBannerService;
    private string _contentType;
    private string _clientId;
    private Mock<ILogger<GetContentRequestHandler>> _logger;
    private string _contentBanner;
    private EmployerAccountsConfiguration _employerAccountsConfiguration;
    private Mock<IHttpContextAccessor> _httpContextAccessor;
    private const string AccountId = "AAASG232";
    private string _userId;

    [SetUp]
    public void Arrange()
    {
        SetUp();
        _employerAccountsConfiguration = new EmployerAccountsConfiguration()
        {
            ApplicationId = "eas-acc",
            DefaultCacheExpirationInMinutes = 1
        };
        _contentBanner = "<p>find out how you can pause your apprenticeships<p>";
        _contentType = "banner";
        _clientId = "eas-acc-levy";
        _logger = new Mock<ILogger<GetContentRequestHandler>>();
        _contentBannerService = new Mock<IContentApiClient>();
        _contentBannerService
            .Setup(cbs => cbs.Get(_contentType, _clientId))
            .ReturnsAsync(_contentBanner);

        Query = new GetContentRequest
        {
            ContentType = "banner"
        };

        _userId = Guid.NewGuid().ToString();
        var userClaim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, _userId);
        var employerUserAccountItems = new Dictionary<string, EmployerUserAccountItem>
        {
            {
                AccountId, new EmployerUserAccountItem
                {
                    AccountId = AccountId,
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy
                }
            }
        };

        var accountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerUserAccountItems));
        var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { userClaim, accountsClaim }) });
        var httpContext = new DefaultHttpContext(new FeatureCollection())
        {
            User = claimsPrinciple
        };
        httpContext.Request.RouteValues.Add("HashedAccountId", AccountId.ToUpper());
        _httpContextAccessor = new Mock<IHttpContextAccessor>();
        _httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        RequestHandler = new GetContentRequestHandler(RequestValidator.Object, _logger.Object, _contentBannerService.Object, _employerAccountsConfiguration, _httpContextAccessor.Object);
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
    {
        await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        _contentBannerService.Verify(x => x.Get(_contentType, _clientId), Times.Once);
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
    {
        //Act
        var response = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        response.Content.Should().Be(_contentBanner);
    }

    [Test, RecursiveMoqAutoData]
    public async Task Check_Cache_ReturnNull_CallFromClient(GetContentRequest query1, string contentBanner1,
        Mock<ICacheStorageService> cacheStorageService1,
        GetContentRequestHandler requestHandler1,
        Mock<IValidator<GetContentRequest>> requestValidator1,
        Mock<ILogger<GetContentRequestHandler>> logger,
        Mock<IContentApiClient> MockContentService)
    {
        //Arrange
        var key = $"{_employerAccountsConfiguration.ApplicationId}-levy";
        query1.ContentType = "Banner";
        query1.UseLegacyStyles = false;

        requestValidator1.Setup(r => r.Validate(query1)).Returns(new ValidationResult());

        string nullCacheString = null;
        var cacheKey = key + "_banner";
        cacheStorageService1.Setup(c => c.TryGet(cacheKey, out nullCacheString))
            .Returns(false);

        MockContentService.Setup(c => c.Get(query1.ContentType, key))
            .ReturnsAsync(contentBanner1);

        requestHandler1 = new GetContentRequestHandler(requestValidator1.Object, logger.Object, MockContentService.Object, _employerAccountsConfiguration, _httpContextAccessor.Object);

        //Act
        var result = await requestHandler1.Handle(query1, CancellationToken.None);

        //assert
        result.Content.Should().Be(contentBanner1);
    }

    [Test, MoqAutoData]
    public async Task TheLevyStatusIsAppendedToApplicationIdFromUserClaims(
        Mock<IValidator<GetContentRequest>> validator,
        Mock<IContentApiClient> contentApiClient,
        EmployerAccountsConfiguration configuration,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        GetContentRequest request,
        Mock<ILogger<GetContentRequestHandler>> logger,
        string content,
        ApprenticeshipEmployerType levyStatus
    )
    {
        request.UseLegacyStyles = false;

        var employerUserAccountItems = new Dictionary<string, EmployerUserAccountItem>
        {
            {
                AccountId, new EmployerUserAccountItem
                {
                    AccountId = AccountId,
                    ApprenticeshipEmployerType = levyStatus
                }
            }
        };

        var jsonAccountItems = JsonConvert.SerializeObject(employerUserAccountItems);
        var accountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, jsonAccountItems);
        var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { accountsClaim }) });

        var httpContext = new DefaultHttpContext(new FeatureCollection()) { User = claimsPrinciple };
        httpContext.Request.RouteValues.Add("HashedAccountId", AccountId.ToUpper());
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        validator.Setup(x => x.Validate(request)).Returns(new ValidationResult());

        var sut = new GetContentRequestHandler(validator.Object, logger.Object, contentApiClient.Object, configuration, httpContextAccessor.Object);

        await sut.Handle(request, CancellationToken.None);

        contentApiClient.Verify(x => x.Get(request.ContentType, $"{configuration.ApplicationId}-{levyStatus.ToString().ToLower()}"), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task ThenAnEmptyResponseIsReturnedWhenThereIsNoHashedAccountIdOnTheRoute(
        Mock<IValidator<GetContentRequest>> validator,
        Mock<IContentApiClient> contentApiClient,
        EmployerAccountsConfiguration configuration,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        GetContentRequest request,
        Mock<ILogger<GetContentRequestHandler>> logger,
        string content,
        ApprenticeshipEmployerType levyStatus
    )
    {
        var httpContext = new DefaultHttpContext(new FeatureCollection());
        httpContext.Request.RouteValues.Add("Nothing", string.Empty);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        validator.Setup(x => x.Validate(request)).Returns(new ValidationResult());

        var sut = new GetContentRequestHandler(validator.Object, logger.Object, contentApiClient.Object, configuration, httpContextAccessor.Object);

        var result = await sut.Handle(request, CancellationToken.None);

        result.Content.Should().BeNullOrEmpty();
        result.HasFailed.Should().BeFalse();
        
        contentApiClient.Verify(x => x.Get(request.ContentType, $"{configuration.ApplicationId}-{levyStatus.ToString().ToLower()}"), Times.Never);
    }
}