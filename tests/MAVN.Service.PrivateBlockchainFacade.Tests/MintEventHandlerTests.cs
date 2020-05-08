using System;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.PrivateBlockchainFacade.Contract.Events;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Balances;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities;
using MAVN.Service.QuorumOperationExecutor.Client;
using MAVN.Service.QuorumOperationExecutor.Client.Models.Responses;
using Moq;
using Xunit;

namespace MAVN.Service.PrivateBlockchainFacade.Tests
{
    public class MintEventHandlerTests
    {
        [Fact]
        public async Task TryToHandleMintEvent_HashIsNull_ReturnAndLogWarning()
        {
            var publisher = Mock.Of<IRabbitPublisher<BonusRewardDetectedEvent>>();
            var balanceService = Mock.Of<IBalanceService>();
            var walletOwnerRepo = new Mock<IWalletOwnersRepository>();
            var executorClient = new Mock<IQuorumOperationExecutorClient>();
            var operationsFetcher = new Mock<IOperationsFetcher>();
            walletOwnerRepo.Setup(x => x.GetByWalletAddressAsync(It.IsAny<string>()))
                .ReturnsAsync(new WalletOwnerEntity())
                .Verifiable();

            var sut = new MintEventHandler(publisher,
                walletOwnerRepo.Object,
                balanceService,
                EmptyLogFactory.Instance,
                executorClient.Object,
                operationsFetcher.Object);

            await sut.HandleAsync(null, 123, "address", DateTime.UtcNow);

            walletOwnerRepo.Verify(x => x.GetByWalletAddressAsync(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async Task TryToHandleMintEvent_InvalidAmount_ReturnAndLogWarning(long amount)
        {
            var publisher = Mock.Of<IRabbitPublisher<BonusRewardDetectedEvent>>();
            var balanceService = Mock.Of<IBalanceService>();
            var walletOwnerRepo = new Mock<IWalletOwnersRepository>();
            var executorClient = new Mock<IQuorumOperationExecutorClient>();
            var operationsFetcher = new Mock<IOperationsFetcher>();
            walletOwnerRepo.Setup(x => x.GetByWalletAddressAsync(It.IsAny<string>()))
                .ReturnsAsync(new WalletOwnerEntity())
                .Verifiable();

            var sut = new MintEventHandler(publisher,
                walletOwnerRepo.Object,
                balanceService,
                EmptyLogFactory.Instance,
                executorClient.Object,
                operationsFetcher.Object);

            await sut.HandleAsync("hash", amount, "address", DateTime.UtcNow);

            walletOwnerRepo.Verify(x => x.GetByWalletAddressAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task TryToHandleMintEvent_WalletDoesNotExistForThisAddress_ReturnAndLogError()
        {
            var publisher = new Mock<IRabbitPublisher<BonusRewardDetectedEvent>>();
            var balanceService = Mock.Of<IBalanceService>();
            var executorClient = new Mock<IQuorumOperationExecutorClient>();
            var operationsFetcher = new Mock<IOperationsFetcher>();
            publisher.Setup(x => x.PublishAsync(It.IsAny<BonusRewardDetectedEvent>()))
                .Verifiable();
            var walletOwnerRepo = new Mock<IWalletOwnersRepository>();
            walletOwnerRepo.Setup(x => x.GetByWalletAddressAsync(It.IsAny<string>()))
                .ReturnsAsync((WalletOwnerEntity) null);

            var sut = new MintEventHandler(
                publisher.Object,
                walletOwnerRepo.Object,
                balanceService,
                EmptyLogFactory.Instance,
                executorClient.Object,
                operationsFetcher.Object);

            await sut.HandleAsync("hash", 123, "address", DateTime.UtcNow);

            publisher.Verify(x => x.PublishAsync(It.IsAny<BonusRewardDetectedEvent>()), Times.Never);
        }

        [Fact]
        public async Task TryToHandleMintEvent_OperationNotFoundInDb_ReturnAndLogError()
        {
            var publisher = new Mock<IRabbitPublisher<BonusRewardDetectedEvent>>();
            publisher.Setup(x => x.PublishAsync(It.IsAny<BonusRewardDetectedEvent>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var balanceService = new Mock<IBalanceService>();
            balanceService.Setup(x =>
                    x.ForceBalanceUpdateAsync(It.IsAny<string>(), It.IsAny<OperationType>(), It.IsAny<Guid>()))
                .Verifiable();

            var executorClient = new Mock<IQuorumOperationExecutorClient>();
            executorClient.Setup(x => x.TransactionsApi.GetTransactionStateAsync(It.IsAny<string>()))
                .ReturnsAsync(new GetTransactionStateResponse
                {
                    Error = GetTransactionStateError.None, OperationId = Guid.NewGuid()
                });
            
            var operationsFetcher = new Mock<IOperationsFetcher>();
            operationsFetcher.Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync((IOperation) null);
            operationsFetcher.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((IOperation) null);

            var walletOwnerRepo = new Mock<IWalletOwnersRepository>();
            walletOwnerRepo.Setup(x => x.GetByWalletAddressAsync(It.IsAny<string>()))
                .ReturnsAsync(new WalletOwnerEntity());

            var sut = new MintEventHandler(
                publisher.Object,
                walletOwnerRepo.Object, 
                balanceService.Object, 
                EmptyLogFactory.Instance,
                executorClient.Object,
                operationsFetcher.Object);

            await sut.HandleAsync("hash", 123, "address", DateTime.UtcNow);

            balanceService.Verify(x => x.ForceBalanceUpdateAsync
                (It.IsAny<string>(), It.IsAny<OperationType>(), It.IsAny<Guid>()), Times.Never);
            publisher.Verify(x => x.PublishAsync(It.IsAny<BonusRewardDetectedEvent>()), Times.Never);
        }

        [Fact]
        public async Task TryToHandleMintEvent_PublisherCalled()
        {
            var publisher = new Mock<IRabbitPublisher<BonusRewardDetectedEvent>>();
            publisher.Setup(x => x.PublishAsync(It.IsAny<BonusRewardDetectedEvent>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var balanceService = new Mock<IBalanceService>();
            balanceService.Setup(x =>
                    x.ForceBalanceUpdateAsync(It.IsAny<string>(), It.IsAny<OperationType>(), It.IsAny<Guid>()))
                .Verifiable();
            
            var executorClient = new Mock<IQuorumOperationExecutorClient>();
            
            var operationsFetcher = new Mock<IOperationsFetcher>();
            operationsFetcher.Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(new OperationEntity {ContextJson = @"{""RequestId"":""Id"",""BonusReason"":""reason""}"});

            var walletOwnerRepo = new Mock<IWalletOwnersRepository>();
            walletOwnerRepo.Setup(x => x.GetByWalletAddressAsync(It.IsAny<string>()))
                .ReturnsAsync(new WalletOwnerEntity());

            var sut = new MintEventHandler(
                publisher.Object,
                walletOwnerRepo.Object,
                balanceService.Object, 
                EmptyLogFactory.Instance,
                executorClient.Object,
                operationsFetcher.Object);

            await sut.HandleAsync("hash", 123, "address", DateTime.UtcNow);

            balanceService.Verify(x => x.ForceBalanceUpdateAsync
                (It.IsAny<string>(), It.IsAny<OperationType>(), It.IsAny<Guid>()), Times.Once);
            publisher.Verify(x => x.PublishAsync(It.IsAny<BonusRewardDetectedEvent>()),Times.Once);
        }

    }
}
