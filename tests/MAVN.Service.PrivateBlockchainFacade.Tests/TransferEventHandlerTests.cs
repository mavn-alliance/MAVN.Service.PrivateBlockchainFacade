using System;
using System.Threading.Tasks;
using Common;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.PrivateBlockchainFacade.Contract.Events;
using MAVN.Service.PrivateBlockchainFacade.Contract.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Balances;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities;
using Lykke.Service.QuorumOperationExecutor.Client;
using Moq;
using Xunit;

namespace MAVN.Service.PrivateBlockchainFacade.Tests
{
    public class TransferEventHandlerTests
    {
        private readonly Mock<IRabbitPublisher<TransferDetectedEvent>> _transferDetectedPublisherMock = new Mock<IRabbitPublisher<TransferDetectedEvent>>();

        private readonly Mock<IRabbitPublisher<P2PTransferDetectedEvent>> _p2PTransferPublisherMock = new Mock<IRabbitPublisher<P2PTransferDetectedEvent>>();

        private readonly Mock<IBalanceService> _balanceServiceMock = new Mock<IBalanceService>();
        
        private readonly Mock<IWalletOwnersRepository> _walletOwnersRepositoryMock = new Mock<IWalletOwnersRepository>();
        
        private readonly Mock<IOperationsFetcher> _operationsFetcherMock = new Mock<IOperationsFetcher>();
        
        private readonly Mock<IQuorumOperationExecutorClient> _quorumOperationsExecutorClientMock = new Mock<IQuorumOperationExecutorClient>();
        
        private const string FakeWalletAddress1 = "0x7d275eb17ceaae04b17768d4459741bae062ee09";
        
        private const string FakeWalletAddress2 = "0xd0eaa2c6fede67044e93b813a878e4d67c65797c";
        
        private const string ValidTransactionHash = "0x09273094bf95663c9cef1b794816bc7bc530bb736311140458dc588baa26092a";
        
        private const long ValidAmount = 100;
        
        private const long ValidBalance = 100;

        private const long FakeStakedAmount = 100;

        private const string FakeCustomerId = "customerId";


        [Theory]
        [InlineData("", FakeWalletAddress2, ValidAmount, ValidTransactionHash)]
        [InlineData(null, FakeWalletAddress2, ValidAmount, ValidTransactionHash)]
        [InlineData(TransferEventHandler.EmptyWalletAddress, FakeWalletAddress2, ValidAmount, ValidTransactionHash)]
        [InlineData(FakeWalletAddress1, "", ValidAmount, ValidTransactionHash)]
        [InlineData(FakeWalletAddress1, null, ValidAmount, ValidTransactionHash)]
        [InlineData(FakeWalletAddress1, TransferEventHandler.EmptyWalletAddress, ValidAmount, ValidTransactionHash)]
        [InlineData(FakeWalletAddress1, FakeWalletAddress2, 0, ValidTransactionHash)]
        [InlineData(FakeWalletAddress1, FakeWalletAddress2, -100, ValidTransactionHash)]
        [InlineData(FakeWalletAddress1, FakeWalletAddress2, ValidAmount, "")]
        [InlineData(FakeWalletAddress1, FakeWalletAddress2, ValidAmount, null)]
        public async Task HandleAsync_InvalidInputParameters_Returns(
            string sourceAddress, 
            string targetAddress, 
            long amount, 
            string transactionHash)
        {
            _walletOwnersRepositoryMock
                .Setup(x => x.GetByWalletAddressAsync(It.IsAny<string>()))
                .ReturnsAsync(new WalletOwnerEntity())
                .Verifiable();
            
            var sut = CreateSutInstance();

            await sut.HandleAsync(sourceAddress, targetAddress, amount, transactionHash, DateTime.UtcNow);
            
            _walletOwnersRepositoryMock.Verify(x => x.GetByWalletAddressAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_UnknownSourceWalletAddress_UpdatesOnlyReceiverBalance()
        {
            _walletOwnersRepositoryMock
                .Setup(x => x.GetByWalletAddressAsync(FakeWalletAddress1))
                .ReturnsAsync((IWalletOwner) null);

            _walletOwnersRepositoryMock
                .Setup(x => x.GetByWalletAddressAsync(FakeWalletAddress2))
                .ReturnsAsync(new WalletOwnerEntity
                {
                    OwnerId = FakeCustomerId
                });

            _balanceServiceMock.Setup(x => x.ForceBalanceUpdateAsync(It.IsAny<string>(), It.IsAny<OperationType>(), It.IsAny<Guid>()))
                .Verifiable();

            _operationsFetcherMock.Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(new OperationEntity {ContextJson = @"{""RequestId"":""Id"",""BonusReason"":""reason""}"});
            
            var sut = CreateSutInstance();

            await sut.HandleAsync(
                FakeWalletAddress1, 
                FakeWalletAddress2, 
                ValidAmount, 
                ValidTransactionHash,
                DateTime.UtcNow);

            _balanceServiceMock.Verify(
                x => x.ForceBalanceUpdateAsync(FakeCustomerId, It.IsAny<OperationType>(), It.IsAny<Guid>()),
                Times.Once);
            _balanceServiceMock.Verify(x => x.ForceBalanceUpdateAsync(
                It.Is<string>(c => c != FakeCustomerId), It.IsAny<OperationType>(), It.IsAny<Guid>()), Times.Never);
            _p2PTransferPublisherMock.Verify(x => x.PublishAsync(It.IsAny<P2PTransferDetectedEvent>()),Times.Never);
            _transferDetectedPublisherMock.Verify(x => x.PublishAsync(It.IsAny<TransferDetectedEvent>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_UnknownTargetWalletAddress_UpdatesOnlySourceCustomerBalance()
        {
            _walletOwnersRepositoryMock
                .Setup(x => x.GetByWalletAddressAsync(FakeWalletAddress1))
                .ReturnsAsync(new WalletOwnerEntity
                {
                    OwnerId = FakeCustomerId
                });
            
            _walletOwnersRepositoryMock
                .Setup(x => x.GetByWalletAddressAsync(FakeWalletAddress2))
                .ReturnsAsync((IWalletOwner) null);

            _balanceServiceMock.Setup(x => x.ForceBalanceUpdateAsync(It.IsAny<string>(), It.IsAny<OperationType>(), It.IsAny<Guid>()))
                .Verifiable();
            
            _operationsFetcherMock.Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(new OperationEntity {ContextJson = @"{""RequestId"":""Id"",""BonusReason"":""reason""}"});

            var sut = CreateSutInstance();

            await sut.HandleAsync(
                FakeWalletAddress1, 
                FakeWalletAddress2, 
                ValidAmount, 
                ValidTransactionHash,
                DateTime.UtcNow);

            _balanceServiceMock.Verify(
                x => x.ForceBalanceUpdateAsync(FakeCustomerId, It.IsAny<OperationType>(), It.IsAny<Guid>()),
                Times.Once);
            _balanceServiceMock.Verify(x => x.ForceBalanceUpdateAsync(
                It.Is<string>(c => c != FakeCustomerId), It.IsAny<OperationType>(), It.IsAny<Guid>()), Times.Never);
            _p2PTransferPublisherMock.Verify(x => x.PublishAsync(It.IsAny<P2PTransferDetectedEvent>()), Times.Never);
            _transferDetectedPublisherMock.Verify(x => x.PublishAsync(It.IsAny<TransferDetectedEvent>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_OperationFound_FinishesSuccessfully()
        {
            _walletOwnersRepositoryMock
                .Setup(x => x.GetByWalletAddressAsync(It.IsAny<string>()))
                .ReturnsAsync(new WalletOwnerEntity());
            
            _balanceServiceMock
                .Setup(x => x.ForceBalanceUpdateAsync(It.IsAny<string>(), It.IsAny<OperationType>(), It.IsAny<Guid>()))
                .ReturnsAsync(CustomerBalanceResultModel.Succeeded(ValidBalance, FakeStakedAmount))
                .Verifiable();

            _operationsFetcherMock
                .Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(new OperationEntity
                {
                    ContextJson = new TokensTransferContext {RequestId = "whatever"}.ToJson()
                });
            
            _transferDetectedPublisherMock
                .Setup(x => x.PublishAsync(It.IsAny<TransferDetectedEvent>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var sut = CreateSutInstance();
            
            await sut.HandleAsync(
                FakeWalletAddress1, 
                FakeWalletAddress2, 
                ValidAmount, 
                ValidTransactionHash,
                DateTime.UtcNow);

            _balanceServiceMock.Verify(
                x => x.ForceBalanceUpdateAsync(It.IsAny<string>(), It.IsAny<OperationType>(), It.IsAny<Guid>()),
                Times.Exactly(2));
            _p2PTransferPublisherMock.Verify(x => x.PublishAsync(It.IsAny<P2PTransferDetectedEvent>()),Times.Once);
            _transferDetectedPublisherMock.Verify(x => x.PublishAsync(It.IsAny<TransferDetectedEvent>()), Times.Once);
        }

        private ITransferEventHandler CreateSutInstance()
        {
            return new TransferEventHandler(
                _transferDetectedPublisherMock.Object,
                _p2PTransferPublisherMock.Object,
                EmptyLogFactory.Instance,
                _balanceServiceMock.Object,
                _walletOwnersRepositoryMock.Object,
                _operationsFetcherMock.Object,
                _quorumOperationsExecutorClientMock.Object);
        }
    }
}
