using System;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities;
using Lykke.Service.PrivateBlockchainFacade.DomainServices.Common;
using Lykke.Service.PrivateBlockchainFacade.DomainServices.Features.Wallets;
using Lykke.Service.QuorumTransactionSigner.Client;
using Lykke.Service.QuorumTransactionSigner.Client.Models.Responses;
using Moq;
using Xunit;

namespace Lykke.Service.PrivateBlockchainFacade.Tests
{
    public class WalletsServiceTests
    {
        private readonly Mock<IWalletOwnersRepository>
            _walletOwnersRepositoryMock = new Mock<IWalletOwnersRepository>();
        
        private readonly Mock<IQuorumTransactionSignerClient> _quorumTransactionSignerClientMock = new Mock<IQuorumTransactionSignerClient>();
        
        private readonly Mock<IOperationRequestsProducer> _operationsProducerMock = new Mock<IOperationRequestsProducer>();
        
        private const string FakeCustomerId = "3f1443e2-b848-4567-8fb5-ebe7337a87e9";

        private const string FakeWalletAddress = "0x7d275eb17ceaae04b17768d4459741bae062ee09";

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task CreateCustomerWallet_InvalidCustomerId_Returns_InvalidCustomerId(string customerId)
        {
            var sut = CreateSutInstance();

            var result = await sut.CreateCustomerWalletAsync(customerId);

            Assert.Equal(CustomerWalletCreationError.InvalidCustomerId, result.Error);
        }

        [Fact]
        public async Task CreateCustomerWallet_WalletAlreadyAssigned_Returns_AlreadyAssigned()
        {
            // emulate case when wallet is already assigned to the customer
            _walletOwnersRepositoryMock
                .Setup(x => x.GetByOwnerIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new WalletOwnerEntity {OwnerId = string.Empty, WalletId = string.Empty});

            var sut = CreateSutInstance();

            var result = await sut.CreateCustomerWalletAsync(FakeCustomerId);

            Assert.Equal(CustomerWalletCreationError.AlreadyCreated, result.Error);
        }

        [Fact]
        public async Task CreateCustomerWallet_AddWalletOwnerRaisesException_Returns_WalletAlreadyCreated()
        {
            _walletOwnersRepositoryMock
                .Setup(x => x.GetByOwnerIdAsync(It.IsAny<string>()))
                .ReturnsAsync((IWalletOwner) null);

            _walletOwnersRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<WalletOwnerDuplicateException>();
            
            _quorumTransactionSignerClientMock
                .Setup(x => x.WalletsApi.CreateWalletAsync())
                .ReturnsAsync(new CreateWalletResponse {Address = FakeWalletAddress});

            var sut = CreateSutInstance();

            var result = await sut.CreateCustomerWalletAsync(FakeCustomerId);

            Assert.Equal(CustomerWalletCreationError.AlreadyCreated, result.Error);
        }

        [Fact]
        public async Task CreateCustomerWallet_AddWalletOwnerRaisesNonHandledException_RaisesException()
        {
            _walletOwnersRepositoryMock
                .Setup(x => x.GetByOwnerIdAsync(It.IsAny<string>()))
                .ReturnsAsync((IWalletOwner) null);

            _walletOwnersRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<Exception>();
            
            _quorumTransactionSignerClientMock
                .Setup(x => x.WalletsApi.CreateWalletAsync())
                .ReturnsAsync(new CreateWalletResponse {Address = FakeWalletAddress});

            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<Exception>(() => sut.CreateCustomerWalletAsync(FakeCustomerId));
        }

        [Fact]
        public async Task CreateCustomerWallet_AddOperationRaisesNonHandledException_RaisesException()
        {
            _walletOwnersRepositoryMock
                .Setup(x => x.GetByOwnerIdAsync(It.IsAny<string>()))
                .ReturnsAsync((IWalletOwner) null);

            _walletOwnersRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new WalletOwnerEntity {OwnerId = string.Empty, WalletId = string.Empty});

            _operationsProducerMock
                .Setup(x => x.AddAsync(
                    It.IsAny<string>(), 
                    It.IsAny<OperationType>(), 
                    It.IsAny<Object>(), 
                    It.IsAny<string>()))
                .Throws<Exception>();
            
            _quorumTransactionSignerClientMock
                .Setup(x => x.WalletsApi.CreateWalletAsync())
                .ReturnsAsync(new CreateWalletResponse {Address = FakeWalletAddress});

            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<Exception>(() => sut.CreateCustomerWalletAsync(FakeCustomerId));
        }

        [Fact]
        public async Task CreateCustomerWallet_AllStepsSucceeded_Returns_Success()
        {
            _walletOwnersRepositoryMock
                .Setup(x => x.GetByOwnerIdAsync(It.IsAny<string>()))
                .ReturnsAsync((IWalletOwner) null);

            _walletOwnersRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new WalletOwnerEntity {OwnerId = string.Empty, WalletId = string.Empty});

            _operationsProducerMock
                .Setup(x => x.AddAsync(
                    It.IsAny<string>(), 
                    It.IsAny<OperationType>(), 
                    It.IsAny<Object>(), 
                    It.IsAny<string>()))
                .ReturnsAsync(Guid.Empty);

            _quorumTransactionSignerClientMock
                .Setup(x => x.WalletsApi.CreateWalletAsync())
                .ReturnsAsync(new CreateWalletResponse {Address = FakeWalletAddress});

            var sut = CreateSutInstance();
            
            var result = await sut.CreateCustomerWalletAsync(FakeCustomerId);
            
            Assert.Equal(CustomerWalletCreationError.None, result.Error);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task GetCustomerWalletAsync_InvalidInputParameters_ReturnsFail(string customerId)
        {
            var sut = CreateSutInstance();

            var result = await sut.GetCustomerWalletAsync(customerId);
            
            Assert.Equal(CustomerWalletAddressError.InvalidCustomerId, result.Error);
            Assert.True(string.IsNullOrEmpty(result.WalletAddress));
        }

        [Fact]
        public async Task GetCustomerWalletAsync_NoWalletRegistered_ReturnsFail()
        {
            _walletOwnersRepositoryMock
                .Setup(x => x.GetByOwnerIdAsync(It.IsAny<string>()))
                .ReturnsAsync((IWalletOwner) null);
            
            var sut = CreateSutInstance();

            var result = await sut.GetCustomerWalletAsync(FakeCustomerId);
            
            Assert.Equal(CustomerWalletAddressError.CustomerWalletMissing, result.Error);
            Assert.True(string.IsNullOrEmpty(result.WalletAddress));
        }

        [Fact]
        public async Task GetCustomerWalletAsync_WalletRegistered_ReturnsWalletAddress()
        {
            _walletOwnersRepositoryMock
                .Setup(x => x.GetByOwnerIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new WalletOwnerEntity {OwnerId = FakeCustomerId, WalletId = FakeWalletAddress});
            
            var sut = CreateSutInstance();

            var result = await sut.GetCustomerWalletAsync(FakeCustomerId);
            
            Assert.Equal(CustomerWalletAddressError.None, result.Error);
            Assert.Equal(FakeWalletAddress, result.WalletAddress);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task GetCustomerIdByWalletAsync_InvalidInputParameters_ReturnsFail(string walletAddress)
        {
            var sut = CreateSutInstance();

            var result = await sut.GetCustomerIdByWalletAsync(walletAddress);

            Assert.Equal(CustomerWalletAddressError.InvalidWalletAddress, result.Error);
            Assert.True(string.IsNullOrEmpty(result.CustomerId));
        }

        [Fact]
        public async Task GetCustomerIdByWalletAsync_NoWalletRegistered_ReturnsFail()
        {
            _walletOwnersRepositoryMock
                .Setup(x => x.GetByWalletAddressAsync(It.IsAny<string>()))
                .ReturnsAsync((IWalletOwner)null);

            var sut = CreateSutInstance();

            var result = await sut.GetCustomerIdByWalletAsync(FakeCustomerId);

            Assert.Equal(CustomerWalletAddressError.CustomerWalletMissing, result.Error);
            Assert.True(string.IsNullOrEmpty(result.CustomerId));
        }

        [Fact]
        public async Task GetCustomerIdByWalletAsync_ExistingWallet_ReturnsCustomerId()
        {
            _walletOwnersRepositoryMock
                .Setup(x => x.GetByWalletAddressAsync(It.IsAny<string>()))
                .ReturnsAsync(new WalletOwnerEntity { OwnerId = FakeCustomerId, WalletId = FakeWalletAddress });

            var sut = CreateSutInstance();

            var result = await sut.GetCustomerIdByWalletAsync(FakeWalletAddress);

            Assert.Equal(CustomerWalletAddressError.None, result.Error);
            Assert.Equal(FakeCustomerId, result.CustomerId);
        }

        private IWalletsService CreateSutInstance()
        {
            return new WalletsService(
                EmptyLogFactory.Instance, 
                new TransactionScopeHandler(EmptyLogFactory.Instance), 
                _walletOwnersRepositoryMock.Object,
                _quorumTransactionSignerClientMock.Object,
                _operationsProducerMock.Object
            );
        }
    }
}
