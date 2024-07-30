using System.Security.Claims;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Http;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.CreateAccountTaskList;

[TestFixture]
public class WhenUserHasNotSignedLegalAgreement
{
    [Test]
    [MoqAutoData]
    public async Task Then_HashedAccountId_IsDecoded(
        string hashedAccountId,
        string userId,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        [NoAutoProperties] EmployerAccountController controller)
    {
        // Arrange
        SetControllerContextUserIdClaim(userId, controller);

        // Act
        _ = await controller.CreateAccountTaskList(hashedAccountId);

        // Assert
        encodingServiceMock.Verify(m => m.Decode(hashedAccountId, EncodingType.AccountId), Times.Once);
    }

     
    private static void SetControllerContextUserIdClaim(string userId, EmployerAccountController controller)
    {
        var claims = new List<Claim> { new Claim(ControllerConstants.UserRefClaimKeyName, userId) };
        var claimsIdentity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(claimsIdentity);
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
    }
}