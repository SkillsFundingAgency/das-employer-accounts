using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetOrganisationAgreements;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetOrganisationAgreements
{
    public class WhenIValidateTheRequest
    {
        private GetOrganisationAgreementsValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetOrganisationAgreementsValidator();
        }

        [Test]
        public async Task ThenShouldReturnValidIfRequestIsValid()
        {
            //Act
            var result = await _validator.ValidateAsync(new GetOrganisationAgreementsRequest { AccountLegalEntityHashedId = "Abc123" });

            //Assert
            Assert.That(result.IsValid(), Is.True);
        }

        [Test]
        public async Task ThenShouldReturnInvaldidIfAccountLegalEntityHashedIdIsEmpty()
        {
            //Act
            var result = await _validator.ValidateAsync(new GetOrganisationAgreementsRequest { AccountLegalEntityHashedId = string.Empty });

            //Assert
            Assert.That(result.IsValid(), Is.False);
        }


        [Test]
        public async Task ThenShouldReturnInvaldidIfAccountLegalEntityHashedIdIsNull()
        {
            //Act
            var result = await _validator.ValidateAsync(new GetOrganisationAgreementsRequest { AccountLegalEntityHashedId = null });

            //Assert
            Assert.That(result.IsValid(), Is.False);
        }
    }
}
