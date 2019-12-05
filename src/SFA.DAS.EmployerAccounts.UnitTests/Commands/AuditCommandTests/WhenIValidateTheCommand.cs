﻿using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Models;
using Entity = SFA.DAS.Audit.Types.Entity;

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
            var actual = _validator.Validate(new CreateAuditCommand {EasAuditMessage = new EasAuditMessage
            {
                Description = "descriptiosn",
                RelatedEntities = new List<Entity> { new Entity () },
                ChangedProperties = new List<PropertyUpdate> { new PropertyUpdate()},
                AffectedEntity = new Entity { Id="1",Type="test"}
            } });

            //Assert
            Assert.IsTrue(actual.IsValid());
        }

        [Test]
        public void ThenFalseIsReturnedAndTheDictionaryIsPopulatedWhenTheCommandIsNotValid()
        {
            //Act
            var actual = _validator.Validate(new CreateAuditCommand { EasAuditMessage = new EasAuditMessage() });

            //Assert
            Assert.IsFalse(actual.IsValid());
            Assert.Contains(new KeyValuePair<string,string>("ChangedProperties", "ChangedProperties has not been supplied"), actual.ValidationDictionary);
            Assert.Contains(new KeyValuePair<string,string>("Description", "Description has not been supplied"), actual.ValidationDictionary);
            Assert.Contains(new KeyValuePair<string,string>("AffectedEntity", "AffectedEntity has not been supplied"), actual.ValidationDictionary);
        }

        [Test]
        public void ThenFalseIsReturnedAndTheDictionaryIsPopulatedWhenTheListsAreEmpty()
        {
            //Act
            var actual = _validator.Validate(new CreateAuditCommand { EasAuditMessage = new EasAuditMessage {Description = "test", ChangedProperties = new List<PropertyUpdate>()} });

            //Assert
            Assert.IsFalse(actual.IsValid());
            Assert.Contains(new KeyValuePair<string, string>("ChangedProperties", "ChangedProperties has not been supplied"), actual.ValidationDictionary);            
        }

        [Test]
        public void ThenFalseIsReturnedAndTheDictionaryIsPopulatedWhenTheCommandEasAuditMessageIsNull()
        {
            //Act
            var actual = _validator.Validate(new CreateAuditCommand ());

            //Assert
            Assert.IsFalse(actual.IsValid());
            Assert.Contains(new KeyValuePair<string, string>("EasAuditMessage", "EasAuditMessage has not been supplied"), actual.ValidationDictionary);
            
        }

        [Test]
        public void ThenTrueIsReturnedIfCategoryIsViewAndChangedPropertiesAreMissing()
        {
            // arrange
            var command = new CreateAuditCommand
            {
                EasAuditMessage = new EasAuditMessage
                {
                    Category = "View",
                    Description = "description",
                    RelatedEntities = new List<Entity> { new Entity() },
                    ChangedProperties = null,
                    AffectedEntity = new Entity { Id = "1", Type = "test" }
                }
            };

            //Act
            var result = _validator.Validate(command);

            //Assert
            Assert.IsTrue(result.IsValid());
        }

        [Test]
        public void ThenFalseIsReturnedIfCategoryIsNotViewAndChangedPropertiesAreMissing()
        {
            // arrange
            var command = new CreateAuditCommand
            {
                EasAuditMessage = new EasAuditMessage
                {
                    Category = "Changed",
                    Description = "description",
                    RelatedEntities = new List<Entity> { new Entity() },
                    ChangedProperties = null,
                    AffectedEntity = new Entity { Id = "1", Type = "test" }
                }
            };

            //Act
            var result = _validator.Validate(command);

            //Assert
            Assert.IsFalse(result.IsValid());
            Assert.Contains(new KeyValuePair<string, string>("ChangedProperties", "ChangedProperties has not been supplied"), result.ValidationDictionary);
        }

    }
}
