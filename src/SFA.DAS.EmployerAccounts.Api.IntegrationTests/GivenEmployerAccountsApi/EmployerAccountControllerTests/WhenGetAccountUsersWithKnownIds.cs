using System.Diagnostics.CodeAnalysis;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.IntegrationTests.Helpers;
using SFA.DAS.EmployerAccounts.Api.IntegrationTests.ModelBuilders;
using SFA.DAS.EmployerAccounts.Api.Types;

namespace SFA.DAS.EmployerAccounts.Api.IntegrationTests.GivenEmployerAccountsApi.EmployerAccountControllerTests;

[ExcludeFromCodeCoverage]
[TestFixture]
public class WhenGetAccountUsersWithKnownIds : GivenEmployerAccountsApi
{
    private Guid _userRef;

    [SetUp]
    public async Task SetUp()
    {
        string? hashedAccountId = null;
        _userRef = Guid.Empty;

        await InitialiseEmployerAccountData(async builder =>
        {
            var data = new TestModelBuilder()
                .WithNewUser()
                .WithNewAccount()
                .WithNewLegalEntity();

            await builder.SetupDataAsync(data);

            hashedAccountId = data.CurrentAccount.AccountOutput?.HashedAccountId;
            _userRef = data.CurrentUser.UserOutput.UserRef;
        });

        WhenControllerActionIsCalled($"/api/accounts/{hashedAccountId}/users");
    }

    [Test]
    public void ThenTheStatusShouldBeFound_AndDataShouldContainOnlyTheExpectedUser()
    {
        var teamMembers = Response?.GetContent<List<TeamMember>>();
        Response?.ExpectStatusCodes(HttpStatusCode.OK);
        
        teamMembers.Should().NotBeNull();
        teamMembers!.Count.Should().Be(1);
        teamMembers[0].UserRef!.Should().Be(_userRef.ToString());
    }
}