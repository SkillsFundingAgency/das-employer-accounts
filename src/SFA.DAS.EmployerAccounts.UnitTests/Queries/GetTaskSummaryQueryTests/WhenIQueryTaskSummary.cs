using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.GetTaskSummary;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetTaskSummaryQueryTests;

[TestFixture]
public class WhenIQueryTaskSummary
{
    [Test, MoqAutoData]
    public async Task Then_NumberTransferPledgeApplicationsToReview_Should_Match_Api_Response(
        [Frozen] Mock<IEmployerAccountService> employerAccountService,
        [Frozen] Mock<IValidator<GetTaskSummaryQuery>> validator,
        TaskSummary taskSummaryResponse,
        GetTaskSummaryQuery request,
        GetTaskSummaryHandler handler)
    {
        validator.Setup(v => v.Validate(request))
            .Returns(new ValidationResult { IsUnauthorized = false, ValidationDictionary = new Dictionary<string, string>() });

        employerAccountService
            .Setup(m => m.GetTaskSummary(It.IsAny<long>()))
            .ReturnsAsync(taskSummaryResponse);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.TaskSummary.Should().Be(taskSummaryResponse);
    }

    [Test, MoqAutoData]
    public async Task Then_Return_Empty_TaskSummary_If_ApiResponse_IsNull(
        [Frozen] Mock<IEmployerAccountService> employerAccountService,
        [Frozen] Mock<IValidator<GetTaskSummaryQuery>> validator,
        GetTaskSummaryQuery request,
        GetTaskSummaryHandler handler)
    {
        validator.Setup(v => v.Validate(request))
            .Returns(new ValidationResult { IsUnauthorized = false, ValidationDictionary = new Dictionary<string, string>() });

        employerAccountService
            .Setup(m => m.GetTaskSummary(It.IsAny<long>()))
            .ReturnsAsync((TaskSummary)null);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.TaskSummary.Should().NotBeNull();
        result.TaskSummary.HasAnyTask().Should().BeFalse();
    }
    
    [Test, MoqAutoData]
    public async Task Then_SingleApprovedTransferPledgeId_Should_Be_Populated_When_SingleApprovedTransferPledgeId_Has_Value(
        [Frozen] Mock<IEmployerAccountService> employerAccountService,
        [Frozen] Mock<IValidator<GetTaskSummaryQuery>> validator,
        [Frozen] Mock<IEncodingService> encodingService,
        int pledgeId,
        string hashedPledgeId,
        TaskSummary taskSummaryResponse,
        GetTaskSummaryQuery request,
        GetTaskSummaryHandler handler)
    {
        taskSummaryResponse.SingleAcceptedTransferPledgeApplicationIdWithNoApprentices = pledgeId;

        validator.Setup(v => v.Validate(request))
            .Returns(new ValidationResult { IsUnauthorized = false, ValidationDictionary = new Dictionary<string, string>() });

        encodingService.Setup(x => x.Encode(taskSummaryResponse.SingleAcceptedTransferPledgeApplicationIdWithNoApprentices.Value, EncodingType.PledgeApplicationId)).Returns(hashedPledgeId);

        employerAccountService
            .Setup(m => m.GetTaskSummary(It.IsAny<long>()))
            .ReturnsAsync(taskSummaryResponse);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.TaskSummary.SingleAcceptedTransferPledgeApplicationHashedIdWithNoApprentices.Should().Be(hashedPledgeId);

        encodingService.Verify(x => x.Encode(taskSummaryResponse.SingleAcceptedTransferPledgeApplicationIdWithNoApprentices.Value, EncodingType.PledgeApplicationId), Times.Once);
    }
}