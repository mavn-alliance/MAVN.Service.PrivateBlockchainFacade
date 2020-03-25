using System;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.Service.PrivateBlockchainFacade.Domain.Deduplication;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities;
using Lykke.Service.PrivateBlockchainFacade.DomainServices.Common;
using Lykke.Service.PrivateBlockchainFacade.DomainServices.Features.Operations;
using Lykke.Service.QuorumOperationExecutor.Client;
using Lykke.Service.QuorumOperationExecutor.Client.Models.Responses;
using Moq;
using Xunit;

namespace Lykke.Service.PrivateBlockchainFacade.Tests
{
    public class OperationStatusUpdaterTests
    {
        private readonly Mock<IOperationsRepository> _operationsRepositoryMock = new Mock<IOperationsRepository>();
        
        private readonly Mock<IOperationRequestsRepository> _operationRequestsRepositoryMock = new Mock<IOperationRequestsRepository>();

        private readonly Mock<IQuorumOperationExecutorClient> _executorClientMock =
            new Mock<IQuorumOperationExecutorClient>();

        private const string ValidTransactionHash = "0x09273094bf95663c9cef1b794816bc7bc530bb736311140458dc588baa26092a";
        
        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000")]
        public async Task AcceptAsync_InvalidId_Returns_Fail(Guid id)
        {
            var sut = CreateSutInstance();

            var result = await sut.AcceptAsync(id, ValidTransactionHash);
            
            Assert.Equal(OperationStatusUpdateError.OperationNotFound, result.Error);
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task AcceptAsync_InvalidHash_Returns_Fail(string hash)
        {
            var sut = CreateSutInstance();

            var result = await sut.AcceptAsync(Guid.NewGuid(), hash);
            
            Assert.Equal(OperationStatusUpdateError.InvalidTransactionHash, result.Error);
        }

        [Fact]
        public async Task AcceptAsync_OperationDoesNotExist_Returns_OperationNotFound()
        {
            _operationsRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((IOperation) null);

            var sut = CreateSutInstance();

            var result = await sut.AcceptAsync(Guid.NewGuid(), ValidTransactionHash);
            
            Assert.Equal(OperationStatusUpdateError.OperationNotFound, result.Error);
        }

        [Fact]
        public async Task AcceptAsync_OperationAlreadyAccepted_Returns_Succeeded()
        {
            _operationRequestsRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((IOperationRequest) null);
            
            _operationsRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new OperationEntity {Status = OperationStatus.Accepted});

            var sut = CreateSutInstance();

            var result = await sut.AcceptAsync(Guid.NewGuid(), ValidTransactionHash);
            
            Assert.Equal(OperationStatusUpdateError.None, result.Error);
        }

        [Theory]
        [InlineData(OperationStatus.Succeeded)]
        [InlineData(OperationStatus.Failed)]
        public async Task AcceptAsync_OperationAlreadyCompleted_Returns_Success(OperationStatus status)
        {
            _operationRequestsRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((IOperationRequest) null);
            
            _operationsRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new OperationEntity {Status = status});
            
            var sut = CreateSutInstance();

            var result = await sut.AcceptAsync(Guid.NewGuid(), ValidTransactionHash);
            
            Assert.Equal(OperationStatusUpdateError.None, result.Error);
        }

        [Fact]
        public async Task AcceptAsync_OperationCreated_CompletesSuccessfully()
        {
            _operationRequestsRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new OperationRequestEntity {Status = OperationStatus.Created});

            _operationsRepositoryMock
                .Setup(x => x.SetStatusAsync(It.IsAny<Guid>(), It.IsAny<OperationStatus>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var sut = CreateSutInstance();

            var result = await sut.AcceptAsync(Guid.NewGuid(), ValidTransactionHash);

            _operationRequestsRepositoryMock.Verify(
                m => m.AcceptAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<OperationType>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>()),
                Times.Once);

            Assert.Equal(OperationStatusUpdateError.None, result.Error);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task FailAsync_InvalidHash_Returns_Fail(string hash)
        {
            var sut = CreateSutInstance();

            var result = await sut.FailAsync(hash);
            
            Assert.Equal(OperationStatusUpdateError.OperationNotFound, result.Error);
        }

        [Fact]
        public async Task FailAsync_OperationDoesNotExist_Returns_OperationNotFound()
        {
            _operationsRepositoryMock
                .Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync((IOperation) null);

            var sut = CreateSutInstance();

            var result = await sut.FailAsync(ValidTransactionHash);
            
            Assert.Equal(OperationStatusUpdateError.OperationNotFound, result.Error);
        }
        
        [Fact]
        public async Task FailAsync_OperationAlreadyFailed_Returns_Succeeded()
        {
            _operationsRepositoryMock
                .Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(new OperationEntity {Status = OperationStatus.Failed});

            var sut = CreateSutInstance();

            var result = await sut.FailAsync(ValidTransactionHash);
            
            Assert.Equal(OperationStatusUpdateError.None, result.Error);
        }
        
        [Theory]
        [InlineData(OperationStatus.Succeeded)]
        [InlineData(OperationStatus.Created)]
        public async Task FailAsync_OperationStatus_Is_Invalid_Returns_Fail(OperationStatus status)
        {
            _operationsRepositoryMock
                .Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(new OperationEntity {Status = status});
            
            var sut = CreateSutInstance();

            var result = await sut.FailAsync(ValidTransactionHash);
            
            Assert.Equal(OperationStatusUpdateError.InvalidStatus, result.Error);
        }

        [Fact]
        public async Task FailAsync_OperationAccepted_CompletesSuccessfully()
        {
            _operationsRepositoryMock
                .Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(new OperationEntity {Status = OperationStatus.Accepted});

            _operationsRepositoryMock
                .Setup(x => x.SetStatusAsync(It.IsAny<Guid>(), It.IsAny<OperationStatus>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var sut = CreateSutInstance();

            var result = await sut.FailAsync(ValidTransactionHash);

            _operationsRepositoryMock.Verify(
                m => m.SetStatusAsync(It.IsAny<Guid>(), It.IsAny<OperationStatus>()),
                Times.Once);

            Assert.Equal(OperationStatusUpdateError.None, result.Error);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SucceedAsync_InvalidHash_Returns_Fail(string hash)
        {
            var sut = CreateSutInstance();

            var result = await sut.SucceedAsync(hash);
            
            Assert.Equal(OperationStatusUpdateError.OperationNotFound, result.Error);
        }
        
        [Fact]
        public async Task SucceedAsync_OperationDoesNotExist_Returns_OperationNotFound()
        {
            _operationsRepositoryMock
                .Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync((IOperation) null);

            var sut = CreateSutInstance();

            var result = await sut.SucceedAsync(ValidTransactionHash);
            
            Assert.Equal(OperationStatusUpdateError.OperationNotFound, result.Error);
        }
        
        [Fact]
        public async Task SucceedAsync_OperationAlreadySucceeded_Returns_Succeeded()
        {
            _operationsRepositoryMock
                .Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(new OperationEntity {Status = OperationStatus.Succeeded});

            var sut = CreateSutInstance();

            var result = await sut.SucceedAsync(ValidTransactionHash);
            
            Assert.Equal(OperationStatusUpdateError.None, result.Error);
        }
        
        [Theory]
        [InlineData(OperationStatus.Failed)]
        [InlineData(OperationStatus.Created)]
        public async Task SucceedAsync_OperationStatus_Is_Invalid_Returns_Fail(OperationStatus status)
        {
            _operationsRepositoryMock
                .Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(new OperationEntity {Status = status});
            
            var sut = CreateSutInstance();

            var result = await sut.SucceedAsync(ValidTransactionHash);
            
            Assert.Equal(OperationStatusUpdateError.InvalidStatus, result.Error);
        }

        [Fact]
        public async Task SucceedAsync_OperationAccepted_CompletesSuccessfully()
        {
            _operationsRepositoryMock
                .Setup(x => x.GetByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(new OperationEntity {Status = OperationStatus.Accepted});

            _operationsRepositoryMock
                .Setup(x => x.SetStatusAsync(It.IsAny<Guid>(), It.IsAny<OperationStatus>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var sut = CreateSutInstance();

            var result = await sut.SucceedAsync(ValidTransactionHash);

            _operationsRepositoryMock.Verify(
                m => m.SetStatusAsync(It.IsAny<Guid>(), It.IsAny<OperationStatus>()),
                Times.Once);

            Assert.Equal(OperationStatusUpdateError.None, result.Error);
        }

        [Fact]
        public async Task SyncWithBlockchain_OperationNotFoundByHash_ErrorIsReturned()
        {
            _executorClientMock.Setup(x => x.TransactionsApi.GetTransactionStateAsync(It.IsAny<string>()))
                .ReturnsAsync(new GetTransactionStateResponse {Error = GetTransactionStateError.TransactionNotFound});

            var sut = CreateSutInstance();

            var result = await sut.SyncWithBlockchainAsync("hash");

            Assert.Equal(OperationStatusUpdateError.OperationNotFound, result.Error);
        }

        [Fact]
        public async Task SyncWithBlockchain_ReturnedOperationHasNoId_ErrorIsReturned()
        {
            _executorClientMock.Setup(x => x.TransactionsApi.GetTransactionStateAsync(It.IsAny<string>()))
                .ReturnsAsync(new GetTransactionStateResponse { OperationId = null});

            var sut = CreateSutInstance();

            var result = await sut.SyncWithBlockchainAsync("hash");

            Assert.Equal(OperationStatusUpdateError.OperationIdIsNull, result.Error);
        }

        [Fact]
        public async Task SyncWithBlockchain_UnsupportedOperationStatus_ErrorIsReturned()
        {
            _executorClientMock.Setup(x => x.TransactionsApi.GetTransactionStateAsync(It.IsAny<string>()))
                .ReturnsAsync(new GetTransactionStateResponse
                {
                    TransactionState = TransactionState.Pending,
                    OperationId = Guid.NewGuid()
                });

            var sut = CreateSutInstance();

            var result = await sut.SyncWithBlockchainAsync("hash");

            Assert.Equal(OperationStatusUpdateError.UnsupportedOperationStatus, result.Error);
        }

        [Fact]
        public async Task SyncWithBlockchain_DuplicatedTransactionHash_ErrorIsReturned()
        {
            var transactionState = new GetTransactionStateResponse
            {
                TransactionState = TransactionState.Succeeded,
                OperationId = Guid.NewGuid(),
                TransactionHash = ValidTransactionHash
            };
            
            _executorClientMock.Setup(x => x.TransactionsApi.GetTransactionStateAsync(It.IsAny<string>()))
                .ReturnsAsync(transactionState);

            _operationRequestsRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((IOperationRequest) null);

            _operationsRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new OperationEntity
                {
                    Id = transactionState.OperationId.Value,
                    Status = OperationStatus.Accepted,
                    TransactionHash = transactionState.TransactionHash
                });

            _operationsRepositoryMock.Setup(x => x.SetStatusAsync
                    (It.IsAny<Guid>(), It.IsAny<OperationStatus>(), It.IsAny<string>()))
                .ThrowsAsync(new DuplicateException());

            var sut = CreateSutInstance();

            var result = await sut.SyncWithBlockchainAsync("hash");

            Assert.Equal(OperationStatusUpdateError.DuplicateTransactionHash, result.Error);
        }

        [Fact]
        public async Task SyncWithBlockchain_OperationSucceeds()
        {
            var transactionState = new GetTransactionStateResponse
            {
                TransactionState = TransactionState.Succeeded,
                OperationId = Guid.NewGuid(),
                TransactionHash = ValidTransactionHash
            };
            
            _executorClientMock.Setup(x => x.TransactionsApi.GetTransactionStateAsync(It.IsAny<string>()))
                .ReturnsAsync(transactionState);
            
            _operationRequestsRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((IOperationRequest) null);
            
            _operationsRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new OperationEntity
                {
                    Id = transactionState.OperationId.Value,
                    Status = OperationStatus.Accepted,
                    TransactionHash = transactionState.TransactionHash
                });

            _operationsRepositoryMock.Setup(x => x.SetStatusAsync
                    (It.IsAny<Guid>(), It.IsAny<OperationStatus>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSutInstance();

            var result = await sut.SyncWithBlockchainAsync(ValidTransactionHash);

            Assert.Equal(OperationStatusUpdateError.None, result.Error);
        }

        private IOperationStatusUpdater CreateSutInstance()
        {
            return new OperationStatusUpdater(
                EmptyLogFactory.Instance,
                new TransactionScopeHandler(EmptyLogFactory.Instance),
                _operationsRepositoryMock.Object,
                _executorClientMock.Object,
                _operationRequestsRepositoryMock.Object);
        }
    }
}
