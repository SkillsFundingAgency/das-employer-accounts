using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Caches;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Models.ApprenticeshipCourse;
using SFA.DAS.EmployerFinance.Services;


namespace SFA.DAS.EmployerFinance.UnitTests.Services.ApprenticeshipInfoServiceWrapperTests
{
    public class WhenGettingApprenticeshipInfo
    {
        private Mock<IApiClient> _apiClient;
        private Mock<IInProcessCache> _cache;
        private ApprenticeshipInfoServiceWrapper _service;

        [SetUp]
        public void Arrange()
        {
            _apiClient = new Mock<IApiClient>();
            _cache = new Mock<IInProcessCache>();
            
            _service = new ApprenticeshipInfoServiceWrapper(_cache.Object, _apiClient.Object);
        }
        
        [Test]
        public async Task Then_The_Standards_Are_Retrieved_From_The_Api_And_Added_To_Cached()
        {
            //Arrange
            StandardsView returnData = null;
            var fixture = new Fixture();
            var apiResponse = fixture.Create<GetStandardsResponse>();
            _apiClient.Setup(x => x.Get<GetStandardsResponse>(It.IsAny<GetStandardsRequest>()))
                .ReturnsAsync(apiResponse);
            _cache.Setup(x => x.Exists("Standards")).Returns(false);
            _cache.Setup(x => x.Set("Standards", It.IsAny<StandardsView>())).Callback<string,object>(
                (key, value) =>
                {
                    returnData = value as StandardsView;
                });
            
            //Act
            await _service.GetStandardsAsync();
            
            //Assert
            _apiClient.Verify(x=>x.Get<GetStandardsResponse>(It.IsAny<GetStandardsRequest>()),Times.Once);
            returnData.Standards.Count.Should().Be(apiResponse.Standards.Count);
            returnData.Standards.ToList().First().ShouldBeEquivalentTo(apiResponse.Standards.First(), options=> options
                .Excluding(c=>c.Title)
                .Excluding(c=>c.Code)
                .Excluding(c=>c.CourseName)
            );
        }

        [Test]
        public async Task Then_Standards_Retrieved_From_Cache_If_Cached()
        {
            //Arrange
            var fixture = new Fixture();
            var cacheData = fixture.Create<StandardsView>();
            _cache.Setup(x => x.Exists("Standards")).Returns(true);
            _cache.Setup(x => x.Get<StandardsView>("Standards")).Returns(cacheData);
            
            //Act
            var actual = await _service.GetStandardsAsync();
            
            //Assert
            _apiClient.Verify(x=>x.Get<GetStandardsResponse>(It.IsAny<GetStandardsRequest>()),Times.Never);
            actual.ShouldBeEquivalentTo(cacheData);
        }
        
        [Test]
        public async Task Then_The_Frameworks_Are_Retrieved_From_The_Api_And_Added_To_Cached()
        {
            //Arrange
            var fixture = new Fixture();
            var apiResponse = fixture.Create<GetFrameworksResponse>();
                
            FrameworksView returnData = null;
            _apiClient.Setup(x => x.Get<GetFrameworksResponse>(It.IsAny<GetFrameworksRequest>()))
                .ReturnsAsync(apiResponse);
            _cache.Setup(x => x.Exists("Frameworks")).Returns(false);
            _cache.Setup(x => x.Set("Frameworks", It.IsAny<FrameworksView>())).Callback<string,object>(
                (key, value) =>
                {
                    returnData = value as FrameworksView;
                });

            //Act
            await _service.GetFrameworksAsync();
            
            //Assert
            _apiClient.Verify(x=>x.Get<GetFrameworksResponse>(It.IsAny<GetFrameworksRequest>()),Times.Once);
            returnData.Frameworks.Count.Should().Be(apiResponse.Frameworks.Count);
            returnData.Frameworks.ToList().First().ShouldBeEquivalentTo(apiResponse.Frameworks.First(), options=> options
                .Excluding(c=>c.Title)
                .Excluding(c=>c.ProgrammeType)
            );
        }
        [Test]
        public async Task Then_Frameworks_Retrieved_From_Cache_If_Cached()
        {
            //Arrange
            var fixture = new Fixture();
            var cacheData = fixture.Create<FrameworksView>();
            _cache.Setup(x => x.Exists("Frameworks")).Returns(true);
            _cache.Setup(x => x.Get<FrameworksView>("Frameworks")).Returns(cacheData);
            
            //Act
            var actual = await _service.GetFrameworksAsync();
            
            //Assert
            _apiClient.Verify(x=>x.Get<GetFrameworksResponse>(It.IsAny<GetFrameworksRequest>()),Times.Never);
            actual.ShouldBeEquivalentTo(cacheData);
        }

    }
}