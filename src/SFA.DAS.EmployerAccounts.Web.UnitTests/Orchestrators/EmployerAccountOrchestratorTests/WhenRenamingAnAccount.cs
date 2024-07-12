using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using SFA.DAS.EmployerAccounts.Commands.RenameEmployerAccount;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccount;
using SFA.DAS.EmployerAccounts.Queries.GetUserAccountRole;
using SFA.DAS.EmployerAccounts.TestCommon.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.EmployerAccountOrchestratorTests;

public class WhenRenamingAnAccount
{
    [Test, DomainAutoData]
    public async Task ThenTheCorrectAccountDetailsShouldBeReturned(
        Account account,
        [Frozen] Mock<IMediator> mediatorMock,
        EmployerAccountOrchestrator orchestrator)
    {
        // Arrange
        mediatorMock.Setup(x => x.Send(It.IsAny<GetEmployerAccountByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetEmployerAccountByIdResponse { Account = account });

        mediatorMock.Setup(x => x.Send(It.IsAny<GetUserAccountRoleQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetUserAccountRoleResponse { UserRole = Role.Owner });

        //Act
        var response = await orchestrator.GetEmployerAccount(account.Id);

        //Assert
        mediatorMock.Verify(x => x.Send(It.Is<GetEmployerAccountByIdQuery>(q => q.AccountId.Equals(account.Id)), It.IsAny<CancellationToken>()));
        response.Data.HashedId.Should().Be(account.HashedId);
        response.Data.Name.Should().Be(account.Name);
        response.Status.Should().Be(HttpStatusCode.OK);
    }

    [Test, DomainAutoData]
    public async Task ThenTheAccountNameShouldBeUpdated(
        Account account, 
        [Frozen] Mock<IMediator> mediatorMock,
        EmployerAccountOrchestrator orchestrator)
    {
        // Arrange
        mediatorMock.Setup(x => x.Send(It.IsAny<GetEmployerAccountByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetEmployerAccountByIdResponse { Account = account });

        mediatorMock.Setup(x => x.Send(It.IsAny<GetUserAccountRoleQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetUserAccountRoleResponse { UserRole = Role.Owner });

        //Act
        var response = await orchestrator.RenameEmployerAccount(account.HashedId, new RenameEmployerAccountViewModel
        {
            ChangeAccountName = true,
            NewName = "New Account Name"
        }, "ABC123");

        //Assert
        response.Should().BeOfType<OrchestratorResponse<RenameEmployerAccountViewModel>>();
        mediatorMock.Verify(x => x.Send(It.Is<RenameEmployerAccountCommand>(c => c.NewName == "New Account Name"), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Test, DomainAutoData]
    public async Task ThenTheAccountNameShouldBeRejectedAsItContainsReservedCharacters(
        Account account,
        [Frozen] Mock<IMediator> mediatorMock,
        EmployerAccountOrchestrator orchestrator)
    {
        // Arrange
        mediatorMock.Setup(x => x.Send(It.IsAny<GetEmployerAccountByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetEmployerAccountByIdResponse { Account = account });

        mediatorMock.Setup(x => x.Send(It.IsAny<GetUserAccountRoleQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetUserAccountRoleResponse { UserRole = Role.Owner });

        //Act
        var response = await orchestrator.RenameEmployerAccount(account.HashedId, new RenameEmployerAccountViewModel
        {
            ChangeAccountName = true,
            NewName = "Is <Invalid>"
        }, "ABC123");

        //Assert
        response.Data.ErrorDictionary.Count().Should().Be(1);
        response.Data.ErrorDictionary["NewName"].Should().Be("New Name must only include letters a to z, numbers 0 to 9, and special characters such as hyphens, spaces and apostrophes");
    }

    [Test, DomainAutoData]
    public async Task ThenTheAccountNameShouldBeRejectedIfBlank(
        Account account,
        [Frozen] Mock<IMediator> mediatorMock,
        EmployerAccountOrchestrator orchestrator)
    {
        // Arrange
        mediatorMock.Setup(x => x.Send(It.IsAny<GetEmployerAccountByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetEmployerAccountByIdResponse { Account = account });

        mediatorMock.Setup(x => x.Send(It.IsAny<GetUserAccountRoleQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetUserAccountRoleResponse { UserRole = Role.Owner });

        //Act
        var response = await orchestrator.RenameEmployerAccount(account.HashedId, new RenameEmployerAccountViewModel
        {
            ChangeAccountName = true,
            NewName = ""
        }, "ABC123");

        //Assert
        response.Data.ErrorDictionary.Count().Should().Be(1);
        response.Data.ErrorDictionary["NewName"].Should().Be("Enter a name, please.....");
    }
}