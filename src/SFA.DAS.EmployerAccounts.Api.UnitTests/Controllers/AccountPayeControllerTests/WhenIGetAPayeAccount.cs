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
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Queries.GetPayeAccountByRef;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.AccountPayeControllerTests;

[TestFixture]
public class WhenIGetAPayeAccount
{
    [Test, MoqAutoData]
    public async Task ThenThePayeAccountIsReturned(
        GetPayeAccountByRefResponse accountResponse,
        [Frozen] Mock<IMediator> mediatorMock,
        [NoAutoProperties] AccountPayeController sut,
        CancellationToken cancellationToken
    )
    {
        // Arrange
        mediatorMock
            .Setup(x => x.Send(It.IsAny<GetPayeAccountByRefQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(accountResponse);

        var schemeRef = $"{RandomNumberGenerator.GetInt32(100, 999)}/REF";
        var encodedRef = schemeRef.Replace(@"/", "%2f");

        // Act
        var response = await sut.GetPayeAccountDetails(encodedRef, cancellationToken);

        // Assert
        var result = response.Should().BeAssignableTo<OkObjectResult>();
        var model = result.Subject.Value as PayeAccount;

        model.Should().NotBeNull();

        model!.AccountId.Should().Be(accountResponse.AccountId);
        model.AddedDate.Should().Be(accountResponse.AddedDate);
        model.RemovedDate.Should().Be(accountResponse.RemovedDate);

    }

    [Test, MoqAutoData]
    public async Task AndThePayeAccountDoesNotExistThenItIsNotReturned(
        [Frozen] Mock<IMediator> mediatorMock,
        CancellationToken cancellationToken
        )
    {
        var schemeRef = $"{RandomNumberGenerator.GetInt32(100, 999)}/REF";
        var encodedRef = schemeRef.Replace(@"/", "%2f");
        mediatorMock
            .Setup(x => x.Send(It.Is<GetPayeAccountByRefQuery>(p => p.Ref == schemeRef),
                cancellationToken))
            .ReturnsAsync((GetPayeAccountByRefResponse)null);

        var sut = new AccountPayeController(mediatorMock.Object);

        // Act
        var response = await sut.GetPayeAccountDetails(encodedRef, cancellationToken);

        // Assert
        response.Should().NotBeNull();
        response.Should().BeAssignableTo<NotFoundResult>();
    }
}