using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NServiceBus;
using SFA.DAS.EAS.Portal.Application.Commands;
using SFA.DAS.EAS.Portal.Application.Services;

namespace SFA.DAS.EAS.Portal.UnitTests.Worker.EventHandlers
{
    public class EventHandlerTestsFixture<TEvent, TEventHandler, TCommand>
        where TEventHandler : IHandleMessages<TEvent>
        where TCommand : class, ICommand<TEvent>
    {
        public TEvent Message { get; set; }
        public TEvent ExpectedMessage { get; set; }
        public IHandleMessages<TEvent> Handler { get; set; }
        public string MessageId { get; set; }
        public Mock<IMessageContext> MessageContext { get; set; }
        public Mock<IMessageHandlerContext> MessageHandlerContext { get; set; }
        public Mock<TCommand> Command { get; set; }
        
        public EventHandlerTestsFixture(Func<IMessageContext, IHandleMessages<TEvent>> constructHandler = null)
        {
            var fixture = new Fixture();
            Message = fixture.Create<TEvent>();

            MessageContext = new Mock<IMessageContext>();
            
            MessageId = fixture.Create<string>();
            //todo: use nservicebus's version??
            MessageHandlerContext = new Mock<IMessageHandlerContext>();
            MessageHandlerContext.Setup(c => c.MessageId).Returns(MessageId);

            var messageHeaders = new Mock<IReadOnlyDictionary<string, string>>();
            messageHeaders.SetupGet(c => c["NServiceBus.TimeSent"]).Returns(DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss:ffffff Z", CultureInfo.InvariantCulture));
            MessageHandlerContext.Setup(c => c.MessageHeaders).Returns(messageHeaders.Object);

            Command = new Mock<TCommand>();
            
            Handler = constructHandler != null ? constructHandler(MessageContext.Object) : ConstructHandler();
        }

        public virtual Task Handle()
        {
            ExpectedMessage = Message.Clone();
            return Handler.Handle(Message, MessageHandlerContext.Object);
        }

        private TEventHandler ConstructHandler()
        {
            return (TEventHandler)Activator.CreateInstance(typeof(TEventHandler), Command.Object, MessageContext.Object);
        }

        public EventHandlerTestsFixture<TEvent, TEventHandler, TCommand> VerifyCommandExecutedWithUnchangedEvent()
        {
            Command.Verify(c => c.Execute(
                It.Is<TEvent>(p => p.IsEqual(ExpectedMessage)), It.IsAny<CancellationToken>()),
                Times.Once);

            //todo: want test to fluent chain using derived methods, covariance?
            return this;
        }

        public EventHandlerTestsFixture<TEvent, TEventHandler, TCommand> VerifyMessageContextIsInitialised()
        {
            //todo: the old chestnut of having to verify a mocked object as param. switching to nservicebus's fake should help
            MessageContext.Verify(mc => mc.Initialise(It.IsAny<IMessageHandlerContext>()), Times.Once);

            return this;
        }
    }
}