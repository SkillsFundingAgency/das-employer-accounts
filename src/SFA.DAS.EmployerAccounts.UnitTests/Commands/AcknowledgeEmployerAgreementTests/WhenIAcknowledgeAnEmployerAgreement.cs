using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.AcknowledgeEmployerAgreement;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.SignEmployerAgreementTests
{
    [TestFixture]
    public class WhenIAcknowledgeAnEmployerAgreement
    {
        [Test, MoqAutoData]
        public async Task Then_Agreement_Should_Be_Acknowledged(
            AcknowledgeEmployerAgreementCommand command,
            [Frozen] Mock<IEmployerAgreementRepository> repositoryMock,
            [NoAutoProperties] AcknowledgeEmployerAgreementCommandHandler handler)
        {
            // Arrange
            
            // Act
            _ = await handler.Handle(command, default);

            // Assert
            repositoryMock.Verify(m => m.AcknowledgeEmployerAgreement(command.AgreementId));
        }
        
    }
}