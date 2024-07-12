//using FluentAssertions;
//using MediatR;
//using Microsoft.AspNetCore.Mvc.Abstractions;
//using Microsoft.AspNetCore.Mvc.Routing;
//using Microsoft.AspNetCore.Routing;
//using Microsoft.Extensions.Logging;
//using SFA.DAS.EmployerAccounts.Web.RouteValues;
//using SFA.DAS.Testing.AutoFixture;

//namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.AccountName
//{
//    [TestFixture]
//    class WhenISelectToNotChangeAccountName : ControllerTestBase
//    {
//        private EmployerAccountController _employerAccountController;
//        private Mock<IMediator> _mediator;
//        private Mock<EmployerAccountOrchestrator> _orchestrator;
//        private Mock<ICookieStorageService<FlashMessageViewModel>> _flashMessage;
//        private const string ExpectedRedirectUrl = "http://redirect.local.test";

//        [SetUp]
//        public void Arrange()
//        {
//            base.Arrange(ExpectedRedirectUrl);

//            _orchestrator = new Mock<EmployerAccountOrchestrator>();
//            _mediator = new Mock<IMediator>();
//            _flashMessage = new Mock<ICookieStorageService<FlashMessageViewModel>>();

//            _orchestrator.Setup(x =>
//                    x.SetEmployerAccountName(It.IsAny<string>(), It.IsAny<RenameEmployerAccountViewModel>(),
//                        It.IsAny<string>()))
//                .ReturnsAsync(new OrchestratorResponse<RenameEmployerAccountViewModel>
//                {
//                    Status = HttpStatusCode.OK,
//                    Data = new RenameEmployerAccountViewModel()
//                });

//            AddUserToContext();

//            _employerAccountController = new EmployerAccountController(
//                _orchestrator.Object,
//                Mock.Of<ILogger<EmployerAccountController>>(),
//                _flashMessage.Object,
//                _mediator.Object,
//                Mock.Of<ICookieStorageService<ReturnUrlModel>>(),
//                Mock.Of<ICookieStorageService<HashedAccountIdModel>>(),
//                Mock.Of<LinkGenerator>())
//            {
//                ControllerContext = ControllerContext,
//                Url = new UrlHelper(new ActionContext(MockHttpContext.Object, Routes, new ActionDescriptor()))
//            };
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            _employerAccountController?.Dispose();
//        }

//        [Test, MoqAutoData]
//        public async Task Then_Name_Is_Updated(string hashedAccountId, RenameEmployerAccountViewModel viewModel)
//        {
//            // Arrange
//            viewModel.ChangeAccountName = false;

//            // Act
//            var result = await _employerAccountController.AccountName(hashedAccountId, viewModel) as ViewResult;

//            // Assert
//            _orchestrator.Verify(m => m.SetEmployerAccountName(hashedAccountId, viewModel, UserId), Times.Once);
//        }

//        [Test, MoqAutoData]
//        public async Task Then_Errors_Are_Returned(string hashedAccountId, RenameEmployerAccountViewModel viewModel)
//        {
//            // Arrange
//            viewModel.ChangeAccountName = false;

//            _orchestrator
//                .Setup(m => m.SetEmployerAccountName(hashedAccountId, viewModel, UserId))
//                .ReturnsAsync(new OrchestratorResponse<RenameEmployerAccountViewModel>
//                {
//                    Status = HttpStatusCode.BadRequest
//                });

//            // Act
//            var result = await _employerAccountController.AccountName(hashedAccountId, viewModel) as ViewResult;
//            var model = result.Model as OrchestratorResponse<RenameEmployerAccountViewModel>;

//            // Assert
//            model.Status.Should().Be(HttpStatusCode.BadRequest);
//        }

//        [Test, MoqAutoData]
//        public async Task Then_Should_Redirect_To_Success(string hashedAccountId, RenameEmployerAccountViewModel viewModel)
//        {
//            // Arrange
//            viewModel.ChangeAccountName = false;

//            _orchestrator
//                .Setup(m => m.SetEmployerAccountName(hashedAccountId, viewModel, UserId))
//                .ReturnsAsync(new OrchestratorResponse<RenameEmployerAccountViewModel>
//                {
//                    Status = HttpStatusCode.OK
//                });

//            // Act
//            var result =
//                await _employerAccountController.AccountName(hashedAccountId, viewModel) as RedirectToRouteResult;

//            // Assert
//            result.RouteName.Should().Be(RouteNames.AccountNameConfirmSuccess);
//        }
//    }
//}