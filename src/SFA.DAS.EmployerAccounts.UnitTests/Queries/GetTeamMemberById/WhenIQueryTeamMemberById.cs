using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Queries.GetMemberById;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetTeamMemberById;

public class WhenIQueryTeamMemberById
{
    [Test, MoqAutoData]
    public async Task Then_The_Team_Member_Is_Returned(
        Mock<IEmployerAccountTeamRepository> repository,
        TeamMember teamMember,
        string hashedAccountId,
        string hashedUserId,
        long accountId,
        long userId,
        Mock<IEncodingService> encodingService)
    {
        var sut = new GetMemberByIdQueryHandler(repository.Object, encodingService.Object);

        var query = new GetMemberByIdQuery
        {
            IsUser = false,
            AccountId = accountId,
            Id = userId
        };

        encodingService.Setup(x => x.Encode(query.AccountId, EncodingType.AccountId)).Returns(hashedAccountId);
        encodingService.Setup(x => x.Encode(query.Id, EncodingType.AccountId)).Returns(hashedUserId);
        repository.Setup(x => x.GetMember(hashedAccountId, query.Id, query.IsUser)).ReturnsAsync(teamMember);

        var result = await sut.Handle(query, CancellationToken.None);

        result.TeamMember.Should().Be(teamMember);
       
        encodingService.Verify(x=> x.Encode(query.AccountId, EncodingType.AccountId), Times.Once);
        encodingService.Verify(x=> x.Encode(teamMember.Id, EncodingType.AccountId), Times.Once);
        
        repository.Verify(x=> x.GetMember(hashedAccountId, query.Id, query.IsUser), Times.Once);
    }
}