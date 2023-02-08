﻿namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerTeamControllerTests;

public class WhenIChooseIIfIKnowWhichCourseTheApprenticeWillTake
{
    private EmployerTeamController _controller;

    private Mock<ICookieStorageService<FlashMessageViewModel>> _mockCookieStorageService;
    private Mock<EmployerTeamOrchestrator> _mockEmployerTeamOrchestrator;

    [SetUp]
    public void Arrange()
    {
        _mockCookieStorageService = new Mock<ICookieStorageService<FlashMessageViewModel>>();
        _mockEmployerTeamOrchestrator = new Mock<EmployerTeamOrchestrator>();

        _controller = new EmployerTeamController(
            _mockCookieStorageService.Object,
            _mockEmployerTeamOrchestrator.Object,
            Mock.Of<IMultiVariantTestingService>());
    }

    [Test]
    public void IfIChooseYesIContinueTheJourney()
    {
        //Act
        var result = _controller.TriageWhichCourseYourApprenticeWillTake(new TriageViewModel { TriageOption = TriageOptions.Yes }) as RedirectToActionResult;

        //Assert
        Assert.AreEqual(ControllerConstants.TriageHaveYouChosenATrainingProviderActionName, result.ActionName);
    }

    [Test]
    public void IfIChooseNoICannotSetupAnApprentice()
    {
        //Act
        var result = _controller.TriageWhichCourseYourApprenticeWillTake(new TriageViewModel { TriageOption = TriageOptions.No }) as RedirectToActionResult;

        //Assert
        Assert.AreEqual(ControllerConstants.TriageYouCannotSetupAnApprenticeshipYetCourseProviderActionName, result.ActionName);
    }
}