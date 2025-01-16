﻿using System.Collections.Generic;
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
        taskSummaryResponse.SingleApprovedTransferPledgeId = pledgeId;

        validator.Setup(v => v.Validate(request))
            .Returns(new ValidationResult { IsUnauthorized = false, ValidationDictionary = new Dictionary<string, string>() });

        encodingService.Setup(x => x.Encode(taskSummaryResponse.SingleApprovedTransferPledgeId.Value, EncodingType.PledgeId)).Returns(hashedPledgeId);

        employerAccountService
            .Setup(m => m.GetTaskSummary(It.IsAny<long>()))
            .ReturnsAsync(taskSummaryResponse);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.TaskSummary.SingleApprovedTransferHashedPledgeId.Should().Be(hashedPledgeId);

        encodingService.Verify(x => x.Encode(taskSummaryResponse.SingleApprovedTransferPledgeId.Value, EncodingType.PledgeId), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task Then_SingleApprovedTransferPledgeId_Should_Be_Null_When_SingleApprovedTransferPledgeId_Is_Null(
        [Frozen] Mock<IEmployerAccountService> employerAccountService,
        [Frozen] Mock<IValidator<GetTaskSummaryQuery>> validator,
        [Frozen] Mock<IEncodingService> encodingService,
        string hashedPledgeId,
        GetTaskSummaryQuery request,
        GetTaskSummaryHandler handler)
    {
        var response = new TaskSummary
        {
            SingleApprovedTransferPledgeId = null,
        };
        
        validator.Setup(v => v.Validate(request))
            .Returns(new ValidationResult { IsUnauthorized = false, ValidationDictionary = new Dictionary<string, string>() });

        employerAccountService
            .Setup(m => m.GetTaskSummary(It.IsAny<long>()))
            .ReturnsAsync(response);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.TaskSummary.SingleApprovedTransferHashedPledgeId.Should().BeNullOrEmpty();

        encodingService.Verify(x => x.Encode(It.IsAny<int>(), EncodingType.PledgeId), Times.Never);
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
}