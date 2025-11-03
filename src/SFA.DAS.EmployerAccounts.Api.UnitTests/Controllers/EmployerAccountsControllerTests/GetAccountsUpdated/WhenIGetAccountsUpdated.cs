using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Exceptions;
using SFA.DAS.EmployerAccounts.Queries.GetAccountsSinceDate;
using SFA.DAS.EmployerAccounts.Validation;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.EmployerAccountsControllerTests;


[TestFixture]
public class WhenIGetAccountsUpdated : EmployerAccountsControllerTests
{
    /// <summary>
    /// The orchestrator has no interface, so confirming the controller forwards requests through
    /// to the orchestrator via mocking the mediator and verifying that it is called instead
    /// not ideal but avoids adding the interface at this stage
    /// </summary>
    [Test]
    public async Task Then_Calls_Orchestrator_And_Mediator_And_Returns_Ok()
    {
        // Arrange
        var sinceDate = DateTime.UtcNow.AddDays(-1);
        var pageNumber = 2;
        var pageSize = 50;

        MediatorMock
            .Setup(x => x.Send(
                It.Is<GetAccountsSinceDateQuery>(q =>
                    q.SinceDate == sinceDate &&
                    q.PageNumber == pageNumber &&
                    q.PageSize == pageSize),
                It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new GetAccountsSinceDateResponse
            {
                Accounts = new Models.Account.Accounts<Models.Account.AccountNameSummary>
                {
                    AccountList = new List<Models.Account.AccountNameSummary>()
                }
            })
            .Verifiable();

        // Act
        var result = await Controller.GetAccountsUpdated(sinceDate, pageNumber, pageSize);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be((int)HttpStatusCode.OK);

        MediatorMock.Verify(
            x => x.Send(
                It.Is<GetAccountsSinceDateQuery>(q =>
                    q.SinceDate == sinceDate &&
                    q.PageNumber == pageNumber &&
                    q.PageSize == pageSize),
                It.IsAny<System.Threading.CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Then_InvalidRequestException_Returns_BadRequest()
    {
        var validation = new ValidationResult();
        validation.AddError("PageSize", "Must be greater than zero");

        MediatorMock
            .Setup(x => x.Send(It.IsAny<GetAccountsSinceDateQuery>(), It.IsAny<System.Threading.CancellationToken>()))
            .Throws(new InvalidRequestException(validation.ValidationDictionary));

        var result = await Controller.GetAccountsUpdated(DateTime.MinValue, 1, 0);

        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Then_Unhandled_Exception_Returns_InternalServerError()
    {
        MediatorMock
            .Setup(x => x.Send(It.IsAny<GetAccountsSinceDateQuery>(), It.IsAny<System.Threading.CancellationToken>()))
            .Throws(new Exception("Unexpected error"));

        var result = await Controller.GetAccountsUpdated(DateTime.MinValue, 1, 100);

        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }
}
