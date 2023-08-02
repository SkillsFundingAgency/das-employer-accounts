using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.CreateAccountComplete;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Testing.Services;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.CreateAccountComplete;

public class WhenICallCreateAccountComplete
{
    private Mock<IMediator> _mediator;
    private Mock<IValidator<CreateAccountCompleteCommand>> _validator;
    private TestableEventPublisher _eventPublisher;
    private CreateAccountCompleteCommandHandler _handler;
    private Mock<IEncodingService> _encodingService;

    private const long ExpectedAccountId = 12343322;
    private const string ExpectedHashString = "123ADF23";
    private const string ExpectedPublicHashString = "SCUFF";
    private const string ExpectedOrganisationNameString = "TheBestCompanyInTheWorld";

    private User _user;

    [SetUp]
    public void Arrange()
    {
        _eventPublisher = new TestableEventPublisher();
        _mediator = new Mock<IMediator>();

        _user = new User { Id = ExpectedAccountId, FirstName = "Danger", LastName = "Mouse", Ref = Guid.NewGuid() };

        _mediator.Setup(x => x.Send(It.IsAny<GetUserByRefQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetUserByRefResponse { User = _user });

        _validator = new Mock<IValidator<CreateAccountCompleteCommand>>();
        _validator.Setup(x => x.Validate(It.IsAny<CreateAccountCompleteCommand>()))
            .Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });

        _encodingService = new Mock<IEncodingService>();
        _encodingService.Setup(x => x.Encode(ExpectedAccountId, EncodingType.AccountId)).Returns(ExpectedHashString);
        _encodingService.Setup(x => x.Encode(ExpectedAccountId, EncodingType.PublicAccountId))
            .Returns(ExpectedPublicHashString);
        
        _handler = new CreateAccountCompleteCommandHandler(
            _eventPublisher,
            _mediator.Object,
            _encodingService.Object,
            _validator.Object,
            Mock.Of<ILogger<CreateAccountCompleteCommandHandler>>());
    }

    [Test]
    public async Task ThenACreatedAccountEventIsPublished()
    {
        //Arrange
        var createAccountCompletedCommand = new CreateAccountCompleteCommand{
            OrganisationName = ExpectedOrganisationNameString,
            HashedAccountId = ExpectedHashString,
            ExternalUserId = _user.Ref.ToString()
        };

        //Act
        await _handler.Handle(createAccountCompletedCommand, CancellationToken.None);

        //Assert
        var createdAccountEvent = _eventPublisher.Events.OfType<CreatedAccountEvent>().Single();

        createdAccountEvent.AccountId.Should().Be(ExpectedAccountId);
        createdAccountEvent.HashedId.Should().Be(ExpectedHashString);
        createdAccountEvent.PublicHashedId.Should().Be(ExpectedPublicHashString);
        createdAccountEvent.Name.Should().Be(ExpectedOrganisationNameString);
        createdAccountEvent.UserName.Should().Be(_user.FullName);
        createdAccountEvent.UserRef.Should().Be(_user.Ref);
    }
}