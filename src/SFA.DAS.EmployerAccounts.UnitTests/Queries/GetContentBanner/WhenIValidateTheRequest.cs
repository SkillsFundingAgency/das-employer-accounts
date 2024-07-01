using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetContent;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetContentBanner
{
    public class WhenIValidateTheRequest
    {
        private GetContentRequestValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetContentRequestValidator();
        }

        [Test]
        public void ThenShouldReturnValidIfRequestIsValid()
        {
            //Act
            var result = _validator.Validate(new GetContentRequest()
            {
                ContentType = "banner"
            });

            //Assert
            Assert.That(result.IsValid(), Is.True);
        }

        [Test]
        public void ThenShouldReturnInvalidIfInfoIsProvided()
        {
            //Act
            var result = _validator.Validate(new GetContentRequest());

            //Assert
            Assert.That(result.IsValid(), Is.False);
        }

        [Test]
        public void ThenShouldReturnInvalidIfContentTypeIdIsEmpty()
        {
            //Act
            var result = _validator.Validate(new GetContentRequest { ContentType = string.Empty });

            //Assert
            Assert.That(result.IsValid(), Is.False);
        }
    }
}
