using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Queries.GetAccountTeamMembers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetAccountTeamMembersQuery;

public class WhenIGetAccountTeamMembers : QueryBaseTest<GetAccountTeamMembersHandler, EmployerAccounts.Queries.GetAccountTeamMembers.GetAccountTeamMembersQuery, GetAccountTeamMembersResponse>
{
    private Mock<IEmployerAccountTeamRepository> _employerAccountTeamRepository;
    private Mock<IEncodingService> _encodingService;
    public override EmployerAccounts.Queries.GetAccountTeamMembers.GetAccountTeamMembersQuery Query { get; set; }
    public override GetAccountTeamMembersHandler RequestHandler { get; set; }
    public override Mock<IValidator<EmployerAccounts.Queries.GetAccountTeamMembers.GetAccountTeamMembersQuery>> RequestValidator { get; set; }

    private const long AccountId = 1234;
    private const string ExpectedHashedAccountId = "MNBGBD";
    private const string ExpectedExternalUserId = "ABCGBD";
    private List<TeamMember> TeamMembers = new();

    [SetUp]
    public void Arrange()
    {
        SetUp();

        TeamMembers.Add(new TeamMember());
            
        _employerAccountTeamRepository = new Mock<IEmployerAccountTeamRepository>();
        _employerAccountTeamRepository
            .Setup(m => m.GetAccountTeamMembersForUserId(ExpectedHashedAccountId, ExpectedExternalUserId))
            .ReturnsAsync(TeamMembers);

        _encodingService = new Mock<IEncodingService>();
        
        RequestHandler = new GetAccountTeamMembersHandler(
            RequestValidator.Object, 
            _employerAccountTeamRepository.Object,
            _encodingService.Object);

        Query = new EmployerAccounts.Queries.GetAccountTeamMembers.GetAccountTeamMembersQuery();
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
    {
        //Act
        await RequestHandler.Handle(new EmployerAccounts.Queries.GetAccountTeamMembers.GetAccountTeamMembersQuery
        {
            HashedAccountId = ExpectedHashedAccountId,
            ExternalUserId = ExpectedExternalUserId
        }, CancellationToken.None);

        //Assert
        _employerAccountTeamRepository.Verify(x => x.GetAccountTeamMembersForUserId(ExpectedHashedAccountId, ExpectedExternalUserId), Times.Once);
    }
        
    [Test]
    public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
    {
        //Act
        var result = await RequestHandler.Handle(new EmployerAccounts.Queries.GetAccountTeamMembers.GetAccountTeamMembersQuery
        {
            HashedAccountId = ExpectedHashedAccountId,
            ExternalUserId = ExpectedExternalUserId
        }, CancellationToken.None);

        //Assert
        result.Should().NotBeNull();
        result.TeamMembers.Should().NotBeNull();
    }
        
    [Test]
    public async Task ThenIfTheMessageIsValidTheUserIdIsEncoded()
    {
        //Act
        var result = await RequestHandler.Handle(new EmployerAccounts.Queries.GetAccountTeamMembers.GetAccountTeamMembersQuery
        {
            HashedAccountId = ExpectedHashedAccountId,
            ExternalUserId = ExpectedExternalUserId
        }, CancellationToken.None);

        //Assert
        _encodingService.Verify(x=> x.Encode(It.IsAny<long>(), EncodingType.AccountId), Times.Exactly(result.TeamMembers.Count));
    }
}