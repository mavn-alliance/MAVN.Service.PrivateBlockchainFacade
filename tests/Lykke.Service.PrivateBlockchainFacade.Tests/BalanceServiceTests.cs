using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Lykke.Logs;
using Falcon.Numerics;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.PrivateBlockchainFacade.Contract.Events;
using Lykke.Service.PrivateBlockchainFacade.Contract.Operations;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Balances;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using Lykke.Service.PrivateBlockchainFacade.DomainServices.Features.Balances;
using Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities;
using Lykke.Service.QuorumOperationExecutor.Client;
using Lykke.Service.QuorumOperationExecutor.Client.Models.Responses;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

namespace Lykke.Service.PrivateBlockchainFacade.Tests
{
    public class BalanceServiceTests
    {
        private readonly Mock<IQuorumOperationExecutorClient> _quorumOperationExecutorClientMock =
            new Mock<IQuorumOperationExecutorClient>();

        private readonly Mock<IWalletsService> _walletsServiceMock = new Mock<IWalletsService>();

        private readonly Mock<IOperationsFetcher> _operationsFetcherMock = new Mock<IOperationsFetcher>();

        private readonly Mock<IRabbitPublisher<CustomerBalanceUpdatedEvent>> _customerBalanceUpdatePublisherMock =
            new Mock<IRabbitPublisher<CustomerBalanceUpdatedEvent>>();
        
        private readonly Mock<IDistributedCache> _distributedCacheMock = new Mock<IDistributedCache>(); 

        private const string FakeCustomerId = "3f1443e2-b848-4567-8fb5-ebe7337a87e9";

        private const string FakeCustomerWalletAddress = "0x7d275eb17ceaae04b17768d4459741bae062ee09";

        private Money18 ValidBalance = 100;
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task GetAsync_InvalidCustomerId_ReturnsFail(string customerId)
        {
            var sut = CreateSutInstance();

            var result = await sut.GetAsync(customerId);

            Assert.Equal(CustomerBalanceError.InvalidCustomerId, result.Error);
            Assert.Equal(0, result.Total);
        }
        
        [Fact]
        public async Task GetAsync_NoWalletRegistered_ReturnsFail()
        {
            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerWalletAddressResultModel.Failed(CustomerWalletAddressError.CustomerWalletMissing));
            
            _distributedCacheMock
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]) null);

            var sut = CreateSutInstance();

            var result = await sut.GetAsync(FakeCustomerId);

            Assert.Equal(CustomerBalanceError.CustomerWalletMissing, result.Error);
            Assert.Equal(0, result.Total);
        }

        [Theory]
        [InlineData(100, 0, 0,100)]
        [InlineData(100, 50, 10, 40)]
        [InlineData(0, 100,0, 0)]
        [InlineData(0, 0,0, 0)]
        [InlineData(-100, 50,0, 0)]
        [InlineData(-100, -50,0, 0)]
        public async Task GetAsync_AllOperationsSucceeded_ReturnsSuccess(
            long total, 
            long reservedForTransfers,
            long reservedForSeize,
            long expectedBalance)
        {
            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerWalletAddressResultModel.Succeeded(FakeCustomerWalletAddress));

            _quorumOperationExecutorClientMock
                .Setup(x => x.AddressesApi.GetBalanceForAddressAsync(It.IsAny<string>()))
                .ReturnsAsync(new AddressBalanceResponse {Balance = total});

            _operationsFetcherMock
                .Setup(x => x.GetTransfersInProgressAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<IOperation>
                {
                    new OperationEntity
                    {
                        Type = OperationType.TokensTransfer,
                        ContextJson = new TokensTransferContext {Amount = reservedForTransfers}.ToJson()
                    }
                });

            _operationsFetcherMock
                .Setup(x => x.GetSeizeOperationsInProgressAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<IOperation>
                {
                    new OperationEntity
                    {
                        Type = OperationType.SeizeToInternal,
                        ContextJson = new SeizeToInternalContext {Amount = reservedForSeize}.ToJson()
                    }
                });

            _distributedCacheMock
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]) null);

            _distributedCacheMock
                .Setup(x => x.SetAsync(It.IsAny<string>(), 
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSutInstance();

            var result = await sut.GetAsync(FakeCustomerId);

            Assert.Equal(CustomerBalanceError.None, result.Error);
            Assert.Equal(expectedBalance, result.Total);
        }

        [Theory]
        [InlineData(100, 50, 0, 100, 50)]
        [InlineData(0, 100, 0, 50, 0)]
        [InlineData(0, 0, 0, 50 , 50)]
        public async Task IncreaseBalanceAsync_AllOperationSucceeded_ReturnSuccess
            (long total, long reservedForTransfers, long reservedForSeize, long updatedTotal, long expectedTotal)
        {
            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerWalletAddressResultModel.Succeeded(FakeCustomerWalletAddress));

            _quorumOperationExecutorClientMock
                .SetupSequence(x => x.AddressesApi.GetBalanceForAddressAsync(It.IsAny<string>()))
                .ReturnsAsync(new AddressBalanceResponse { Balance = total })
                .ReturnsAsync(new AddressBalanceResponse { Balance = updatedTotal });

            _operationsFetcherMock
                .Setup(x => x.GetTransfersInProgressAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<IOperation>
                {
                    new OperationEntity
                    {
                        Type = OperationType.TokensTransfer,
                        ContextJson = new TokensTransferContext {Amount = reservedForTransfers}.ToJson()
                    }
                });

            _operationsFetcherMock
                .Setup(x => x.GetSeizeOperationsInProgressAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<IOperation>
                {
                    new OperationEntity
                    {
                        Type = OperationType.SeizeToInternal,
                        ContextJson = new SeizeToInternalContext {Amount = reservedForSeize}.ToJson()
                    }
                });

            _distributedCacheMock
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]) null);

            _distributedCacheMock
                .Setup(x => x.SetAsync(It.IsAny<string>(), 
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSutInstance();

            await sut.GetAsync(FakeCustomerId);
            var result = await sut.ForceBalanceUpdateAsync(
                FakeCustomerId,
                OperationType.CustomerBonusReward,
                Guid.NewGuid());

            Assert.Equal(CustomerBalanceError.None, result.Error);
            Assert.Equal(expectedTotal, result.Total);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task ForceBalanceUpdateAsync_InvalidCustomerId_ReturnsFail(string customerId)
        {
            var sut = CreateSutInstance();
            
            var result = await sut.ForceBalanceUpdateAsync(customerId, OperationType.CustomerBonusReward, Guid.NewGuid());
            
            Assert.Equal(CustomerBalanceError.InvalidCustomerId, result.Error);
            Assert.Equal(0, result.Total);
        }

        [Theory]
        [InlineData(OperationType.CustomerWalletCreation)]
        public async Task ForceBalanceUpdateAsync_InvalidOperationType_ThrowsException(OperationType operationType)
        {
            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.ForceBalanceUpdateAsync(FakeCustomerId, operationType, Guid.NewGuid()));
        }
        
        [Fact]
        public async Task ForceBalanceUpdateAsync_UpdateSucceeded_PublisherGetsCalled()
        {
            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerWalletAddressResultModel.Succeeded(FakeCustomerWalletAddress));

            _quorumOperationExecutorClientMock
                .Setup(x => x.AddressesApi.GetBalanceForAddressAsync(It.IsAny<string>()))
                .ReturnsAsync(new AddressBalanceResponse {Balance = ValidBalance});

            _operationsFetcherMock
                .Setup(x => x.GetTransfersInProgressAsync(It.IsAny<string>()))
                .ReturnsAsync(Enumerable.Empty<IOperation>().ToList());

            _operationsFetcherMock
                .Setup(x => x.GetSeizeOperationsInProgressAsync(It.IsAny<string>()))
                .ReturnsAsync(Enumerable.Empty<IOperation>().ToList());

            _customerBalanceUpdatePublisherMock
                .Setup(x => x.PublishAsync(It.IsAny<CustomerBalanceUpdatedEvent>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _distributedCacheMock
                .Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            _distributedCacheMock
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]) null);

            _distributedCacheMock
                .Setup(x => x.SetAsync(It.IsAny<string>(), 
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            var sut = CreateSutInstance();

            await sut.ForceBalanceUpdateAsync(
                FakeCustomerId,
                OperationType.CustomerBonusReward, Guid.NewGuid());

            _customerBalanceUpdatePublisherMock.Verify(
                x => x.PublishAsync(It.IsAny<CustomerBalanceUpdatedEvent>()),
                Times.Once());
        }

        private IBalanceService CreateSutInstance()
        {
            return new BalanceService(
                _quorumOperationExecutorClientMock.Object,
                _walletsServiceMock.Object,
                TimeSpan.FromMinutes(1),
                _operationsFetcherMock.Object,
                _customerBalanceUpdatePublisherMock.Object,
                EmptyLogFactory.Instance,
                _distributedCacheMock.Object);
        }
    }
}
