using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccount;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests;

public class WhenProviderAddedSuccessfully : EmployerAccountControllerTestsBase
{
    [Test, RecursiveMoqAutoData]
    public async Task ThenTaskListIsCompletedAndUserRedirectedToAccountCreated(
        string userId,
        string hashedAccountId,
        int accountId,
        string publicHashedId,
        GetEmployerAccountByIdResponse getEmployerAccountByIdResponse,
        [Frozen] Mock<IMediator> mediatorMock,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        [Greedy] EmployerAccountController sut)
    {
        SetControllerContextUserIdClaim(userId, sut);
        encodingServiceMock.Setup(e => e.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);
        encodingServiceMock.Setup(e => e.Encode(accountId, EncodingType.PublicAccountId)).Returns(publicHashedId);
        mediatorMock.Setup(m => m.Send(It.Is<GetEmployerAccountByIdQuery>(q => q.AccountId == accountId && q.UserId == userId), It.IsAny<CancellationToken>())).ReturnsAsync(getEmployerAccountByIdResponse);

        var result = await sut.AddedTrainingProvider(hashedAccountId);

        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.CreateAccountSuccess);
    }
}
