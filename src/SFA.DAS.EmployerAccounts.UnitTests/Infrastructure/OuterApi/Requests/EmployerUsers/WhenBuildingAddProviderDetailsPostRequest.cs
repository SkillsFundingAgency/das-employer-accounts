using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.UserAccounts;

namespace SFA.DAS.EmployerAccounts.UnitTests.Infrastructure.OuterApi.Requests.EmployerUsers
{
    public class WhenBuildingAddProviderDetailsPostRequest
    {
        [Test, AutoData]
        public void Then_The_Url_Is_Correctly_Built(string userId, string correlationId, long accountId,
            string email, string firstName, string lastName)
        {
            var actual = new AddProviderDetailsPostRequest(userId, correlationId,
                accountId, email, firstName, lastName);

            actual.PostUrl.Should().Be($"accountusers/{userId}/add-provider-details-from-invitation");
        }

        [Test, AutoData]
        public void Constructor_Should_Set_Properties_Correctly(string userId, string correlationId, long accountId,
            string email, string firstName, string lastName)
        {
            // Arrange
            var expectedData = new
            {
                AccountId = accountId,
                CorrelationId = correlationId,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };

            // Act
            var addProviderDetailsPostRequest = new AddProviderDetailsPostRequest(userId, correlationId, accountId, email, firstName, lastName);

            // Assert
            addProviderDetailsPostRequest.UserId.Should().Be(userId);
            addProviderDetailsPostRequest.Data.Should().NotBeNull();
            addProviderDetailsPostRequest.Data.Should().BeEquivalentTo(expectedData);
            addProviderDetailsPostRequest.PostUrl.Should().Be($"accountusers/{userId}/add-provider-details-from-invitation");
        }
    }
}
