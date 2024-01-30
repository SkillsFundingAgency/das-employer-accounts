using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.UnitTests.Models.Accounts
{
    public class TaskSummaryTests
    {
        [Test]
        public void HasAnyTask_Should_Return_True_When_ShowLevyDeclarationTask_IsTrue()
        {
            // Arrange
            var taskSummary = new TaskSummary
            {
                ShowLevyDeclarationTask = true,
                NumberOfApprenticesToReview = 0,
                NumberOfCohortsForApproval = 0,
                NumberOfPendingTransferConnections = 0,
                NumberOfTransferRequestToReview = 0,
                NumberTransferPledgeApplicationsToReview = 0
            };

            // Act
            var result = taskSummary.HasAnyTask();

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void HasAnyTask_Should_Return_False_When_All_Task_Properties_Are_False_And_Equal_To_Zero()
        {
            // Arrange
            var taskSummary = new TaskSummary();

            // Act
            var result = taskSummary.HasAnyTask();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void HasAnyTask_Should_Return_True_When_Any_Task_Property_Is_Greater_Than_Zero()
        {
            // Arrange
            var taskSummary = new TaskSummary
            {
                ShowLevyDeclarationTask = false,
                NumberOfApprenticesToReview = 0,
                NumberOfCohortsForApproval = 0,
                NumberOfPendingTransferConnections = 1,
                NumberOfTransferRequestToReview = 0,
                NumberTransferPledgeApplicationsToReview = 0
            };

            // Act
            var result = taskSummary.HasAnyTask();

            // Assert
            result.Should().BeTrue();
        }
    }
}
