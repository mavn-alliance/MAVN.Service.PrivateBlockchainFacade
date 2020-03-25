using System;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.PrivateBlockchainFacade.Contract.Events;
using Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq;
using Lykke.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers;
using Moq;
using Xunit;

namespace Lykke.Service.PrivateBlockchainFacade.Tests
{
    public class CustomerRegisteredInBlockchainHandlerTests
    {
        private readonly Mock<IRabbitPublisher<CustomerWalletCreatedEvent>> _customerWalletCreationPublisherMock = new Mock<IRabbitPublisher<CustomerWalletCreatedEvent>>();
        
        private const string ValidTransactionHash = "0x09273094bf95663c9cef1b794816bc7bc530bb736311140458dc588baa26092a";
        private const string ValidCustomerId = "cdd477c1-2711-4c20-89b3-b3e96e4b8ba1";

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000", ValidTransactionHash)]
        [InlineData("00000000-0000-0000-0000-000000000000", "")]
        [InlineData("00000000-0000-0000-0000-000000000000", null)]
        [InlineData(ValidCustomerId, "")]
        [InlineData(ValidCustomerId, null)]
        public async Task HandleAsync_InvalidInputParameters_OperationUpdateNeverCalled(Guid customerId, string hash)
        {
            var sut = CreateSutInstance();

            await sut.HandleAsync(customerId, hash);
           
            _customerWalletCreationPublisherMock.Verify(
                m => m.PublishAsync(It.IsAny<CustomerWalletCreatedEvent>()),
                Times.Never);
        }

        [Theory]
        [InlineData(ValidCustomerId, ValidTransactionHash)]
        public async Task HandleAsync_StatusUpdateSucceeded_PublisherGetsCalled(Guid customerId, string hash)
        {            
            var sut = CreateSutInstance();

            await sut.HandleAsync(customerId, hash);
           
            
            _customerWalletCreationPublisherMock.Verify(
                m => m.PublishAsync(It.IsAny<CustomerWalletCreatedEvent>()),
                Times.Once);
        }

        private ICustomerRegisteredInBlockchainHandler CreateSutInstance()
        {
            return new CustomerRegisteredInBlockchainHandler(
                _customerWalletCreationPublisherMock.Object,
                EmptyLogFactory.Instance);
        }
    }
}
