﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Caches;
using SFA.DAS.EmployerAccounts.Models.ReferenceData;
using SFA.DAS.EmployerAccounts.Services;
using SFA.DAS.ReferenceData.Api.Client;
using SFA.DAS.ReferenceData.Types.DTO;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.UnitTests.Services.ReferenceData
{
    [TestFixture]
    public class WhenISearchOrganisations
    {
        [Test, MoqAutoData]
        public async Task WhenOrgExistsInCache_ThenReturnCacheResponse(
            string searchTerm,
            OrganisationName cacheResponse,
            [Frozen] Mock<IInProcessCache> cacheMock,
            [Greedy] ReferenceDataService referenceDataService)
        {
            // Arrange
            var base64SearchTerm = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(searchTerm));
            cacheMock.Setup(m => m.Get<List<OrganisationName>>(It.Is<string>(s => s.Contains(base64SearchTerm)))).Returns(new List<OrganisationName> { cacheResponse });

            // Act
            var result = await referenceDataService.SearchOrganisations(searchTerm);

            // Assert
            result.Data.First().Should().Be(cacheResponse);
        }

        [Test, MoqAutoData]
        public async Task WhenOrgExistsInCache_DoNotCallApi(
            string searchTerm,
            OrganisationName cacheResponse,
            [Frozen] Mock<IInProcessCache> cacheMock,
            [Frozen] Mock<IReferenceDataApiClient> refDataApiClientMock,
            [Greedy] ReferenceDataService referenceDataService)
        {
            // Arrange
            var base64SearchTerm = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(searchTerm));
            cacheMock.Setup(m => m.Get<List<OrganisationName>>(It.Is<string>(s => s.Contains(base64SearchTerm)))).Returns(new List<OrganisationName> { cacheResponse });

            // Act
            var result = await referenceDataService.SearchOrganisations(searchTerm);

            // Assert
            refDataApiClientMock.Verify(m => m.SearchOrganisations(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task WhenSearchTermNotPresentInCache_ThenCallReferenceDataApi(
            string searchTerm,
            [Frozen] Mock<IReferenceDataApiClient> refDataApiClientMock,
            [Greedy] ReferenceDataService referenceDataService)
        {
            // Arrange

            // Act
            var result = await referenceDataService.SearchOrganisations(searchTerm);

            // Assert
            refDataApiClientMock.Verify(m => m.SearchOrganisations(searchTerm, It.IsAny<int>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task WhenNoResultsForSearchTerm_ThenReturnEmptyResultSet(
            string searchTerm,
            [Frozen] Mock<IReferenceDataApiClient> refDataApiClientMock,
            [Greedy] ReferenceDataService referenceDataService)
        {
            // Arrange
            refDataApiClientMock.Setup(m => m.SearchOrganisations(searchTerm, It.IsAny<int>())).ReturnsAsync(new List<Organisation>());

            // Act
            var result = await referenceDataService.SearchOrganisations(searchTerm);

            // Assert
            result.Data.Count.Should().Be(0);
        }

        [Test, MoqAutoData]
        public async Task WhenNullResultsForSearchTerm_ThenReturnEmptyResultSet(
            string searchTerm,
            [Frozen] Mock<IReferenceDataApiClient> refDataApiClientMock,
            [Greedy] ReferenceDataService referenceDataService)
        {
            // Arrange
            refDataApiClientMock.Setup(m => m.SearchOrganisations(searchTerm, It.IsAny<int>())).ReturnsAsync((List<Organisation>)null);

            // Act
            var result = await referenceDataService.SearchOrganisations(searchTerm);

            // Assert
            result.Data.Count.Should().Be(0);
        }

        [Test]
        [MoqInlineAutoData(OrganisationStatus.None)]
        [MoqInlineAutoData(OrganisationStatus.Active)]
        public async Task WhenValidStatusResultsExistForSearchTerm_ThenShouldReturnResult(
           OrganisationStatus organisationStatus,
            string searchTerm,
           Organisation organisation,
           [Frozen] Mock<IReferenceDataApiClient> refDataApiClientMock,
           [Greedy] ReferenceDataService referenceDataService)
        {
            // Arrange
            organisation.OrganisationStatus = organisationStatus;
            refDataApiClientMock.Setup(m => m.SearchOrganisations(searchTerm, It.IsAny<int>())).ReturnsAsync(new List<Organisation> { organisation });

            // Act
            var result = await referenceDataService.SearchOrganisations(searchTerm);

            // Assert
            result.Data.Count.Should().Be(1);
        }

        [Test, MoqAutoData]
        public async Task WhenResultsExistForSearchTerm_ThenShouldMapResultAddress(
           string searchTerm,
           Organisation organisation,
           [Frozen] Mock<IReferenceDataApiClient> refDataApiClientMock,
           [Greedy] ReferenceDataService referenceDataService)
        {
            // Arrange
            refDataApiClientMock.Setup(m => m.SearchOrganisations(searchTerm, It.IsAny<int>())).ReturnsAsync(new List<Organisation> { organisation });

            // Act
            var result = await referenceDataService.SearchOrganisations(searchTerm);

            // Assert
            result.Data.First().Address.ShouldBeEquivalentTo(organisation.Address);
        }

        [Test]
        [MoqInlineAutoData(OrganisationStatus.Dissolved)]
        [MoqInlineAutoData(OrganisationStatus.Liquidation)]
        [MoqInlineAutoData(OrganisationStatus.Receivership)]
        [MoqInlineAutoData(OrganisationStatus.Administration)]
        [MoqInlineAutoData(OrganisationStatus.VoluntaryArrangement)]
        [MoqInlineAutoData(OrganisationStatus.ConvertedClosed)]
        [MoqInlineAutoData(OrganisationStatus.InsolvencyProceedings)]
        public async Task WhenResultsExistForSearchTerm_ThenShouldFilterNoneActiveOrgs(
           OrganisationStatus organisationStatus,
           string searchTerm,
           Organisation organisation,
           [Frozen] Mock<IReferenceDataApiClient> refDataApiClientMock,
           [Greedy] ReferenceDataService referenceDataService)
        {
            // Arrange
            organisation.OrganisationStatus = organisationStatus;
            refDataApiClientMock.Setup(m => m.SearchOrganisations(searchTerm, It.IsAny<int>())).ReturnsAsync(new List<Organisation> { organisation });

            // Act
            var result = await referenceDataService.SearchOrganisations(searchTerm);

            // Assert
            result.Data.Count.Should().Be(0);
        }
    }
}
