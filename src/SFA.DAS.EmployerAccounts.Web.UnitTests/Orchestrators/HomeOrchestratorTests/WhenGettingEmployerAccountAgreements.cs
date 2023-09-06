using AutoFixture.NUnit3;
using MediatR;
using FluentAssertions;
using SFA.DAS.EmployerAccounts.Queries.GetAccountEmployerAgreements;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.HomeOrchestratorTests;

public class WhenGettingEmployerAccountAgreements
{
    [Test, MoqAutoData]
    public async Task Then_Should_Get_Employer_Account_Agreements(
        long accountId,
        string externalUserId,
        [Frozen] Mock<IMediator> mediatorMock,
        [Greedy] HomeOrchestrator orchestrator)
    {
        // Arrange
        
        
        // Act
        _ = await orchestrator.GetEmployerAccountAgreements(accountId, externalUserId);

        // Assert
        mediatorMock.Verify(m => 
            m.Send(
                It.Is<GetAccountEmployerAgreementsRequest>(x => x.AccountId == accountId && x.ExternalUserId == externalUserId), 
                It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task Then_Should_Return_Invalid_Request_On_Error(
        long accountId,
        string externalUserId,
        [Frozen] Mock<IMediator> mediatorMock,
        [Greedy] HomeOrchestrator orchestrator)
    {
        // Arrange
        mediatorMock.Setup(m => 
            m.Send(
                It.Is<GetAccountEmployerAgreementsRequest>(x => x.AccountId == accountId && x.ExternalUserId == externalUserId), 
                It.IsAny<CancellationToken>())).Throws(new Exception());
        
        // Act
        var response = await orchestrator.GetEmployerAccountAgreements(accountId, externalUserId);

        // Assert
        response.Status.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Test, MoqAutoData]
    public async Task Then_Should_Return_Response(
        long accountId,
        string externalUserId,
        GetAccountEmployerAgreementsResponse getAccountEmployerAgreementsResponse,
        [Frozen] Mock<IMediator> mediatorMock,
        [Greedy] HomeOrchestrator orchestrator)
    {
        // Arrange
        mediatorMock.Setup(m => 
            m.Send(
                It.Is<GetAccountEmployerAgreementsRequest>(x => x.AccountId == accountId && x.ExternalUserId == externalUserId), 
                It.IsAny<CancellationToken>())).ReturnsAsync(getAccountEmployerAgreementsResponse);
        
        // Act
        var response = await orchestrator.GetEmployerAccountAgreements(accountId, externalUserId);

        // Assert
        response.Status.Should().Be(HttpStatusCode.OK);
        response.Data.EmployerAgreementsData.Should().BeEquivalentTo(getAccountEmployerAgreementsResponse);
    }
}