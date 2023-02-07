﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.AddPayeToAccount;
using SFA.DAS.EmployerAccounts.Commands.CreateAccount;
using SFA.DAS.EmployerAccounts.Commands.CreateLegalEntity;
using SFA.DAS.EmployerAccounts.Commands.RenameEmployerAccount;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.EmployerAccounts.Web.Models;
using SFA.DAS.EmployerAccounts.Web.Orchestrators;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.EmployerAccountOrchestratorTests.Given_User_Account_Has_Been_Created;

public class WhenCreatingTheAccount
{
    private EmployerAccountOrchestrator _employerAccountOrchestrator;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<EmployerAccountOrchestrator>> _logger;
    private Mock<ICookieStorageService<EmployerAccountData>> _cookieService;

    private EmployerAccountsConfiguration _configuration;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<EmployerAccountOrchestrator>>();
        _cookieService = new Mock<ICookieStorageService<EmployerAccountData>>();
        _configuration = new EmployerAccountsConfiguration();

        _employerAccountOrchestrator = new EmployerAccountOrchestrator(_mediator.Object, _logger.Object, _cookieService.Object, _configuration);
        _mediator.Setup(x => x.Send(It.IsAny<CreateLegalEntityCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new CreateLegalEntityCommandResponse { AgreementView = new EmployerAgreementView() });
        _mediator.Setup(x => x.Send(It.IsAny<CreateAccountCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateAccountCommandResponse()
            {
                HashedAccountId = "ABS10"
            });
    }

    [Test]
    public async Task Then_New_Account_Is_Not_Created()
    {
        await _employerAccountOrchestrator.CreateOrUpdateAccount(ArrangeModel(),
            It.IsAny<HttpContext>());

        _mediator
            .Verify(
                x => x.Send(It.IsAny<CreateAccountCommand>(), It.IsAny<CancellationToken>()),
                Times.Never);
    }

    [Test]
    public async Task Then_Paye_Information_Is_Added_To_Existing_Account()
    {
        var requestModel = ArrangeModel();

        await _employerAccountOrchestrator.CreateOrUpdateAccount(requestModel,
            It.IsAny<HttpContext>());

        _mediator.Verify(x => x.Send(It.Is<AddPayeToAccountCommand>(c => c.HashedAccountId.Equals(requestModel.HashedAccountId.Value)), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Then_Legal_Entity_Is_Added_To_Existing_Account()
    {
        var requestModel = ArrangeModel();
        var expectedHashedAgreementId = "LKJDDSF";

        _mediator
            .Setup(x => x.Send(It.Is<CreateLegalEntityCommand>(c =>
                    c.HashedAccountId == requestModel.HashedAccountId.Value &&
                    c.Code == requestModel.OrganisationReferenceNumber &&
                    c.DateOfIncorporation == requestModel.OrganisationDateOfInception &&
                    c.Status == requestModel.OrganisationStatus &&
                    c.Source == requestModel.OrganisationType &&
                    c.PublicSectorDataSource == Convert.ToByte(requestModel.PublicSectorDataSource) &&
                    c.Sector == requestModel.Sector &&
                    c.Name == requestModel.OrganisationName &&
                    c.Address == requestModel.OrganisationAddress &&
                    c.ExternalUserId == requestModel.UserId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateLegalEntityCommandResponse { AgreementView = new EmployerAgreementView { HashedAgreementId = expectedHashedAgreementId } });

        var result = await _employerAccountOrchestrator.CreateOrUpdateAccount(requestModel, It.IsAny<HttpContext>());

        Assert.AreEqual(expectedHashedAgreementId, result.Data.EmployerAgreement.HashedAgreementId);
    }

    [Test]
    public async Task Then_Account_Name_Is_Updated()
    {
        var requestModel = ArrangeModel();

        await _employerAccountOrchestrator.CreateOrUpdateAccount(ArrangeModel(),
            It.IsAny<HttpContext>());

        _mediator.Verify(x => x.Send(It.Is<RenameEmployerAccountCommand>(c => c.HashedAccountId.Equals(requestModel.HashedAccountId.Value) 
                                                                              && c.NewName.Equals(requestModel.OrganisationName)),
            It.IsAny<CancellationToken>()));
    }

    private static CreateAccountModel ArrangeModel()
    {
        return new CreateAccountModel
        {
            OrganisationName = "test",
            UserId = Guid.NewGuid().ToString(),
            PayeReference = "123ADFC",
            OrganisationReferenceNumber = "12345",
            OrganisationDateOfInception = new DateTime(2016, 10, 30),
            OrganisationAddress = "My Address",
            AccessToken = Guid.NewGuid().ToString(),
            RefreshToken = Guid.NewGuid().ToString(),
            OrganisationStatus = "active",
            EmployerRefName = "Scheme 1",
            HashedAccountId = new HashedAccountIdModel { Value = "WX4LB" }
        };
    }
}