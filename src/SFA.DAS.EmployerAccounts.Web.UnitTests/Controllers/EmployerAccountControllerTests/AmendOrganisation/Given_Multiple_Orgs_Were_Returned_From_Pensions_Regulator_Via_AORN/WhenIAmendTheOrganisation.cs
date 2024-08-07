﻿using MediatR;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.AmendOrganisation.Given_Multiple_Orgs_Were_Returned_From_Pensions_Regulator_Via_AORN;

[TestFixture]
public class WhenIAmendTheOrganisation
{
    private EmployerAccountController _employerAccountController;

    [SetUp]
    public void Setup()
    {
        var orchestrator = new Mock<EmployerAccountOrchestrator>();
        orchestrator.Setup(x => x.GetCookieData()).Returns(new EmployerAccountData
        {
            EmployerAccountOrganisationData = new EmployerAccountOrganisationData { OrganisationType = OrganisationType.PensionsRegulator, PensionsRegulatorReturnedMultipleResults = true },
            EmployerAccountPayeRefData = new EmployerAccountPayeRefData { AORN = "AORN" }
        });

        _employerAccountController = new EmployerAccountController(
            orchestrator.Object,
            Mock.Of<ILogger<EmployerAccountController>>(),
            Mock.Of<ICookieStorageService<FlashMessageViewModel>>(),
            Mock.Of<IMediator>(),
            Mock.Of<ICookieStorageService<ReturnUrlModel>>(),
            Mock.Of<ICookieStorageService<HashedAccountIdModel>>(),
            Mock.Of<LinkGenerator>());
    }

    [TearDown]
    public void TearDown()
    {
        _employerAccountController?.Dispose();
    }

    [Test]
    public void ThenTheAORNPensionRegulatorChooseOrganisationPageIsDisplayed()
    {
        var response = _employerAccountController.AmendOrganisation();
        var redirectResponse = (RedirectToActionResult)response;

        Assert.That(redirectResponse.ActionName, Is.EqualTo(ControllerConstants.SearchUsingAornActionName));
        Assert.That(redirectResponse.ControllerName, Is.EqualTo(ControllerConstants.SearchPensionRegulatorControllerName));
    }
}