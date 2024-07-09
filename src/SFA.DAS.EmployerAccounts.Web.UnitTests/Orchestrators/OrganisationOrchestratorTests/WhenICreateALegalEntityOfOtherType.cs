﻿using AutoMapper;
using FluentAssertions;
using MediatR;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.OrganisationOrchestratorTests;

public class WhenICreateALegalEntityOfOtherType
{
    private OrganisationOrchestrator _orchestrator;
    private Mock<IMediator> _mediator;
    private Mock<IMapper> _mapper;
    private Mock<ICookieStorageService<EmployerAccountData>> _cookieService;
    private Mock<IEncodingService> _encodingServiceMock;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _mapper = new Mock<IMapper>();
        _cookieService = new Mock<ICookieStorageService<EmployerAccountData>>();
        _encodingServiceMock = new Mock<IEncodingService>();

        _orchestrator = new OrganisationOrchestrator(
            _mediator.Object,
            _mapper.Object,
            _cookieService.Object,
            _encodingServiceMock.Object);
    }

    [Test]
    public async Task ThenTheNameIsMandatory()
    {
        var request = new OrganisationDetailsViewModel();
        var result = await _orchestrator.ValidateLegalEntityName(request);

        result.Data.Valid.Should().BeFalse();
        result.Data.ErrorDictionary.ContainsKey("Name").Should().BeTrue();
    }

    [Test]
    public async Task ThenTheLegalEntityIsValidIfNameIsProvided()
    {
        var request = new OrganisationDetailsViewModel
        {
            Name = "Test Organisation"
        };
        var result = await _orchestrator.ValidateLegalEntityName(request);

        result.Data.Valid.Should().BeTrue();
    }

    [Test]
    public async Task ThenTheNameCannotContainInvalidChars()
    {
        var request = new OrganisationDetailsViewModel();
        request.Name = "<not Allowed>";
        var result = await _orchestrator.ValidateLegalEntityName(request);

        result.Data.Valid.Should().BeFalse();
        result.Data.ErrorDictionary.ContainsKey("Name").Should().BeTrue();
    }
}