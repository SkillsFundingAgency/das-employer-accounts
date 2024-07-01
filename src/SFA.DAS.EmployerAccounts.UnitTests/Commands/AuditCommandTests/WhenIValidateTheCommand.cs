using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.AuditCommandTests
{
    public class WhenIValidateTheCommand
    {
        private CreateAuditCommandValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new CreateAuditCommandValidator();
        }

        [Test]
        public void ThenTrueIsReturnedWhenAllFieldsArePopulated()
        {
            //Act
            var actual = _validator.Validate(new CreateAuditCommand
            {
                EasAuditMessage = new AuditMessage
                {
                    Description = "descriptiosn",
                    RelatedEntities = new List<AuditEntity> { new AuditEntity() },
                    ChangedProperties = new List<PropertyUpdate> { new PropertyUpdate() },
                    AffectedEntity = new AuditEntity { Id = "1", Type = "test" }
                }
            });

            //Assert
            Assert.That(actual.IsValid(), Is.True);
        }

        [Test]
        public void ThenFalseIsReturnedAndTheDictionaryIsPopulatedWhenTheCommandIsNotValid()
        {
            //Act
            var actual = _validator.Validate(new CreateAuditCommand { EasAuditMessage = new AuditMessage() });

            //Assert
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("ChangedProperties", "ChangedProperties has not been supplied")));
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("Description", "Description has not been supplied")));
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("AffectedEntity", "AffectedEntity has not been supplied")));
        }

        [Test]
        public void ThenFalseIsReturnedAndTheDictionaryIsPopulatedWhenTheListsAreEmpty()
        {
            //Act
            var actual = _validator.Validate(new CreateAuditCommand { EasAuditMessage = new AuditMessage { Description = "test", ChangedProperties = new List<PropertyUpdate>() } });

            //Assert
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("ChangedProperties", "ChangedProperties has not been supplied")));
        }

        [Test]
        public void ThenFalseIsReturnedAndTheDictionaryIsPopulatedWhenTheCommandEasAuditMessageIsNull()
        {
            //Act
            var actual = _validator.Validate(new CreateAuditCommand());

            //Assert
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("EasAuditMessage", "EasAuditMessage has not been supplied")));

        }

        [Test]
        public void ThenTrueIsReturnedIfCategoryIsViewAndChangedPropertiesAreMissing()
        {
            // arrange
            var command = new CreateAuditCommand
            {
                EasAuditMessage = new AuditMessage
                {
                    Category = "View",
                    Description = "description",
                    RelatedEntities = new List<AuditEntity> { new AuditEntity() },
                    ChangedProperties = null,
                    AffectedEntity = new AuditEntity { Id = "1", Type = "test" }
                }
            };

            //Act
            var result = _validator.Validate(command);

            //Assert
            Assert.That(result.IsValid(), Is.True);
        }

        [Test]
        public void ThenFalseIsReturnedIfCategoryIsNotViewAndChangedPropertiesAreMissing()
        {
            // arrange
            var command = new CreateAuditCommand
            {
                EasAuditMessage = new AuditMessage
                {
                    Category = "Changed",
                    Description = "description",
                    RelatedEntities = new List<AuditEntity> { new AuditEntity() },
                    ChangedProperties = null,
                    AffectedEntity = new AuditEntity { Id = "1", Type = "test" }
                }
            };

            //Act
            var result = _validator.Validate(command);

            //Assert
            Assert.That(result.IsValid(), Is.False);
            Assert.That(result.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("ChangedProperties", "ChangedProperties has not been supplied")));
        }

    }
}
