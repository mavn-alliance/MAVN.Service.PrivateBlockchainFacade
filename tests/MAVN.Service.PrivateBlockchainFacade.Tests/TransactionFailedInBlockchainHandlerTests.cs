using System;
using System.Threading.Tasks;
using Lykke.Logs;
using MAVN.Numerics;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.PrivateBlockchainFacade.Contract.Events;
using MAVN.Service.PrivateBlockchainFacade.Contract.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace MAVN.Service.PrivateBlockchainFacade.Tests
{
    public class TransactionFailedInBlockchainHandlerTests
    {
        private readonly Mock<IOperationStatusUpdater> _operationStatusUpdaterMock = new Mock<IOperationStatusUpdater>();
        private readonly Mock<IOperationsFetcher> _operationsFetcherMock = new Mock<IOperationsFetcher>();
        private readonly Mock<IWalletOwnersRepository> _walletOwnersRepoMock = new Mock<IWalletOwnersRepository>();
        private readonly Mock<IRabbitPublisher<P2PTransferFailedEvent>> _p2PFailedPublisherMock = new Mock<IRabbitPublisher<P2PTransferFailedEvent>>();
        private readonly Mock<IRabbitPublisher<TransactionFailedEvent>> _transactionFailedPublisherMock = new Mock<IRabbitPublisher<TransactionFailedEvent>>();
        private readonly Mock<IRabbitPublisher<WalletStatusChangeFailedEvent>> _walletStatusChangeFailedPublisher = new Mock<IRabbitPublisher<WalletStatusChangeFailedEvent>>();
        private readonly Mock<IRabbitPublisher<TransferToExternalFailedEvent>> _transferToExternalFailedPublisher = new Mock<IRabbitPublisher<TransferToExternalFailedEvent>>();

        private const string ValidTransactionHash = "0x09273094bf95663c9cef1b794816bc7bc530bb736311140458dc588baa26092a";
        private const string FakeSenderAddress = "0xe26a8c3dc20ef9e8dc3373162e00e9eaa1f10d82";
        private const string FakeReceiverAddress = "0x0967c67827aBc955AC5eAb43393d75336b62Ae8e";
        private static  Money18 ValidAmount = 100;
        private const string FakeRequestId = "071ba315-389d-46f4-86e3-1b04ad5cbac7";
        private const string FakeSenderId = "id1";
        private const string FakeReceiverId = "id2";

        private string FakeTransactionContext =
            $"{{\"SenderWalletAddress\":\"{FakeSenderAddress}\"," +
            $"\"RecipientWalletAddress\":\"{FakeReceiverAddress}\"," +
            $"\"Amount\":\"{ValidAmount}\",\"RequestId\":\"{FakeRequestId}\"}}";

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
            _operationStatusUpdaterMock.Setup(x => x.FailAsync(It.IsAny<string>()))
                .ReturnsAsync(OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.OperationNotFound));

            _operationStatusUpdaterMock.Setup(x => x.SyncWithBlockchainAsync(It.IsAny<string>()))
                .ReturnsAsync(OperationStatusUpdateResultModel.Succeeded);

            var sut = CreateSutInstance();

            await sut.HandleAsync("hash");

            _operationStatusUpdaterMock.Verify(
                m => m.FailAsync(It.IsAny<string>()),
                Times.Once);

        }

        [Theory]
        [InlineData(ValidTransactionHash)]
        public async Task HandleAsync_ValidInput_Parameters_OperationUpdateGetsCalled(string hash)
        {
            _operationStatusUpdaterMock.Setup(x => x.FailAsync(It.IsAny<string>()))
                .ReturnsAsync(OperationStatusUpdateResultModel.Succeeded());

            var sut = CreateSutInstance();

            await sut.HandleAsync(hash);

            _operationStatusUpdaterMock.Verify(
                m => m.FailAsync(It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task HandleAsync_StatusUpdaterError_OperationsFetcherNotCalled()
        {
            _operationStatusUpdaterMock.Setup(x => x.FailAsync(It.IsAny<string>()))
                .ReturnsAsync(OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.OperationNotFound));

            _operationStatusUpdaterMock.Setup(x => x.SyncWithBlockchainAsync(It.IsAny<string>()))
                .ReturnsAsync(OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.OperationNotFound));

            var sut = CreateSutInstance();

            await sut.HandleAsync(ValidTransactionHash);

            _operationsFetcherMock.Verify(x => x.GetByHashAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_AlreadyMarkedAsFailedOperationNotFoundInDb_WalletOwnersRepoNotCalled()
        {
            _operationStatusUpdaterMock.Setup(x => x.FailAsync(It.IsAny<string>()))
                .ReturnsAsync(OperationStatusUpdateResultModel.Succeeded());

            _operationsFetcherMock.Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync((IOperation)null);

            var sut = CreateSutInstance();

            await sut.HandleAsync(ValidTransactionHash);

            _walletOwnersRepoMock.Verify(x => x.GetByWalletAddressAsync(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(OperationType.CustomerBonusReward)]
        [InlineData(OperationType.GenericOperation)]
        [InlineData(OperationType.CustomerWalletCreation)]
        public async Task HandleAsync_OperationIsNotTokensTransfer_WalletOwnersRepoNotCalledAndTransactionFailedPublisherCalled(OperationType operationType)
        {
            _operationStatusUpdaterMock.Setup(x => x.FailAsync(It.IsAny<string>()))
                .ReturnsAsync(OperationStatusUpdateResultModel.Succeeded());

            _operationsFetcherMock.Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(new OperationEntity
                {
                    Type = operationType
                });

            var sut = CreateSutInstance();

            await sut.HandleAsync(ValidTransactionHash);

            _walletOwnersRepoMock.Verify(x => x.GetByWalletAddressAsync(It.IsAny<string>()), Times.Never);
            _transactionFailedPublisherMock.Verify(x => x.PublishAsync(It.IsAny<TransactionFailedEvent>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_SenderIsNotACustomer_P2PFailedPublisherNotCalled()
        {
            _operationStatusUpdaterMock.Setup(x => x.FailAsync(It.IsAny<string>()))
                .ReturnsAsync(OperationStatusUpdateResultModel.Succeeded());

            _walletOwnersRepoMock.Setup(x => x.GetByWalletAddressAsync(FakeSenderAddress))
                .ReturnsAsync((IWalletOwner)null);

            _operationsFetcherMock.Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(new OperationEntity
                {
                    Type = OperationType.TokensTransfer,
                    ContextJson = FakeTransactionContext
                });

            var sut = CreateSutInstance();

            await sut.HandleAsync(ValidTransactionHash);

            _p2PFailedPublisherMock.Verify(x => x.PublishAsync(It.IsAny<P2PTransferFailedEvent>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_ReceiverIsNotACustomer_P2PFailedPublisherNotCalled()
        {
            _operationStatusUpdaterMock.Setup(x => x.FailAsync(It.IsAny<string>()))
                .ReturnsAsync(OperationStatusUpdateResultModel.Succeeded());

            _walletOwnersRepoMock.Setup(x => x.GetByWalletAddressAsync(FakeSenderAddress))
                .ReturnsAsync(new WalletOwnerEntity
                {
                    OwnerId = FakeSenderId,
                    WalletId = FakeSenderAddress
                });

            var transactionContext = new TokensTransferContext()
            {
                 SenderWalletAddress = FakeSenderAddress,
                 RecipientWalletAddress = FakeReceiverAddress,
                 Amount = ValidAmount,
                 RequestId = FakeRequestId
            };

            var operationContextJson = JsonConvert.SerializeObject(transactionContext);

            _walletOwnersRepoMock.Setup(x => x.GetByWalletAddressAsync(FakeReceiverAddress))
                .ReturnsAsync((IWalletOwner)null);

            _operationsFetcherMock.Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(new OperationEntity
                {
                    Type = OperationType.TokensTransfer,
                    ContextJson = operationContextJson
                });

            var sut = CreateSutInstance();

            await sut.HandleAsync(ValidTransactionHash);

            _p2PFailedPublisherMock.Verify(x => x.PublishAsync(It.IsAny<P2PTransferFailedEvent>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_PublishedP2PFailedEvent()
        {
            _operationStatusUpdaterMock.Setup(x => x.FailAsync(It.IsAny<string>()))
                .ReturnsAsync(OperationStatusUpdateResultModel.Succeeded());

            _walletOwnersRepoMock.Setup(x => x.GetByWalletAddressAsync(FakeSenderAddress))
                .ReturnsAsync(new WalletOwnerEntity
                {
                    OwnerId = FakeSenderId,
                    WalletId = FakeSenderAddress
                });

            _walletOwnersRepoMock.Setup(x => x.GetByWalletAddressAsync(FakeReceiverAddress))
                .ReturnsAsync(new WalletOwnerEntity
                {
                    OwnerId = FakeReceiverId,
                    WalletId = FakeReceiverAddress
                });

            _operationsFetcherMock.Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(new OperationEntity
                {
                    Type = OperationType.TokensTransfer,
                    ContextJson = FakeTransactionContext,
                    TransactionHash = ValidTransactionHash
                });

            var sut = CreateSutInstance();

            await sut.HandleAsync(ValidTransactionHash);

            _p2PFailedPublisherMock.Verify(x => x.PublishAsync(It.Is<P2PTransferFailedEvent>(
                    obj => obj.RequestId == FakeRequestId &&
                         obj.Amount == ValidAmount &&
                         obj.ReceiverCustomerId == FakeReceiverId &&
                         obj.SenderCustomerId == FakeSenderId &&
                         obj.TransactionHash == ValidTransactionHash)),
                Times.Once);
        }

        private ITransactionFailedInBlockchainHandler CreateSutInstance()
        {
            return new TransactionFailedInBlockchainHandler(
                _operationStatusUpdaterMock.Object,
                _operationsFetcherMock.Object,
                _walletOwnersRepoMock.Object,
                _p2PFailedPublisherMock.Object,
                _transactionFailedPublisherMock.Object,
                _walletStatusChangeFailedPublisher.Object,
                _transferToExternalFailedPublisher.Object,
                EmptyLogFactory.Instance);
        }
    }
}
