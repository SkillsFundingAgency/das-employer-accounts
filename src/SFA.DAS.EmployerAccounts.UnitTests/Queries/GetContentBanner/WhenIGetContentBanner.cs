using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Queries.GetContent;
using SFA.DAS.EmployerAccounts.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetContentBanner;

public class WhenIGetContentBanner : QueryBaseTest<GetContentRequestHandler, GetContentRequest, GetContentResponse>
{
    public override GetContentRequest Query { get; set; }
    public override GetContentRequestHandler RequestHandler { get; set; }
    public override Mock<IValidator<GetContentRequest>> RequestValidator { get; set; }

    private Mock<IContentApiClient> _contentBannerService;
    private Mock<IAssociatedAccountsService> _associatedAccountsService;
    private string _contentType;
    private string _clientId;
    private Mock<ILogger<GetContentRequestHandler>> _logger;
    private string _contentBanner;
    private EmployerAccountsConfiguration _employerAccountsConfiguration;
    private Mock<IHttpContextAccessor> _httpContextAccessor;
    private const string AccountId = "AAASG232";

    [SetUp]
    public void Arrange()
    {
        SetUp();
        _employerAccountsConfiguration = new EmployerAccountsConfiguration
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

        var employerUserAccountItems = new Dictionary<string, GovUK.Auth.Employer.EmployerUserAccountItem>
        {
            {
                AccountId, new GovUK.Auth.Employer.EmployerUserAccountItem
                {
                    AccountId = AccountId,
                    ApprenticeshipEmployerType = GovUK.Auth.Employer.ApprenticeshipEmployerType.Levy
                }
            }
        };
        
        _associatedAccountsService = new Mock<IAssociatedAccountsService>();
        _associatedAccountsService.Setup(x => x.GetAccounts(false)).ReturnsAsync(employerUserAccountItems);

        Query = new GetContentRequest
        {
            ContentType = "banner"
        };
        
        var httpContext = new DefaultHttpContext(new FeatureCollection());
        httpContext.Request.RouteValues.Add("HashedAccountId", AccountId.ToUpper());
        _httpContextAccessor = new Mock<IHttpContextAccessor>();
        _httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        RequestHandler = new GetContentRequestHandler(RequestValidator.Object, _logger.Object, _contentBannerService.Object, _employerAccountsConfiguration, _httpContextAccessor.Object, _associatedAccountsService.Object);
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
        Mock<IAssociatedAccountsService> associatedAccountsService,
        Mock<ILogger<GetContentRequestHandler>> logger,
        Mock<IContentApiClient> MockContentService)
    {
        //Arrange
        var key = $"{_employerAccountsConfiguration.ApplicationId}-levy";
        query1.ContentType = "Banner";
        
        var employerUserAccountItems = new Dictionary<string, GovUK.Auth.Employer.EmployerUserAccountItem>
        {
            {
                AccountId, new GovUK.Auth.Employer.EmployerUserAccountItem
                {
                    AccountId = AccountId,
                    ApprenticeshipEmployerType = GovUK.Auth.Employer.ApprenticeshipEmployerType.Levy
                }
            }
        };
        
        associatedAccountsService.Setup(x => x.GetAccounts(false)).ReturnsAsync(employerUserAccountItems);
    
        requestValidator1.Setup(r => r.Validate(query1)).Returns(new ValidationResult());
    
        var cacheKey = key + "_banner";
        cacheStorageService1.Setup(c => c.TryGetAsync(cacheKey))
            .ReturnsAsync((false, (string)null));
    
        MockContentService.Setup(c => c.Get(query1.ContentType, key))
            .ReturnsAsync(contentBanner1);
    
        requestHandler1 = new GetContentRequestHandler(requestValidator1.Object, logger.Object, MockContentService.Object, _employerAccountsConfiguration, _httpContextAccessor.Object, associatedAccountsService.Object);
    
        //Act
        var result = await requestHandler1.Handle(query1, CancellationToken.None);
    
        //assert
        result.Content.Should().Be(contentBanner1);
    }
    
    [Test]
    [MoqInlineAutoData(ApprenticeshipEmployerType.Levy)]
    [MoqInlineAutoData(ApprenticeshipEmployerType.NonLevy)]
    public async Task TheLevyStatusIsAppendedToApplicationIdFromUserClaims(
        GovUK.Auth.Employer.ApprenticeshipEmployerType levyStatus,
        Mock<IValidator<GetContentRequest>> validator,
        Mock<IContentApiClient> contentApiClient,
        EmployerAccountsConfiguration configuration,
        Mock<IAssociatedAccountsService> associatedAccountsService,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        GetContentRequest request,
        Mock<ILogger<GetContentRequestHandler>> logger,
        string content
    )
    {
        var employerUserAccountItems = new Dictionary<string, GovUK.Auth.Employer.EmployerUserAccountItem>
        {
            {
                AccountId, new GovUK.Auth.Employer.EmployerUserAccountItem
                {
                    AccountId = AccountId,
                    ApprenticeshipEmployerType = levyStatus
                }
            }
        };
        
        associatedAccountsService.Setup(x => x.GetAccounts(false)).ReturnsAsync(employerUserAccountItems);
       
        var httpContext = new DefaultHttpContext(new FeatureCollection());
        httpContext.Request.RouteValues.Add("HashedAccountId", AccountId.ToUpper());
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    
        validator.Setup(x => x.Validate(request)).Returns(new ValidationResult());
    
        var sut = new GetContentRequestHandler(validator.Object, logger.Object, contentApiClient.Object, configuration, httpContextAccessor.Object, associatedAccountsService.Object);
    
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
        GovUK.Auth.Employer.ApprenticeshipEmployerType levyStatus
    )
    {
        var httpContext = new DefaultHttpContext(new FeatureCollection());
        httpContext.Request.RouteValues.Add("Nothing", string.Empty);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    
        validator.Setup(x => x.Validate(request)).Returns(new ValidationResult());
    
        var sut = new GetContentRequestHandler(validator.Object, logger.Object, contentApiClient.Object, configuration, httpContextAccessor.Object, Mock.Of<IAssociatedAccountsService>());
    
        var result = await sut.Handle(request, CancellationToken.None);
    
        result.Content.Should().BeNullOrEmpty();
        result.HasFailed.Should().BeFalse();
        
        contentApiClient.Verify(x => x.Get(request.ContentType, $"{configuration.ApplicationId}-{levyStatus.ToString().ToLower()}"), Times.Never);
    }
}