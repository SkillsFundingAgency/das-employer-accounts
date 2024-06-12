using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using SFA.DAS.EmployerAccounts.Queries.GetUserAccounts;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;
using SFA.DAS.EmployerAccounts.Queries.GetUserInvitations;
using SFA.DAS.EmployerAccounts.TestCommon.AutoFixture;
using SFA.DAS.EmployerAccounts.Web.Extensions;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.HomeOrchestratorTests;

public class WhenGettingUserAccounts
{
    [Test, DomainAutoData]
    public async Task Then_ShouldGetUserAccountInformation(
            string userId,
            GetUserAccountsQueryResponse getUserAccountsQueryResponse,
            GetNumberOfUserInvitationsResponse numberOfUserInvitationsResponse,
            GetUserByRefResponse userByRefResponse,
            [Frozen] Mock<IMediator> mediatorMock,
            [NoAutoProperties] HomeOrchestrator sut)
    {
        // Arrange
        mediatorMock.Setup(m => m.Send(It.Is<GetUserAccountsQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>())).ReturnsAsync(getUserAccountsQueryResponse);
        mediatorMock.Setup(m => m.Send(It.Is<GetNumberOfUserInvitationsQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>())).ReturnsAsync(numberOfUserInvitationsResponse);
        mediatorMock.Setup(m => m.Send(It.Is<GetUserByRefQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>())).ReturnsAsync(userByRefResponse);

        // Act
        await sut.GetUserAccounts(userId, null, null, null, null);
        
        // Assert
        mediatorMock.Verify(m => m.Send(It.IsAny<GetUserAccountsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        mediatorMock.Verify(m => m.Send(It.IsAny<GetNumberOfUserInvitationsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        mediatorMock.Verify(m => m.Send(It.IsAny<GetUserByRefQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, DomainAutoData]
    public async Task Then_ShouldReturnRedirectUri_WhenRedirectUriIsValid(
            string userId,
            GetUserAccountsQueryResponse getUserAccountsQueryResponse,
            GetNumberOfUserInvitationsResponse numberOfUserInvitationsResponse,
            GetUserByRefResponse userByRefResponse,
            [Frozen] Mock<IMediator> mediatorMock,
            [NoAutoProperties] HomeOrchestrator sut)
    {
        // Arrange
        mediatorMock.Setup(m => m.Send(It.Is<GetUserAccountsQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>())).ReturnsAsync(getUserAccountsQueryResponse);
        mediatorMock.Setup(m => m.Send(It.Is<GetNumberOfUserInvitationsQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>())).ReturnsAsync(numberOfUserInvitationsResponse);
        mediatorMock.Setup(m => m.Send(It.Is<GetUserByRefQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>())).ReturnsAsync(userByRefResponse);

        var redirectUri = new Uri("https://somewhereovertherainbow?param=1&otherparam=2");
        var redirectDescription = "land of oz";
        
        // Act
        var result = await sut.GetUserAccounts(userId, null,
            redirectUri.ToString(),
            new List<RedirectUriConfiguration>
            {
                new RedirectUriConfiguration
                {
                    Uri = redirectUri.RemoveQuery(),
                    Description = redirectDescription
                }
            }, null);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.RedirectUri.Should().Be(redirectUri.ToString());
        result.Data.RedirectDescription.Should().Be(redirectDescription);
    }

    [Test, DomainAutoData]
    public async Task Then_ShouldReturnNullRedirectUri_WhenRedirectUriIsInvalid(
            string userId,
            GetUserAccountsQueryResponse getUserAccountsQueryResponse,
            GetNumberOfUserInvitationsResponse numberOfUserInvitationsResponse,
            GetUserByRefResponse userByRefResponse,
            [Frozen] Mock<IMediator> mediatorMock,
            [NoAutoProperties] HomeOrchestrator sut)
    {
        // Arrange
        mediatorMock.Setup(m => m.Send(It.Is<GetUserAccountsQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>())).ReturnsAsync(getUserAccountsQueryResponse);
        mediatorMock.Setup(m => m.Send(It.Is<GetNumberOfUserInvitationsQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>())).ReturnsAsync(numberOfUserInvitationsResponse);
        mediatorMock.Setup(m => m.Send(It.Is<GetUserByRefQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>())).ReturnsAsync(userByRefResponse);

        var redirectUri = new Uri("https://somewhereovertherainbow?param=1&otherparam=2");
        var configuredRedirectUri = new Uri("https://singingintherain/");
        var configuredRedirectDescription = "land of oz";

        // Act
        var result = await sut.GetUserAccounts(userId, null,
            redirectUri.ToString(),
            new List<RedirectUriConfiguration>
            {
                new RedirectUriConfiguration
                {
                    Uri = configuredRedirectUri.ToString(),
                    Description = configuredRedirectDescription
                }
            }, null);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.RedirectUri.Should().BeNull();
        result.Data.RedirectDescription.Should().BeNull();
    }

    [Test, DomainAutoData]
    public async Task Then_ShouldReturnNullRedirectUri_WhenNoRedirectUrisAreConfigured(
            string userId,
            GetUserAccountsQueryResponse getUserAccountsQueryResponse,
            GetNumberOfUserInvitationsResponse numberOfUserInvitationsResponse,
            GetUserByRefResponse userByRefResponse,
            [Frozen] Mock<IMediator> mediatorMock,
            [NoAutoProperties] HomeOrchestrator sut)
    {
        // Arrange
        mediatorMock.Setup(m => m.Send(It.Is<GetUserAccountsQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>())).ReturnsAsync(getUserAccountsQueryResponse);
        mediatorMock.Setup(m => m.Send(It.Is<GetNumberOfUserInvitationsQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>())).ReturnsAsync(numberOfUserInvitationsResponse);
        mediatorMock.Setup(m => m.Send(It.Is<GetUserByRefQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>())).ReturnsAsync(userByRefResponse);

        var redirectUri = new Uri("https://somewhereovertherainbow?param=1&otherparam=2");

        // Act
        var result = await sut.GetUserAccounts(userId, null,
            redirectUri.ToString(),
            new List<RedirectUriConfiguration> { }, null);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.RedirectUri.Should().BeNull();
        result.Data.RedirectDescription.Should().BeNull();
    }

    [Test, DomainAutoData]
    public async Task Then_ShouldReturnNullRedirectUri_WhenRedirectUrisAreMissing(
            string userId,
            GetUserAccountsQueryResponse getUserAccountsQueryResponse,
            GetNumberOfUserInvitationsResponse numberOfUserInvitationsResponse,
            GetUserByRefResponse userByRefResponse,
            [Frozen] Mock<IMediator> mediatorMock,
            [NoAutoProperties] HomeOrchestrator sut)
    {
        // Arrange
        mediatorMock.Setup(m => m.Send(It.Is<GetUserAccountsQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>())).ReturnsAsync(getUserAccountsQueryResponse);
        mediatorMock.Setup(m => m.Send(It.Is<GetNumberOfUserInvitationsQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>())).ReturnsAsync(numberOfUserInvitationsResponse);
        mediatorMock.Setup(m => m.Send(It.Is<GetUserByRefQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>())).ReturnsAsync(userByRefResponse);

        var redirectUri = new Uri("https://somewhereovertherainbow?param=1&otherparam=2");

        // Act
        var result = await sut.GetUserAccounts(userId, null,
            redirectUri.ToString(),
            null, null);

        // Assert
        result.Data.Should().NotBeNull();
        result.Data.RedirectUri.Should().BeNull();
        result.Data.RedirectDescription.Should().BeNull();
    }
}