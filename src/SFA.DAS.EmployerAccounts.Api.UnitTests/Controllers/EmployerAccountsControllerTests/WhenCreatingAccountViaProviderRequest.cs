using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Commands.AcknowledgeTrainingProviderTask;
using SFA.DAS.EmployerAccounts.Commands.CreateAccount;
using SFA.DAS.EmployerAccounts.Commands.UpsertRegisteredUser;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.EmployerAccountsControllerTests;

public class WhenCreatingAccountViaProviderRequest : EmployerAccountsControllerTests
{
    [Test, AutoData]
    public async Task Then_Creates_User(CreateEmployerAccountViaProviderRequestModel model, CreateAccountCommandResponse createAccountResponse, CancellationToken cancellationToken)
    {
        MediatorMock.Setup(m => m.Send(It.IsAny<CreateAccountCommand>(), cancellationToken)).ReturnsAsync(createAccountResponse);

        await Controller.CreateEmployerAccountViaProviderRequest(model, cancellationToken);

        MediatorMock.Verify(
            m => m.Send(It.Is<UpsertRegisteredUserCommand>(c =>
                c.CorrelationId == model.RequestId.ToString() &&
                c.EmailAddress == model.Email &&
                c.FirstName == model.FirstName &&
                c.LastName == model.LastName &&
                c.UserRef == model.UserRef.ToString()),
            cancellationToken), Times.Once);
    }

    [Test, AutoData]
    public async Task Then_Creates_Employer_Account(CreateEmployerAccountViaProviderRequestModel model, CreateAccountCommandResponse createAccountResponse, CancellationToken cancellationToken)
    {
        MediatorMock.Setup(m => m.Send(It.IsAny<CreateAccountCommand>(), cancellationToken)).ReturnsAsync(createAccountResponse);

        await Controller.CreateEmployerAccountViaProviderRequest(model, cancellationToken);

        MediatorMock.Verify(
            m => m.Send(It.Is<CreateAccountCommand>(c =>
            c.IsViaProviderRequest &&
            c.CorrelationId == model.RequestId.ToString() &&
            c.ExternalUserId == model.UserRef.ToString() &&
            c.OrganisationType == OrganisationType.PensionsRegulator &&
            c.OrganisationName == model.EmployerOrganisationName &&
            c.OrganisationAddress == model.EmployerAddress &&
            c.PayeReference == model.EmployerPaye &&
            c.Aorn == model.EmployerAorn &&
            c.OrganisationReferenceNumber == model.EmployerOrganisationReferenceNumber &&
            c.OrganisationStatus == "active" &&
            c.EmployerRefName == model.EmployerOrganisationName),
            cancellationToken), Times.Once);
    }

    [Test, AutoData]
    public async Task Then_Acknowledges_Provider_Task(CreateEmployerAccountViaProviderRequestModel model, CreateAccountCommandResponse createAccountResponse, CancellationToken cancellationToken)
    {
        MediatorMock.Setup(m => m.Send(It.IsAny<CreateAccountCommand>(), cancellationToken)).ReturnsAsync(createAccountResponse);

        await Controller.CreateEmployerAccountViaProviderRequest(model, cancellationToken);

        MediatorMock.Verify(m => m.Send(It.Is<AcknowledgeTrainingProviderTaskCommand>(c => c.AccountId == createAccountResponse.AccountId), cancellationToken), Times.Once);
    }

    [Test, AutoData]
    public async Task Then_Returns_Created_Response(CreateEmployerAccountViaProviderRequestModel model, CreateAccountCommandResponse createAccountResponse, CancellationToken cancellationToken)
    {
        MediatorMock.Setup(m => m.Send(It.IsAny<CreateAccountCommand>(), cancellationToken)).ReturnsAsync(createAccountResponse);

        var result = await Controller.CreateEmployerAccountViaProviderRequest(model, cancellationToken);

        result.As<CreatedResult>().Should().NotBeNull();
    }
}
