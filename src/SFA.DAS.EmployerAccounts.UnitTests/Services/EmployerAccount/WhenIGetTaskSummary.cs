using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.Accounts;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Responses.Accounts;
using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;
using SFA.DAS.EmployerAccounts.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.UnitTests.Services.EmployerAccount
{
    [TestFixture]
    public class WhenIGetTaskSummary
    {
        [Test, MoqAutoData]
        public async Task GetTaskSummary_Should_Return_TaskSummary_When_Api_Response_Is_Successful(
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        long accountId,
        GetTasksResponse tasksResponse,
        EmployerAccountService employerAccountService)
        {
            // Arrange
            outerApiClient.Setup(m => m.Get<GetTasksResponse>(It.IsAny<GetTasksRequest>()))
                .ReturnsAsync(tasksResponse);

            // Act
            var result = await employerAccountService.GetTaskSummary(accountId);

            // Assert
            result.Should().NotBeNull();
            result.ShowLevyDeclarationTask.Should().Be(tasksResponse.ShowLevyDeclarationTask);
            result.NumberOfCohortsForApproval.Should().Be(tasksResponse.NumberOfCohortsForApproval);
            result.NumberOfApprenticesToReview.Should().Be(tasksResponse.NumberOfApprenticesToReview);
            result.NumberOfPendingTransferConnections.Should().Be(tasksResponse.NumberOfPendingTransferConnections);
            result.NumberOfTransferRequestToReview.Should().Be(tasksResponse.NumberOfTransferRequestToReview);
            result.NumberTransferPledgeApplicationsToReview.Should().Be(tasksResponse.NumberTransferPledgeApplicationsToReview);
            result.UnableToGetTasks.Should().BeFalse();

        }

        [Test, MoqAutoData]
        public async Task GetTaskSummary_Should_Return_Null_When_Api_Response_IsNull(
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        long accountId,
        GetTasksResponse tasksResponse,
        EmployerAccountService employerAccountService)
        {
            // Arrange
            outerApiClient.Setup(m => m.Get<GetTasksResponse>(It.IsAny<GetTasksRequest>()))
                .ReturnsAsync((GetTasksResponse)null);

            // Act
            var result = await employerAccountService.GetTaskSummary(accountId);

            // Assert
            result.Should().BeNull();
        }

        [Test, MoqAutoData]
        public async Task UnableToGetTasks_IsTrue_If_Exception_Thrown(
        [Frozen] Mock<IOuterApiClient> outerApiClient,
        long accountId,
        GetTasksResponse tasksResponse,
        EmployerAccountService employerAccountService)
        {
            // Arrange
            outerApiClient.Setup(m => m.Get<GetTasksResponse>(It.IsAny<GetTasksRequest>()))
            .ThrowsAsync(new Exception("Test outer API exception"));

            // Act
            var result = await employerAccountService.GetTaskSummary(accountId);

            // Assert
            result.UnableToGetTasks.Should().BeTrue();
        }
    }
}
