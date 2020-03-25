using System;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.Service.PrivateBlockchainFacade.Domain.Deduplication;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Bonuses;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using Lykke.Service.PrivateBlockchainFacade.DomainServices.Common;
using Lykke.Service.PrivateBlockchainFacade.DomainServices.Features.Bonuses;
using Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities.Deduplication;
using Moq;
using Xunit;

namespace Lykke.Service.PrivateBlockchainFacade.Tests
{
    public class BonusServiceTests
    {
        private readonly Mock<IDeduplicationLogRepository<BonusRewardDeduplicationLogEntity>> _deduplicationLogMock =
            new Mock<IDeduplicationLogRepository<BonusRewardDeduplicationLogEntity>>();

        private readonly Mock<IWalletsService> _walletsServiceMock = new Mock<IWalletsService>();

        private readonly Mock<IOperationRequestsProducer> _operationsProducerMock =
            new Mock<IOperationRequestsProducer>();

        private const string FakeCustomerId = "3f1443e2-b848-4567-8fb5-ebe7337a87e9";

        private const string FakeMasterWalletAddress = "c8cd7daa-864e-4e71-bded-15e92e852264";

        private const string FakeCustomerWalletAddress = "0x7d275eb17ceaae04b17768d4459741bae062ee09";

        [Theory]
        [InlineData("", 0, "bonus", "campaignId", "conditionId", BonusRewardError.InvalidCustomerId)]
        [InlineData(null, 0, "bonus", "campaignId", "conditionId", BonusRewardError.InvalidCustomerId)]
        [InlineData(FakeCustomerId, 0, "bonus", "campaignId", "conditionId", BonusRewardError.InvalidAmount)]
        [InlineData(FakeCustomerId, -1, "bonus", "campaignId", "conditionId", BonusRewardError.InvalidAmount)]
        [InlineData(FakeCustomerId, 1, null, "campaignId", "conditionId", BonusRewardError.MissingBonusReason)]
        [InlineData(FakeCustomerId, 10, "bonus", null, "conditionId", BonusRewardError.InvalidCampaignId)]
        public async Task RewardAsync_InvalidInputParameters_ReturnsFail(string customerId, long amount,
            string bonusReason, string campaignId, string conditionId, BonusRewardError expectedError)
        {
            var sut = CreateSutInstance();

            var result = await sut.RewardAsync(customerId, amount, default(string), bonusReason, campaignId,
                conditionId);

            Assert.Equal(expectedError, result.Error);
        }

        [Fact]
        public async Task RewardAsync_CustomerWallet_HasNotBeenRegisteredYet_ReturnsFail()
        {
            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(It.IsAny<string>()))
                .ReturnsAsync(
                    CustomerWalletAddressResultModel.Failed(CustomerWalletAddressError.CustomerWalletMissing));

            var sut = CreateSutInstance();

            var result = await sut.RewardAsync(FakeCustomerId, 1, default(string), "bonusReason", "campaignId",
                "conditionId");

            Assert.Equal(BonusRewardError.CustomerWalletMissing, result.Error);
        }

        [Fact]
        public async Task RewardAsync_BonusHasAlreadyBeenAdded_ReturnsFail()
        {
            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerWalletAddressResultModel.Succeeded(FakeCustomerWalletAddress));

            _deduplicationLogMock
                .Setup(x => x.IsDuplicateAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var sut = CreateSutInstance();

            var result = await sut.RewardAsync(FakeCustomerId, 1, default(string), "bonusReason", "campaignId",
                "conditionId");

            Assert.Equal(BonusRewardError.DuplicateRequest, result.Error);
        }

        [Fact]
        public async Task RewardAsync_AllOperationsSucceeded_ReturnsSuccess()
        {
            _walletsServiceMock
                .Setup(x => x.GetCustomerWalletAsync(It.IsAny<string>()))
                .ReturnsAsync(CustomerWalletAddressResultModel.Succeeded(FakeCustomerWalletAddress));

            _deduplicationLogMock
                .Setup(x => x.IsDuplicateAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _operationsProducerMock
                .Setup(x => x.AddAsync(
                    It.IsAny<string>(),
                    It.IsAny<OperationType>(),
                    It.IsAny<object>(),
                    It.IsAny<string>()))
                .ReturnsAsync(Guid.Empty);

            var sut = CreateSutInstance();

            var result = await sut.RewardAsync(FakeCustomerId, 1, default(string), "bonusReason", "campaignId",
                "conditionId");

            Assert.Equal(BonusRewardError.None, result.Error);
        }

        private IBonusService CreateSutInstance()
        {
            return new BonusService(
                _deduplicationLogMock.Object,
                new TransactionScopeHandler(EmptyLogFactory.Instance),
                EmptyLogFactory.Instance,
                _operationsProducerMock.Object,
                _walletsServiceMock.Object,
                FakeMasterWalletAddress);
        }
    }
}
