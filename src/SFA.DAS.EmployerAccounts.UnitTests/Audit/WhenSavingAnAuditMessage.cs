using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Audit;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Data.Contracts;

namespace SFA.DAS.EmployerAccounts.UnitTests.Audit;

public class WhenSavingAnAuditMessage
{
    [Test]
    [AutoData]
    public async Task then_store_should_be_called_on_the_repository(
        Mock<IAuditRepository> repository,
        AuditMessage message)
    {
        // arrange
        var sut = new AuditClient(repository.Object);
        
        // act
        await sut.Audit(message);
        
        // assert
        repository.Verify(x => x.Store(It.Is<AuditMessage>(m => m == message)), Times.Once);
    }
}