using System;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.PrivateBlockchainFacade.Contract.Events;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities;
using Moq;
using Xunit;

namespace MAVN.Service.PrivateBlockchainFacade.Tests
{
    public class TransactionCompletedInBlockchainHandlerTests
    {
        private readonly Mock<IOperationStatusUpdater> _operationStatusUpdaterMock = new Mock<IOperationStatusUpdater>();
        private readonly Mock<IOperationsFetcher> _operationsFetcherMock = new Mock<IOperationsFetcher>();
        private readonly Mock<IRabbitPublisher<TransactionSucceededEvent>> _transcationSucceededPublisherMock = new Mock<IRabbitPublisher<TransactionSucceededEvent>>();

        private const string ValidTransactionHash = "0x09273094bf95663c9cef1b794816bc7bc530bb736311140458dc588baa26092a";
        private const string FakeOperationId = "0e81662a-d9cd-4027-be2f-a8cda45cb197";

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task HandleAsync_InvalidInputParameters_OperationUpdateNeverCalled(string hash)
        {
            var sut = CreateSutInstance();

            await sut.HandleAsync(hash);

            _operationStatusUpdaterMock.Verify(
                m => m.FailAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task HandleAsync_OperationNotFoundInDbButFoundInExecutor_OperationUpdateGetsCalled()
        {
            _operationStatusUpdaterMock.Setup(x => x.SucceedAsync(It.IsAny<string>()))
                .ReturnsAsync(OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.OperationNotFound));

            _operationStatusUpdaterMock.Setup(x => x.SyncWithBlockchainAsync(It.IsAny<string>()))
                .ReturnsAsync(OperationStatusUpdateResultModel.Succeeded);

            var sut = CreateSutInstance();

            await sut.HandleAsync(ValidTransactionHash);

            _operationStatusUpdaterMock.Verify(
                m => m.SucceedAsync(It.IsAny<string>()),
                Times.Once);

        }

        [Fact]
        public async Task HandleAsync_ErrorWhenTryingToUpdateToSucceeded_PublisherNotCalled()
        {
            _operationStatusUpdaterMock.Setup(x => x.SucceedAsync(It.IsAny<string>()))
                .ReturnsAsync(OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.OperationNotFound));

            _operationStatusUpdaterMock.Setup(x => x.SyncWithBlockchainAsync(ValidTransactionHash))
                .ReturnsAsync(OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.OperationNotFound));

            var sut = CreateSutInstance();

            await sut.HandleAsync(ValidTransactionHash);

            _transcationSucceededPublisherMock.Verify(
                m => m.PublishAsync(It.IsAny<TransactionSucceededEvent>()),
                Times.Never);
        }

        [Fact]
        public async Task HandleAsync_OperationNotFoundAfterItWasUpdatedToSucceeded_PublisherNotCalled()
        {
            _operationStatusUpdaterMock.Setup(x => x.SucceedAsync(It.IsAny<string>()))
                .ReturnsAsync(OperationStatusUpdateResultModel.Succeeded());

            _operationsFetcherMock.Setup(x => x.GetByHashAsync(ValidTransactionHash))
                .ReturnsAsync((IOperation) null);

            var sut = CreateSutInstance();

            await sut.HandleAsync(ValidTransactionHash);

            _transcationSucceededPublisherMock.Verify(
                m => m.PublishAsync(It.IsAny<TransactionSucceededEvent>()),
                Times.Never);
        }

        [Fact]
        public async Task HandleAsync_PublisherGetsCalled()
        {
            _operationStatusUpdaterMock.Setup(x => x.SucceedAsync(It.IsAny<string>()))
                .ReturnsAsync(OperationStatusUpdateResultModel.Succeeded());

            _operationsFetcherMock.Setup(x => x.GetByHashAsync(ValidTransactionHash))
                .ReturnsAsync(new OperationEntity{Id = Guid.Parse(FakeOperationId) });

            var sut = CreateSutInstance();

            await sut.HandleAsync(ValidTransactionHash);

            _transcationSucceededPublisherMock.Verify(
                m => m.PublishAsync(It.Is<TransactionSucceededEvent>(e => e.OperationId == FakeOperationId)),
                Times.Once);
        }

        private ITransactionCompletedInBlockchainHandler CreateSutInstance()
        {
            return new TransactionCompletedInBlockchainHandler(
                _operationStatusUpdaterMock.Object,
                _operationsFetcherMock.Object,
                _transcationSucceededPublisherMock.Object,
                EmptyLogFactory.Instance);
        }
    }
}
