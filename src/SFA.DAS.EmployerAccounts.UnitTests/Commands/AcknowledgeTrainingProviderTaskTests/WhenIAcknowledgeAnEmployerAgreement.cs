using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.AcknowledgeTrainingProviderTask;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.AcknowledgeTrainingProviderTaskTests
{
    [TestFixture]
    public class WhenIAcknowledgeATrainingProvider
    {
        [Test, MoqAutoData]
        public async Task Then_Agreement_Should_Be_Acknowledged(
            AcknowledgeTrainingProviderTaskCommand command,
            [Frozen] Mock<IEmployerAccountRepository> repositoryMock,
            [NoAutoProperties] AcknowledgeTrainingProviderTaskCommandHandler handler)
        {
            // Arrange
            
            // Act
            await handler.Handle(command, default);

            // Assert
            repositoryMock.Verify(m => m.AcknowledgeTrainingProviderTask(command.AccountId));
        }
        
    }
}