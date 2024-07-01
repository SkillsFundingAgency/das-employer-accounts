using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.RecordUserLoggedIn;
using SFA.DAS.EmployerAccounts.Data.Contracts;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.RecordUserLoggedIn;

public class WhenWeRecordUserHasLoggedIn
{
    private Mock<IUserAccountRepository> _userAccountRepository;        
    private RecordUserLoggedInCommandHandler _handler;
    private RecordUserLoggedInCommand _command;

    [SetUp]
    public void Arrange()
    {

        _userAccountRepository = new Mock<IUserAccountRepository>();

        _handler = new RecordUserLoggedInCommandHandler(_userAccountRepository.Object, Mock.Of<ILogger<RecordUserLoggedInCommandHandler>>());

        _command = new RecordUserLoggedInCommand
        {
            UserRef = "XXXXX"
        };
    }

    [Test]
    public async Task AndIdIsAValidGuidThenItShouldCallRepositoryToRecordTheUserLoggingIn()
    {
        var expectedGuid = Guid.NewGuid();
        _command.UserRef = expectedGuid.ToString();

        await _handler.Handle(_command, CancellationToken.None);

        _userAccountRepository.Verify(x=>x.RecordLogin(expectedGuid), Times.Once);
    }

    [Test]
    public async Task AndIdIsANotaGuidThenItShouldNotCallRepository()
    {
        await _handler.Handle(_command, CancellationToken.None);
        _userAccountRepository.Verify(x => x.RecordLogin(It.IsAny<Guid>()), Times.Never);
    }
}