﻿using AutoFixture.NUnit3;
using FluentAssertions;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.Summary
{
    [TestFixture]
    public class WhenSubmittingOrganisationSummary
    {
        [Test, MoqAutoData]
        public async Task With_No_Option_Should_Return_Error(
            SummaryViewModel viewModel,
            [NoAutoProperties] EmployerAccountController controller)
        {
            // Arrange
            viewModel.IsOrganisationWithCorrectAddress = null;
            controller.ModelState.AddModelError(nameof(viewModel.IsOrganisationWithCorrectAddress), "You have not set");

            // Act
            var result = await controller.Summary(viewModel) as ViewResult;
            var model = result.Model as OrchestratorResponse<SummaryViewModel>;

            // Assert
            result.ViewData.ModelState.IsValid.Should().BeFalse();
            model.Data.Should().BeEquivalentTo(viewModel);
        }

        [Test, MoqAutoData]
        public async Task With_Wrong_Address_Should_Redirect_To_Address_Shutter(
            SummaryViewModel viewModel,
            [NoAutoProperties] EmployerAccountController controller)
        {
            // Arrange
            viewModel.IsOrganisationWithCorrectAddress = false;

            // Act
            var result = await controller.Summary(viewModel) as RedirectToRouteResult;

            // Assert
            result.RouteName.Should().Be(RouteNames.OrganisationWrongAddress);
        }
    }
}
