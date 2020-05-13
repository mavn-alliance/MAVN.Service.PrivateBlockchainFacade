using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using MAVN.Common.MsSql;
using Lykke.Logs;
using MAVN.Service.PrivateBlockchainFacade.Contract.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.Common;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Contexts;
using MAVN.Service.QuorumOperationExecutor.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace MAVN.Service.PrivateBlockchainFacade.Tests
{
    public class OperationsFetcherTests
    {
        private IOperationsRepository _operationsRepository;

        private IOperationRequestsRepository _operationRequestsRepository;

        private IQuorumOperationExecutorClient _executorClient;

        private const int MaxNewOperationsAmount = 100;
        
        private const int MaxAcceptedOperationsAmount = 100;
        
        private const string FakeCustomerId = "3f1443e2-b848-4567-8fb5-ebe7337a87e9";
        
        private const string FakeWalletAddress = "0x7d275eb17ceaae04b17768d4459741bae062ee09";
        
        private static readonly IReadOnlyList<string> ValidTransactionHashes =
            new List<string>
            {
                // ReSharper disable StringLiteralTypo
                "0xc53f65d1664d5eac5d184f1f1456a8c331ca563fe73cdee2307f455ecb4eec5c",
                "0x9c5704ab9c3d4f41e08fe64537f0468ebcc3d0d41e424a32d3a5b301dce801d8",
                "0xbc0d286e83354f0cc5cfad66527a55721e740c646f1b24449615e3d217a74e11",
                "0x80546e104a88455f1e537d3038140bf91cbcf52136f03a4fac775563a7d5f48b",
                "0xb0e1179b2bec0a05cf2dd22c160b0af1574957a88a0dcb22286d11fed7bd98d7",
                "0x001152bd8404192132e073ff665ab486fa7b12d6d3ca5fd028cb2f2d2cbd6471"
                // ReSharper restore StringLiteralTypo
            };

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task GetTransferReservedAmountAsync_InvalidInputParameters_RaisesException(string walletAddress)
        {
            var sut = await CreateSutInstanceAsync();

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.GetTransfersInProgressAsync(walletAddress));
        }

        [Fact]
        public async Task GetTransfersInProgressAsync_FetchOnlyOperations_Related_To_MasterWalletAddress()
        {
            var sut = await CreateSutInstanceAsync(
                "read_transfers__master_wallet_address_condition_check__database", 
                (repo, requestsRepo) => SeedFor.GetTransfersInProgressAsync_WalletAddressConditionCheck(repo, requestsRepo, _executorClient, FakeWalletAddress));

            var transferOperations = await sut.GetTransfersInProgressAsync(FakeWalletAddress);
            
            Assert.All(transferOperations, x => Assert.Equal(FakeWalletAddress, x.MasterWalletAddress));
        }

        [Fact]
        public async Task GetTransfersInProgressAsync_FetchOnlyOperations_OfTokensTransferType()
        {
            var sut = await CreateSutInstanceAsync(
                "read_transfers__operation_type_condition_check__database", 
                (repo, requestsRepo) => SeedFor.GetTransfersInProgressAsync_OperationTypeConditionCheck(repo, requestsRepo, _executorClient));

            var transferOperations = await sut.GetTransfersInProgressAsync(FakeWalletAddress);
            
            Assert.All(transferOperations, x => Assert.Equal(OperationType.TokensTransfer, x.Type));
        }

        [Fact]
        public async Task GetTransfersInProgressAsync_FetchOnlyOperations_InAcceptedStatus()
        {
            var sut = await CreateSutInstanceAsync(
                "read_transfers__accepted_status_only_condition_check__database",
                (repo, requestsRepo) => SeedFor.GetTransfersInProgressAsync_OperationStatusConditionCheck(repo, requestsRepo, _executorClient));
            
            var transferOperations = await sut.GetTransfersInProgressAsync(FakeWalletAddress);
            
            Assert.All(transferOperations, x => Assert.Equal(OperationStatus.Accepted, x.Status));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task GetByHashAsync_InvalidInputParameters_RaisesException(string hash)
        {
            var sut = await CreateSutInstanceAsync();

            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.GetByHashAsync(hash));
        }

        private async Task<IOperationsFetcher> CreateSutInstanceAsync(
            string inMemoryDatabaseName = "operations_fetcher_tests", 
            Func<IOperationsRepository, IOperationRequestsRepository, Task> seedAction = null)
        {
            var options = new DbContextOptionsBuilder<PbfContext>()
                .UseInMemoryDatabase(inMemoryDatabaseName)
                .Options;

            var dbFactory = new MsSqlContextFactory<PbfContext>(opts => new PbfContext(options, true), options);

            _operationsRepository = new OperationsRepository(dbFactory);

            _operationRequestsRepository = new OperationRequestsRepository(
                dbFactory,
                new SqlRepositoryHelper(new MemoryCache(new MemoryCacheOptions()),  EmptyLogFactory.Instance));

            _executorClient = Mock.Of<IQuorumOperationExecutorClient>();

            if (seedAction != null)
            {
                await seedAction.Invoke(_operationsRepository, _operationRequestsRepository);
            }

            return new OperationsFetcher(
                MaxNewOperationsAmount,
                MaxAcceptedOperationsAmount,
                _operationsRepository,
                _operationRequestsRepository
            );
        }
        
        #region Seeding

        private static class SeedFor
        {
            public static async Task GetTransfersInProgressAsync_WalletAddressConditionCheck(
                IOperationsRepository repo,
                IOperationRequestsRepository requestsRepo,
                IQuorumOperationExecutorClient executorClient,
                string includeWalletAddress)
            {
                var rnd = new Random();
                
                var o = await repo.AddAsync(
                    Guid.NewGuid(),
                    FakeCustomerId,
                    1,
                    includeWalletAddress,
                    OperationType.TokensTransfer,
                    new TokensTransferContext
                    {
                        SenderWalletAddress = FakeWalletAddress, RecipientWalletAddress = FakeWalletAddress, Amount = rnd.Next()
                    }.ToJson(),
                    DateTime.UtcNow, 
                    ValidTransactionHashes[0]);

                await repo.AddAsync(
                    Guid.NewGuid(), 
                    FakeCustomerId,
                    2,
                    includeWalletAddress,
                    OperationType.TokensTransfer,
                    new TokensTransferContext
                    {
                        SenderWalletAddress = FakeWalletAddress, RecipientWalletAddress = FakeWalletAddress, Amount = rnd.Next()
                    }.ToJson(),
                    DateTime.UtcNow, 
                    string.Empty);

                await repo.AddAsync(
                    Guid.NewGuid(),
                    FakeCustomerId,
                    3,
                    $"{includeWalletAddress}-{rnd.Next()}",
                    OperationType.CustomerWalletCreation,
                    new CustomerWalletContext {CustomerId = FakeCustomerId, WalletAddress = includeWalletAddress}
                        .ToJson(),
                    DateTime.UtcNow,
                    string.Empty);
            }

            public static async Task GetTransfersInProgressAsync_OperationTypeConditionCheck(
                IOperationsRepository repo, 
                IOperationRequestsRepository requestsRepo, 
                IQuorumOperationExecutorClient executorClient)
            {
                var rnd = new Random();
                
                var statusUpdater = new OperationStatusUpdater(
                    EmptyLogFactory.Instance,
                    new TransactionScopeHandler(EmptyLogFactory.Instance), 
                    repo,
                    executorClient,
                    requestsRepo);

                var o1 = await repo.AddAsync(
                    Guid.NewGuid(), 
                    FakeCustomerId,
                    1,
                    FakeWalletAddress,
                    OperationType.TokensTransfer,
                    new TokensTransferContext
                    {
                        SenderWalletAddress = FakeWalletAddress, RecipientWalletAddress = FakeWalletAddress, Amount = rnd.Next()
                    }.ToJson(),
                    DateTime.UtcNow,
                    ValidTransactionHashes[0]);

                var o2 = await repo.AddAsync(
                    Guid.NewGuid(), 
                    FakeCustomerId,
                    2,
                    FakeWalletAddress,
                    OperationType.CustomerWalletCreation,
                    new CustomerWalletContext {CustomerId = FakeCustomerId, WalletAddress = FakeWalletAddress}.ToJson(),
                    DateTime.UtcNow, 
                    ValidTransactionHashes[1]);
                await statusUpdater.SucceedAsync(ValidTransactionHashes[1]);

                var o3 = await repo.AddAsync(
                    Guid.NewGuid(), 
                    FakeCustomerId,
                    3,
                    FakeWalletAddress,
                    OperationType.CustomerBonusReward,
                    new CustomerBonusRewardContext
                    {
                        CustomerId = FakeCustomerId,
                        WalletAddress = FakeWalletAddress,
                        Amount = rnd.Next(),
                        MinterAddress = FakeWalletAddress
                    }.ToJson(),
                    DateTime.UtcNow, 
                    ValidTransactionHashes[2]);
                await statusUpdater.SucceedAsync(ValidTransactionHashes[2]);
            }

            public static async Task GetTransfersInProgressAsync_OperationStatusConditionCheck(
                IOperationsRepository repo, 
                IOperationRequestsRepository requestsRepo, 
                IQuorumOperationExecutorClient executorClient)
            {
                var rnd = new Random();
                
                var statusUpdater = new OperationStatusUpdater(
                    EmptyLogFactory.Instance,
                    new TransactionScopeHandler(EmptyLogFactory.Instance), 
                    repo,
                    executorClient,
                    requestsRepo);

                var o1 = await repo.AddAsync(
                    Guid.NewGuid(),
                    FakeCustomerId,
                    1,
                    FakeWalletAddress,
                    OperationType.TokensTransfer,
                    new TokensTransferContext
                    {
                        SenderWalletAddress = FakeWalletAddress, RecipientWalletAddress = FakeWalletAddress, Amount = rnd.Next()
                    }.ToJson(),
                    DateTime.UtcNow,
                    ValidTransactionHashes[0]);
                
                var o2 = await repo.AddAsync(
                    Guid.NewGuid(), 
                    FakeCustomerId,
                    2,
                    FakeWalletAddress,
                    OperationType.TokensTransfer,
                    new TokensTransferContext
                    {
                        SenderWalletAddress = FakeWalletAddress, RecipientWalletAddress = FakeWalletAddress, Amount = rnd.Next()
                    }.ToJson(),
                    DateTime.UtcNow,
                    ValidTransactionHashes[1]);
                
                var o3 = await repo.AddAsync(
                    Guid.NewGuid(), 
                    FakeCustomerId,
                    3,
                    FakeWalletAddress,
                    OperationType.TokensTransfer,
                    new TokensTransferContext
                    {
                        SenderWalletAddress = FakeWalletAddress, RecipientWalletAddress = FakeWalletAddress, Amount = rnd.Next()
                    }.ToJson(),
                    DateTime.UtcNow,
                    ValidTransactionHashes[2]);
                await statusUpdater.SucceedAsync(ValidTransactionHashes[2]);
                
                var o4 = await repo.AddAsync(
                    Guid.NewGuid(),
                    FakeCustomerId,
                    4,
                    FakeWalletAddress,
                    OperationType.TokensTransfer,
                    new TokensTransferContext
                    {
                        SenderWalletAddress = FakeWalletAddress, RecipientWalletAddress = FakeWalletAddress, Amount = rnd.Next()
                    }.ToJson(),
                    DateTime.UtcNow,
                    ValidTransactionHashes[3]);
                await statusUpdater.FailAsync(ValidTransactionHashes[3]);
                
                var o5 = await repo.AddAsync(
                    Guid.NewGuid(), 
                    FakeCustomerId,
                    5,
                    FakeWalletAddress,
                    OperationType.CustomerWalletCreation,
                    new CustomerWalletContext {CustomerId = FakeCustomerId, WalletAddress = FakeWalletAddress}.ToJson(),
                    DateTime.UtcNow,
                    ValidTransactionHashes[4]);
                await statusUpdater.AcceptAsync(o5, ValidTransactionHashes[4]);
                
                var o6 = await repo.AddAsync(
                    Guid.NewGuid(), 
                    FakeCustomerId,
                    6,
                    FakeWalletAddress,
                    OperationType.CustomerWalletCreation,
                    new CustomerWalletContext {CustomerId = FakeCustomerId, WalletAddress = FakeWalletAddress}.ToJson(),
                    DateTime.UtcNow,
                    ValidTransactionHashes[5]);
            }
        }
        
        #endregion
    }
}
