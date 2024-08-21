using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Controllers;
using SFA.DAS.EmployerAccounts.Models.PAYE;
using SFA.DAS.EmployerAccounts.Queries.GetPayeSchemeAccountByRef;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.PayeControllerTests;

[TestFixture]
public class WhenIGetAPayeSchemeAccount
{
    [Test, MoqAutoData]
    public async Task ThenThePayeSchemeAccountIsReturned(
        Models.PAYE.PayeScheme payeSchemeResponse,
        [Frozen] Mock<IMediator> mediatorMock,
        [NoAutoProperties] PayeController sut,
        CancellationToken cancellationToken
    )
    {
        // Arrange
        mediatorMock
            .Setup(x => x.Send(It.IsAny<GetPayeSchemeAccountByRefQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(payeSchemeResponse);

        var schemeRef = $"{RandomNumberGenerator.GetInt32(100, 999)}/REF";
        var encodedRef = schemeRef.Replace(@"/", "%2f");

        // Act
        var response = await sut.GetAccountHistoryByRef(encodedRef, cancellationToken);

        // Assert
        var result = response.Should().BeAssignableTo<OkObjectResult>();
        var model = result.Subject.Value as Types.AccountHistory;

        model.Should().NotBeNull();

        model!.AccountId.Should().Be(payeSchemeResponse.AccountId);
        model.AddedDate.Should().Be(payeSchemeResponse.AddedDate);
        model.RemovedDate.Should().Be(payeSchemeResponse.RemovedDate);

    }

    [Test, MoqAutoData]
    public async Task AndThePayeSchemeAccountDoesNotExistThenItIsNotReturned(
        [Frozen] Mock<IMediator> mediatorMock,
        CancellationToken cancellationToken
        )
    {
        var schemeRef = $"{RandomNumberGenerator.GetInt32(100, 999)}/REF";
        var encodedRef = schemeRef.Replace(@"/", "%2f");
        mediatorMock
            .Setup(x => x.Send(It.Is<GetPayeSchemeAccountByRefQuery>(p => p.Ref == schemeRef),
                cancellationToken))
            .ReturnsAsync((PayeScheme)null);

        var sut = new PayeController(mediatorMock.Object);

        // Act
        var response = await sut.GetAccountHistoryByRef(encodedRef, cancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.Should().BeAssignableTo<NotFoundResult>();
    }
}