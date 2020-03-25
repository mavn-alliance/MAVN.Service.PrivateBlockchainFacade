using System.Threading.Tasks;
using Lykke.Logs;
using Falcon.Numerics;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.PrivateBlockchainFacade.Contract.Events;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Fees;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Transfers;
using Lykke.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers;
using Moq;
using Xunit;

namespace Lykke.Service.PrivateBlockchainFacade.Tests
{
    public class TransferToExternalRequestedHandlerTests
    {
        private const string FakeOperationId = "opId";
        private const string FakeContractAddress = "0xAddress";
        private const string FakeCustomerId = "61be11f1-2d32-419d-82da-c71507e53634";
        private readonly Money18 InvalidAmount = 0;

        private readonly Mock<ITransferService> _transfersServiceMock = new Mock<ITransferService>();
        private readonly Mock<IFeesService> _feesServiceMock = new Mock<IFeesService>();
        private readonly Mock<IRabbitPublisher<TransferToExternalFailedEvent>> _transferToExternalFailedPublisherMock = new Mock<IRabbitPublisher<TransferToExternalFailedEvent>>();

        [Fact]
        public async Task HandleAsync_ErrorWhenCreatingGenericTransfer_TransferToExternalFailedPublisherCalled()
        {
            _feesServiceMock.Setup(x => x.GetTransfersToPublicFeeAsync())
                .ReturnsAsync(0);

            _transfersServiceMock.Setup(x =>
                    x.TransferToExternalAsync(FakeCustomerId, FakeContractAddress, InvalidAmount, 0, FakeOperationId))
                .ReturnsAsync(new TransferResultModel { Error = TransferError.NotEnoughFunds });

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeOperationId, FakeCustomerId, InvalidAmount, FakeContractAddress);

            _transferToExternalFailedPublisherMock.Verify(x =>
                x.PublishAsync(It.Is<TransferToExternalFailedEvent>(e =>
                    e.CustomerId == FakeCustomerId && e.Amount == InvalidAmount)), Times.Once);
        }

        private TransferToExternalRequestedHandler CreateSutInstance()
        {
            return new TransferToExternalRequestedHandler(
                _transfersServiceMock.Object,
                _feesServiceMock.Object,
                _transferToExternalFailedPublisherMock.Object,
                EmptyLogFactory.Instance);
        }
    }
}
