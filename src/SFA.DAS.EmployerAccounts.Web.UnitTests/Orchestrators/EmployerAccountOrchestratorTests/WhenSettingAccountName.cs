﻿using AutoFixture.NUnit3;
using MediatR;
using SFA.DAS.EmployerAccounts.Commands.CreateAccountComplete;
using SFA.DAS.EmployerAccounts.Commands.RenameEmployerAccount;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccount;
using SFA.DAS.EmployerAccounts.Queries.GetUserAccountRole;
using SFA.DAS.EmployerAccounts.TestCommon.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.EmployerAccountOrchestratorTests;

public class WhenSettingAccountName
{
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
        var response = await orchestrator.SetEmployerAccountName(account.HashedId, new RenameEmployerAccountViewModel
        {
            ChangeAccountName = true,
            NewName = "New Account Name"
        }, "ABC123");

        //Assert
        Assert.That(response, Is.InstanceOf<OrchestratorResponse<RenameEmployerAccountViewModel>>());

        mediatorMock.Verify(x => x.Send(It.Is<RenameEmployerAccountCommand>(c => c.NewName == "New Account Name"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, DomainAutoData]
    public async Task ThenErrorShouldSendCreateAccountCompleteCommand(
      Account account,
      [Frozen] Mock<IMediator> mediatorMock,
      EmployerAccountOrchestrator orchestrator)
    {
        // Arrange
        mediatorMock
            .Setup(x => x.Send(It.IsAny<GetEmployerAccountByIdQuery>(), It.IsAny<CancellationToken>()))
            .Throws(new Exception());

        //Act
        var response = await orchestrator.SetEmployerAccountName(account.HashedId, new RenameEmployerAccountViewModel
        {
            ChangeAccountName = true,
            NewName = "New Account Name"
        }, "ABC123");

        //Assert
        mediatorMock.Verify(x => x.Send(It.IsAny<SendAccountTaskListCompleteNotificationCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}