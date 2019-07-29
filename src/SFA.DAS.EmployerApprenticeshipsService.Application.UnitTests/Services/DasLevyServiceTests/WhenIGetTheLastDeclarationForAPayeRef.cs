﻿using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Application.Queries.AccountTransactions.GetLastLevyDeclaration;
using SFA.DAS.EAS.Application.Services;
using SFA.DAS.EAS.Domain.Data.Repositories;
using SFA.DAS.EAS.Domain.Models.Levy;

namespace SFA.DAS.EAS.Application.UnitTests.Services.DasLevyServiceTests
{
    public class WhenIGetTheLastDeclarationForAPayeRef
    {
        private DasLevyService _dasLevyService;
        private Mock<IMediator> _mediator;
        private Mock<ITransactionRepository> _transactionRepoMock;
      

        [SetUp]
        public void Arrange()
        {
            _transactionRepoMock = new Mock<ITransactionRepository>();
            _mediator = new Mock<IMediator>();
            _mediator.Setup(x => x.SendAsync(It.IsAny<GetLastLevyDeclarationQuery>()))
                     .ReturnsAsync(new GetLastLevyDeclarationResponse()
                     {
                        Transaction = new DasDeclaration()
                        {
                            
                        }
                     });

            _dasLevyService = new DasLevyService(_mediator.Object, _transactionRepoMock.Object);
        }

        [Test]
        public async Task ThenTheMediatorMethodIsCalled()
        {
            //Arrange
            var empRef = "123FGV";

            //Act
            await _dasLevyService.GetLastLevyDeclarationforEmpRef(empRef);

            //Assert
            _mediator.Verify(x => x.SendAsync(It.Is<GetLastLevyDeclarationQuery>(c => c.EmpRef.Equals(empRef))), Times.Once);
        }

        [Test]
        public async Task ThenTheResponseFromTheQueryIsReturned()
        {
            //Arrange
            var empRef = "123FGV";

            //Act
            var actual = await _dasLevyService.GetLastLevyDeclarationforEmpRef(empRef);

            //Assert
            Assert.IsNotNull(actual);
            Assert.IsAssignableFrom<DasDeclaration>(actual);
        }
    }
}
