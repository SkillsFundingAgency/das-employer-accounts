﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementTemplates;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetEmployerAgreementTemplatesTests;

[TestFixture]
public class WhenIGetEmployerAgreementTemplates
{
    private Mock<IEmployerAgreementTemplatesRepository> _repository;
    private GetEmployerAgreementTemplatesRequest Request { get; set; }

    private GetEmployerAgreementTemplatesHandler RequestHandler { get; set; }
    private GetEmployerAgreementTemplatesResponse _expectedResponse;
    private List<EmployerAgreementTemplate> EmployerAgreementTemplates { get; set; }

    [SetUp]
    public void Arrange()
    {
        EmployerAgreementTemplates = new List<EmployerAgreementTemplate>
        {
            new() { CreatedDate = DateTime.Now, Id = 1, PartialViewName = "_partial1_", VersionNumber = 1 },
            new() { CreatedDate = DateTime.Now.AddDays(-1), Id = 2, PartialViewName = "_parital2_", VersionNumber = 1 }
        };

        _expectedResponse = new GetEmployerAgreementTemplatesResponse
        {
            EmployerAgreementTemplates = EmployerAgreementTemplates
        };

        _repository = new Mock<IEmployerAgreementTemplatesRepository>();
        _repository
            .Setup(x => x.GetEmployerAgreementTemplates())
            .ReturnsAsync(EmployerAgreementTemplates);

        Request = new GetEmployerAgreementTemplatesRequest();

        RequestHandler = new GetEmployerAgreementTemplatesHandler(_repository.Object);
    }


    [Test]
    public async Task ThenTheRepositoryIsCalled()
    {
        //Act
        var cancellationToken = new CancellationToken();
        await RequestHandler.Handle(Request, cancellationToken);

        //Assert
        _repository.Verify(x => x.GetEmployerAgreementTemplates(), Times.Once);
    }

    [Test]
    public async Task ThenTheExpectedResponseIsReturned()
    {
        //Act
        var cancellationToken = new CancellationToken();
        var response = await RequestHandler.Handle(Request, cancellationToken);

        //Assert
        response.Should().BeEquivalentTo(_expectedResponse);
    }
}
