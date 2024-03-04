using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.AddProviderDetailsFromInvitation;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.UserAccounts;
using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.AddProviderDetailsFromInvitationTests
{
    [TestFixture]
    public class WhenICallHandle
    {
        [Test, MoqAutoData]
        public async Task Then_Send_AddProviderDetailsPostRequest(
            AddProviderDetailsFromInvitationCommand command,
            [Frozen] Mock<IOuterApiClient> outerApi,
            [NoAutoProperties] AddProviderDetailsFromInvitationCommandHandler handler)
        {
            //Arrange
            var expectedRequest = new AddProviderDetailsPostRequest(command.UserId, command.CorrelationId,
            command.AccountId, command.Email, command.FirstName, command.LastName);

            var expectedData = new
            {
                command.AccountId,
                command.CorrelationId,
                command.Email,
                command.FirstName,
                command.LastName
            };

            // Act
            await handler.Handle(command, default);

            // Assert
            outerApi.Verify(
             x => x.Post(It.Is<AddProviderDetailsPostRequest>(
                 actualRequest =>
                     actualRequest.UserId == expectedRequest.UserId &&
                     actualRequest.PostUrl == expectedRequest.PostUrl &&
                     actualRequest.Data != null &&
                     actualRequest.Data.GetType().GetProperty("AccountId").GetValue(actualRequest.Data).As<long>() == expectedData.AccountId &&
                     actualRequest.Data.GetType().GetProperty("CorrelationId").GetValue(actualRequest.Data).ToString() == expectedData.CorrelationId &&
                     actualRequest.Data.GetType().GetProperty("Email").GetValue(actualRequest.Data).ToString() == expectedData.Email &&
                     actualRequest.Data.GetType().GetProperty("FirstName").GetValue(actualRequest.Data).ToString() == expectedData.FirstName &&
                     actualRequest.Data.GetType().GetProperty("LastName").GetValue(actualRequest.Data).ToString() == expectedData.LastName
             )), Times.Once);
        }
    }
}
