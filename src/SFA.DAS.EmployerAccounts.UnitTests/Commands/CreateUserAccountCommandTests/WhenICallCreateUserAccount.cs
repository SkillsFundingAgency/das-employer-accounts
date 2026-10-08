using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.CreateUserAccount;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.CreateUserAccountCommandTests;

public class WhenICallCreateUserAccount
{
    private Mock<IAccountRepository> _accountRepository;
    private Mock<IMediator> _mediator;
    private Mock<IValidator<CreateUserAccountCommand>> _validator;
    private Mock<IEncodingService> _encodingService;
    private CreateUserAccountCommandHandler _handler;
    private User _user;

    [SetUp]
    public void Arrange()
    {
        _user = new User { Id = 33, FirstName = "Bob", LastName = "Green", Ref = Guid.NewGuid() };

        _accountRepository = new Mock<IAccountRepository>();
        _accountRepository.Setup(x => x.CreateUserAccount(It.IsAny<long>(), It.IsAny<string>()))
            .ReturnsAsync(new CreateUserAccountResult { AccountId = 99 });

        _mediator = new Mock<IMediator>();
        _mediator.Setup(x => x.Send(It.IsAny<GetUserByRefQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetUserByRefResponse { User = _user });

        _validator = new Mock<IValidator<CreateUserAccountCommand>>();
        _validator.Setup(x => x.Validate(It.IsAny<CreateUserAccountCommand>()))
            .Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });

        _encodingService = new Mock<IEncodingService>();
        _encodingService.Setup(x => x.Encode(99, EncodingType.AccountId)).Returns("HASHED");
        _encodingService.Setup(x => x.Encode(99, EncodingType.PublicAccountId)).Returns("PUBLIC");

        _handler = new CreateUserAccountCommandHandler(
            _accountRepository.Object,
            _mediator.Object,
            _validator.Object,
            _encodingService.Object);
    }

    [Test]
    public async Task ThenOrganisationNameIsTrimmedBeforeCreate()
    {
        var command = new CreateUserAccountCommand
        {
            ExternalUserId = _user.Ref.ToString(),
            OrganisationName = "  ALDRIDGE EDUCATION "
        };

        await _handler.Handle(command, CancellationToken.None);

        _accountRepository.Verify(x => x.CreateUserAccount(_user.Id, "ALDRIDGE EDUCATION"), Times.Once);
        command.OrganisationName.Should().Be("ALDRIDGE EDUCATION");
    }
}
