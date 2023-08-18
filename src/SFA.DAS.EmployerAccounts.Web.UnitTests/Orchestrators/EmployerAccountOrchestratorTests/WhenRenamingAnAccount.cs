using AutoFixture.NUnit3;
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
        Assert.AreEqual(account.HashedId, response.Data.HashedId);
        Assert.AreEqual(account.Name, response.Data.Name);
        Assert.AreEqual(HttpStatusCode.OK, response.Status);
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
        Assert.IsInstanceOf<OrchestratorResponse<RenameEmployerAccountViewModel>>(response);

        mediatorMock.Verify(x => x.Send(It.Is<RenameEmployerAccountCommand>(c => c.NewName == "New Account Name"), It.IsAny<CancellationToken>()), Times.Once());
    }
}