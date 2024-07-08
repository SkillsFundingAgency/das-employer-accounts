using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Controllers;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Queries.GetAccountPayeSchemes;
using SFA.DAS.EmployerAccounts.Queries.GetPayeSchemeByRef;
using SFA.DAS.EmployerAccounts.TestCommon.Extensions;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.AccountPayeSchemesControllerTests;

[TestFixture]
public class WhenIGetAPayeScheme
{
    [Test, MoqAutoData]
    public async Task ThenThePayeSchemesAreReturned(
        string hashedAccountId,
        long accountId,
        GetAccountPayeSchemesResponse accountResponse,
        [Frozen] Mock<IMediator> mediatorMock,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        [Frozen] Mock<IUrlHelper> urlHelperMock,
        [NoAutoProperties] AccountPayeSchemesController sut)
    {
        // Arrange
        accountResponse.PayeSchemes.RemoveAt(2);

        sut.Url = urlHelperMock.Object;

        mediatorMock
            .Setup(x => x.Send(It.Is<GetAccountPayeSchemesQuery>(q => q.AccountId == accountId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(accountResponse);

        encodingServiceMock
            .Setup(x => x.Decode(hashedAccountId, EncodingType.AccountId))
            .Returns(accountId);

        foreach (var scheme in accountResponse.PayeSchemes)
        {
            scheme.Ref = $"{RandomNumberGenerator.GetInt32(100, 999)}/REF";

            urlHelperMock
                .Setup(
                    x => x.RouteUrl(
                        It.Is<UrlRouteContext>(c =>
                            c.RouteName == "GetPayeScheme" &&
                            c.Values.IsEquivalentTo(new
                            {
                                accountId,
                                payeSchemeRef = Uri.EscapeDataString(scheme.Ref)
                            })))
                ).Returns($"/api/accounts/{accountId}/payeschemes/scheme?payeSchemeRef={scheme.Ref.Replace(@"/", "%2f")}");
        }

        // Act
        var response = await sut.GetPayeSchemes(accountId);
        
        // Assert
        var result = response.Should().BeAssignableTo<OkObjectResult>();
        var model = result.Subject.Value as ResourceList;
        model.Should().NotBeNull();

        foreach (var payeScheme in accountResponse.PayeSchemes)
        {
            var matchedScheme = model.Single(x => x.Id == payeScheme.Ref);
            matchedScheme?.Href.Should()
                .Be($"/api/accounts/{accountId}/payeschemes/scheme?payeSchemeRef={payeScheme.Ref.Replace(@"/", "%2f")}");
        }
    }

    [Test, MoqAutoData]
    public async Task AndTheAccountDoesNotExistThenItIsNotReturned(
        long accountId,
        [NoAutoProperties] AccountPayeSchemesController sut)
    {
        // Act
        var response = await sut.GetPayeSchemes(accountId);

        // Asset
        response.Should().NotBeNull();
        response.Should().BeAssignableTo<NotFoundResult>();
    }

    [Test, MoqAutoData]
    public async Task ThenTheAccountIsReturned(
        long accountId,
        string payeSchemeRef,
        GetPayeSchemeByRefResponse payeSchemeResponse,
        [Frozen] Mock<IMediator> mediatorMock,
        [NoAutoProperties] AccountPayeSchemesController sut)
    {
        mediatorMock
            .Setup(x => x.Send(
                It.Is<GetPayeSchemeByRefQuery>(q => q.Ref == payeSchemeRef && q.AccountId == accountId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(payeSchemeResponse);

        var response = await sut.GetPayeScheme(accountId, payeSchemeRef.Replace("/", "%2f")) as OkObjectResult;

        var result = response.Should().BeAssignableTo<OkObjectResult>();
        var model = result.Subject.Value as PayeScheme;

        model.Should().NotBeNull();
        model.Should().BeEquivalentTo(payeSchemeResponse.PayeScheme);
        model?.AccountId.Should().Be(accountId);
    }

    [Test, MoqAutoData]
    public async Task AndThePayeSchemeDoesNotExistThenItIsNotReturned(
        long accountId,
        string payeSchemeRef,
        GetPayeSchemeByRefResponse payeSchemeResponse,
        [Frozen] Mock<IMediator> mediatorMock,
        [NoAutoProperties] AccountPayeSchemesController sut)
    {
        payeSchemeResponse.PayeScheme = null;

        mediatorMock
            .Setup(x => x.Send(
                It.Is<GetPayeSchemeByRefQuery>(q => q.Ref == payeSchemeRef && q.AccountId == accountId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(payeSchemeResponse);

        var response = await sut.GetPayeScheme(accountId, payeSchemeRef);

        response.Should().BeAssignableTo<NotFoundResult>();
    }
}