using System.Threading.Tasks;
using Lykke.Logs;
using MAVN.Service.PrivateBlockchainFacade.Domain.Deduplication;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Balances;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Transfers;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.Common;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.Features.Transfers;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities.Deduplication;
using Moq;
using Xunit;

namespace MAVN.Service.PrivateBlockchainFacade.Tests
{
    public class TransferServiceTests
    {
        private readonly Mock<IWalletsService> _walletsServiceMock = new Mock<IWalletsService>();

        private readonly Mock<IBalanceService> _balanceServiceMock = new Mock<IBalanceService>();

        private readonly Mock<IOperationRequestsProducer> _operationsProducerMock = new Mock<IOperationRequestsProducer>();

        private readonly Mock<IDeduplicationLogRepository<TransferDeduplicationLogEntity>> _deduplicationLogMock = new Mock<IDeduplicationLogRepository<TransferDeduplicationLogEntity>>();

        private const string FakeSenderCustomerId = "3f1443e2-b848-4567-8fb5-ebe7337a87e9";

        private const string FakeRecipientCustomerId = "60d4f58d-7fde-4640-8d0e-feb97b12a90e";

        private const string FakeTransferId = "47dc1b1b-d38a-468b-89f2-a97050e2e8b7";

        private const string FakeWalletAddress = "0x7d275eb17ceaae04b17768d4459741bae062ee09";

        private const long FakeStakedAmount = 100;

        [Theory]
        [InlineData("", FakeRecipientCustomerId, 1, TransferError.InvalidSenderId)]
        [InlineData(null, FakeRecipientCustomerId, 1, TransferError.InvalidSenderId)]
        [InlineData(FakeSenderCustomerId, "", 1, TransferError.InvalidRecipientId)]
        [InlineData(FakeSenderCustomerId, null, 1, TransferError.InvalidRecipientId)]
        [InlineData(FakeSenderCustomerId, FakeRecipientCustomerId, 0, TransferError.InvalidAmount)]
        [InlineData(FakeSenderCustomerId, FakeRecipientCustomerId, -1, TransferError.InvalidAmount)]
        public async Task TransferAsync_InvalidInputParameters_ReturnsFail(
            string senderId,
            string recipientId,
            long amount,
            TransferError expectedError)
        {
            var sut = CreateSutInstance();

            var result = await sut.P2PTransferAsync(senderId, recipientId, amount, FakeTransferId);

            Assert.Equal(expectedError, result.Error);
        }

        [Fact]
        public async Task TransferAsync_SenderWalletNotAssigned_ReturnsFail()
        {
            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(FakeSenderCustomerId))
                .ReturnsAsync(CustomerWalletAddressResultModel.Failed(CustomerWalletAddressError.CustomerWalletMissing));

            var sut = CreateSutInstance();

            var result = await sut.P2PTransferAsync(FakeSenderCustomerId, FakeRecipientCustomerId, 1, FakeTransferId);

            Assert.Equal(TransferError.SenderWalletMissing, result.Error);
        }

        [Fact]
        public async Task TransferAsync_RecipientWalletNotAssigned_ReturnsFail()
        {
            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(FakeSenderCustomerId))
                .ReturnsAsync(CustomerWalletAddressResultModel.Succeeded(FakeWalletAddress));

            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(FakeRecipientCustomerId))
                .ReturnsAsync(CustomerWalletAddressResultModel.Failed(CustomerWalletAddressError.CustomerWalletMissing));

            var sut = CreateSutInstance();

            var result = await sut.P2PTransferAsync(FakeSenderCustomerId, FakeRecipientCustomerId, 1, FakeTransferId);

            Assert.Equal(TransferError.RecipientWalletMissing, result.Error);
        }

        [Theory]
        [InlineData(CustomerBalanceError.InvalidCustomerId, TransferError.InvalidSenderId)]
        [InlineData(CustomerBalanceError.CustomerWalletMissing, TransferError.SenderWalletMissing)]
        public async Task TransferAsync_BalanceServiceReturnsError_ReturnsFail(CustomerBalanceError balanceError, TransferError expectedError)
        {
            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerWalletAddressResultModel.Succeeded(FakeWalletAddress));

            _balanceServiceMock
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerBalanceResultModel.Failed(balanceError));

            var sut = CreateSutInstance();

            var result = await sut.P2PTransferAsync(FakeSenderCustomerId, FakeRecipientCustomerId, 1, FakeTransferId);

            Assert.Equal(expectedError, result.Error);
        }

        [Theory]
        [InlineData(100, -100)]
        [InlineData(100, 0)]
        [InlineData(100, 99)]
        public async Task TransferAsync_SenderWalletNotEnoughFunds_ReturnsFail(long transferAmount, long balanceAmount)
        {
            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerWalletAddressResultModel.Succeeded(FakeWalletAddress));

            _balanceServiceMock
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerBalanceResultModel.Succeeded(balanceAmount, FakeStakedAmount));

            var sut = CreateSutInstance();

            var result = await sut.P2PTransferAsync(FakeSenderCustomerId, FakeRecipientCustomerId, transferAmount, FakeTransferId);

            Assert.Equal(TransferError.NotEnoughFunds, result.Error);
        }

        [Theory]
        [InlineData(true, TransferError.DuplicateRequest)]
        [InlineData(false, TransferError.None)]
        public async Task TransferAsync_IsDuplicateCheck_WorksCorrectly(bool isDuplicate, TransferError expectedError)
        {
            const long amount = 100;

            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerWalletAddressResultModel.Succeeded(FakeWalletAddress));

            _balanceServiceMock
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerBalanceResultModel.Succeeded(amount, FakeStakedAmount));

            _deduplicationLogMock
                .Setup(x => x.IsDuplicateAsync(It.IsAny<string>()))
                .ReturnsAsync(isDuplicate);

            var sut = CreateSutInstance();

            var result = await sut.P2PTransferAsync(FakeSenderCustomerId, FakeRecipientCustomerId, amount, FakeTransferId);

            Assert.Equal(expectedError, result.Error);
        }


        [Theory]
        [InlineData("", FakeRecipientCustomerId, 1, null, TransferError.InvalidSenderId)]
        [InlineData(null, FakeRecipientCustomerId, 1, null, TransferError.InvalidSenderId)]
        [InlineData(FakeSenderCustomerId, "", 1, null, TransferError.RecipientWalletMissing)]
        [InlineData(FakeSenderCustomerId, null, 1, null, TransferError.RecipientWalletMissing)]
        [InlineData(FakeSenderCustomerId, FakeRecipientCustomerId, 0, null, TransferError.InvalidAmount)]
        [InlineData(FakeSenderCustomerId, FakeRecipientCustomerId, -1, null, TransferError.InvalidAmount)]
        [InlineData(FakeSenderCustomerId, FakeRecipientCustomerId, 1, "asd", TransferError.InvalidAdditionalDataFormat)]
        public async Task GenericTransferAsync_InvalidInputParameters_ReturnsFail(
            string senderId,
            string recipientWalletAddress,
            long amount,
            string additionalData,
            TransferError expectedError)
        {
            var sut = CreateSutInstance();

            var result = await sut.GenericTransferAsync(senderId, recipientWalletAddress, amount, FakeTransferId, additionalData);

            Assert.Equal(expectedError, result.Error);
        }

        [Fact]
        public async Task GenericTransferAsync_SenderWalletNotAssigned_ReturnsFail()
        {
            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(FakeSenderCustomerId))
                .ReturnsAsync(CustomerWalletAddressResultModel.Failed(CustomerWalletAddressError.CustomerWalletMissing));

            var sut = CreateSutInstance();

            var result = await sut.GenericTransferAsync(FakeSenderCustomerId, FakeWalletAddress, 1, FakeTransferId, null);

            Assert.Equal(TransferError.SenderWalletMissing, result.Error);
        }

        [Theory]
        [InlineData(CustomerBalanceError.InvalidCustomerId, TransferError.InvalidSenderId)]
        [InlineData(CustomerBalanceError.CustomerWalletMissing, TransferError.SenderWalletMissing)]
        public async Task GenericTransferAsync_BalanceServiceReturnsError_ReturnsFail(CustomerBalanceError balanceError, TransferError expectedError)
        {
            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerWalletAddressResultModel.Succeeded(FakeWalletAddress));

            _balanceServiceMock
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerBalanceResultModel.Failed(balanceError));

            var sut = CreateSutInstance();

            var result = await sut.GenericTransferAsync(FakeSenderCustomerId, FakeWalletAddress, 1, FakeTransferId, null);

            Assert.Equal(expectedError, result.Error);
        }

        [Theory]
        [InlineData(100, -100)]
        [InlineData(100, 0)]
        [InlineData(100, 99)]
        public async Task GenericTransferAsync_SenderWalletNotEnoughFunds_ReturnsFail(long transferAmount, long balanceAmount)
        {
            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerWalletAddressResultModel.Succeeded(FakeWalletAddress));

            _balanceServiceMock
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerBalanceResultModel.Succeeded(balanceAmount, FakeStakedAmount));

            var sut = CreateSutInstance();

            var result = await sut.GenericTransferAsync(FakeSenderCustomerId, FakeWalletAddress, transferAmount, FakeTransferId, null);

            Assert.Equal(TransferError.NotEnoughFunds, result.Error);
        }

        [Theory]
        [InlineData(true, TransferError.DuplicateRequest)]
        [InlineData(false, TransferError.None)]
        public async Task GenericTransferAsync_IsDuplicateCheck_WorksCorrectly(bool isDuplicate, TransferError expectedError)
        {
            const long amount = 100;

            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerWalletAddressResultModel.Succeeded(FakeWalletAddress));

            _balanceServiceMock
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerBalanceResultModel.Succeeded(amount, FakeStakedAmount));

            _deduplicationLogMock
                .Setup(x => x.IsDuplicateAsync(It.IsAny<string>()))
                .ReturnsAsync(isDuplicate);

            var sut = CreateSutInstance();

            var result = await sut.GenericTransferAsync(FakeSenderCustomerId, FakeWalletAddress, amount, FakeTransferId, null);

            Assert.Equal(expectedError, result.Error);
        }


        [Theory]
        [InlineData("", FakeRecipientCustomerId, 1, 0, TransferError.InvalidSenderId)]
        [InlineData(null, FakeRecipientCustomerId, 1, 0, TransferError.InvalidSenderId)]
        [InlineData(FakeSenderCustomerId, "", 1, 0, TransferError.RecipientWalletMissing)]
        [InlineData(FakeSenderCustomerId, null, 1, 0, TransferError.RecipientWalletMissing)]
        [InlineData(FakeSenderCustomerId, FakeRecipientCustomerId, 0, 0, TransferError.InvalidAmount)]
        [InlineData(FakeSenderCustomerId, FakeRecipientCustomerId, -1, 0, TransferError.InvalidAmount)]
        [InlineData(FakeSenderCustomerId, FakeRecipientCustomerId, 1, -1, TransferError.InvalidFeeAmount)]
        public async Task TransferToExternalAsync_InvalidInputParameters_ReturnsFail(
            string senderId,
            string recipientWalletAddress,
            long amount,
            long fee,
            TransferError expectedError)
        {
            var sut = CreateSutInstance();

            var result = await sut.TransferToExternalAsync(senderId, recipientWalletAddress, amount, fee, FakeTransferId);

            Assert.Equal(expectedError, result.Error);
        }

        [Fact]
        public async Task TransferToExternalAsync_SenderWalletNotAssigned_ReturnsFail()
        {
            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(FakeSenderCustomerId))
                .ReturnsAsync(CustomerWalletAddressResultModel.Failed(CustomerWalletAddressError.CustomerWalletMissing));

            var sut = CreateSutInstance();

            var result = await sut.TransferToExternalAsync(FakeSenderCustomerId, FakeWalletAddress, 1, 0, FakeTransferId);

            Assert.Equal(TransferError.SenderWalletMissing, result.Error);
        }

        [Theory]
        [InlineData(CustomerBalanceError.InvalidCustomerId, TransferError.InvalidSenderId)]
        [InlineData(CustomerBalanceError.CustomerWalletMissing, TransferError.SenderWalletMissing)]
        public async Task TransferToExternalAsync_BalanceServiceReturnsError_ReturnsFail(CustomerBalanceError balanceError, TransferError expectedError)
        {
            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerWalletAddressResultModel.Succeeded(FakeWalletAddress));

            _balanceServiceMock
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerBalanceResultModel.Failed(balanceError));

            var sut = CreateSutInstance();

            var result = await sut.TransferToExternalAsync(FakeSenderCustomerId, FakeWalletAddress, 1, 0, FakeTransferId);

            Assert.Equal(expectedError, result.Error);
        }

        [Theory]
        [InlineData(100, 0, -100)]
        [InlineData(100, 1, 0)]
        [InlineData(100,0, 99)]
        [InlineData(99, 1, 99)]
        public async Task TransferToExternalAsync_SenderWalletNotEnoughFunds_ReturnsFail(long transferAmount, long fee, long balanceAmount)
        {
            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerWalletAddressResultModel.Succeeded(FakeWalletAddress));

            _balanceServiceMock
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerBalanceResultModel.Succeeded(balanceAmount, FakeStakedAmount));

            var sut = CreateSutInstance();

            var result = await sut.TransferToExternalAsync(FakeSenderCustomerId, FakeWalletAddress, transferAmount, fee, FakeTransferId);

            Assert.Equal(TransferError.NotEnoughFunds, result.Error);
        }

        [Theory]
        [InlineData(true, TransferError.DuplicateRequest)]
        [InlineData(false, TransferError.None)]
        public async Task TransferToExternalAsync_IsDuplicateCheck_WorksCorrectly(bool isDuplicate, TransferError expectedError)
        {
            const long amount = 100;

            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerWalletAddressResultModel.Succeeded(FakeWalletAddress));

            _balanceServiceMock
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerBalanceResultModel.Succeeded(amount, FakeStakedAmount));

            _deduplicationLogMock
                .Setup(x => x.IsDuplicateAsync(It.IsAny<string>()))
                .ReturnsAsync(isDuplicate);

            var sut = CreateSutInstance();

            var result = await sut.TransferToExternalAsync(FakeSenderCustomerId, FakeWalletAddress, amount, 0, FakeTransferId);

            Assert.Equal(expectedError, result.Error);
        }

        [Theory]
        [InlineData("", FakeWalletAddress, 1, TransferError.PrivateWalletMissing)]
        [InlineData(null, FakeWalletAddress, 1, TransferError.PrivateWalletMissing)]
        [InlineData(FakeWalletAddress, "", 1, TransferError.PublicWalletMissing)]
        [InlineData(FakeWalletAddress, null, 1, TransferError.PublicWalletMissing)]
        [InlineData(FakeWalletAddress, FakeRecipientCustomerId, 0, TransferError.InvalidAmount)]
        public async Task TransferToInternalAsync_InvalidInputParameters_ReturnsFail(
            string privateWalletAddress,
            string publicWalletAddress,
            long amount,
            TransferError expectedError)
        {
            var sut = CreateSutInstance();

            var result = await sut.TransferToInternalAsync(privateWalletAddress, publicWalletAddress, amount, 123);

            Assert.Equal(expectedError, result.Error);
        }

        [Theory]
        [InlineData(true, TransferError.DuplicateRequest)]
        [InlineData(false, TransferError.None)]
        public async Task TransferToInternalAsync_IsDuplicateCheck_WorksCorrectly(bool isDuplicate, TransferError expectedError)
        {
            const long amount = 100;

            _deduplicationLogMock
                .Setup(x => x.IsDuplicateAsync(It.IsAny<string>()))
                .ReturnsAsync(isDuplicate);

            var sut = CreateSutInstance();
            var result = await sut.TransferToInternalAsync(FakeWalletAddress, FakeRecipientCustomerId, amount, 123);

            Assert.Equal(expectedError, result.Error);
        }


        private ITransferService CreateSutInstance()
        {
            return new TransferService(
                _walletsServiceMock.Object,
                new TransactionScopeHandler(EmptyLogFactory.Instance),
                _balanceServiceMock.Object,
                _operationsProducerMock.Object,
                _deduplicationLogMock.Object,
                EmptyLogFactory.Instance);
        }
    }
}
