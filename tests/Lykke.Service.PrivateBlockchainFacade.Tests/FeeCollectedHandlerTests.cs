using System;
using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.PrivateBlockchainFacade.Contract.Events;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using Lykke.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers;
using Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities;
using Moq;
using Xunit;

namespace Lykke.Service.PrivateBlockchainFacade.Tests
{
    public class FeeCollectedHandlerTests
    {
        private const string FakeEventId = "evtId";
        private const string FakeWalletAddress = "address";
        private const string FakeCustomerId = "custID";
        private const string TransferToPublicFeeReason = "Fee for transfer to Ethereum network";
        private const string WalletLinkingFeeReason = "Fee for Ethereum account linking";
        private const long FakeAmount = 1;
        private const string FakeTxHash = "hash";

        private readonly Mock<IWalletOwnersRepository> _walletOwnersRepoMock = new Mock<IWalletOwnersRepository>();
        private readonly Mock<IRabbitPublisher<FeeCollectedEvent>> _publisherMock = new Mock<IRabbitPublisher<FeeCollectedEvent>>();

        [Theory]
        [InlineData(null, FakeWalletAddress, WalletLinkingFeeReason, FakeAmount)]
        [InlineData(FakeEventId, null, WalletLinkingFeeReason, FakeAmount)]
        [InlineData(FakeEventId, FakeWalletAddress, null, FakeAmount)]
        [InlineData(FakeEventId, FakeWalletAddress, WalletLinkingFeeReason, -1)]
        public async Task HandleAsync_InvalidInputParameters_PublisherNotCalled(string eventId, string walletAddress, string reason, long amount)
        {
            var sut = CreateSutInstance();

            await sut.HandleAsync(eventId, walletAddress, reason, amount, FakeTxHash);

            _publisherMock.Verify(x => x.PublishAsync(It.IsAny<FeeCollectedEvent>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WalletOwnerMissing_PublisherNotCalled()
        {
            _walletOwnersRepoMock.Setup(x => x.GetByWalletAddressAsync(FakeWalletAddress))
                .ReturnsAsync((IWalletOwner) null);

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeEventId, FakeWalletAddress, WalletLinkingFeeReason, FakeAmount, FakeTxHash);

            _publisherMock.Verify(x => x.PublishAsync(It.IsAny<FeeCollectedEvent>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_InvalidReason_ArgumentOutOfRangeExceptionThrown()
        {
            _walletOwnersRepoMock.Setup(x => x.GetByWalletAddressAsync(FakeWalletAddress))
                .ReturnsAsync(new WalletOwnerEntity { OwnerId = FakeCustomerId });

            var sut = CreateSutInstance();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                sut.HandleAsync(FakeEventId, FakeWalletAddress, "reason", FakeAmount, FakeTxHash));
        }

        [Theory]
        [InlineData(TransferToPublicFeeReason, FeeCollectedReason.TransferToPublic)]
        [InlineData(WalletLinkingFeeReason, FeeCollectedReason.WalletLinking)]
        public async Task HandleAsync_PublisherCalledWithCorrectReason(string reason, FeeCollectedReason expectedFeeCollectedReason)
        {
            _walletOwnersRepoMock.Setup(x => x.GetByWalletAddressAsync(FakeWalletAddress))
                .ReturnsAsync(new WalletOwnerEntity{OwnerId = FakeCustomerId});

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeEventId, FakeWalletAddress, reason, FakeAmount, FakeTxHash);

            _publisherMock.Verify(
                x => x.PublishAsync(It.Is<FeeCollectedEvent>(o => o.Reason == expectedFeeCollectedReason)), Times.Once);
        }

        private FeeCollectedHandler CreateSutInstance()
        {
            return new FeeCollectedHandler(_walletOwnersRepoMock.Object, _publisherMock.Object, EmptyLogFactory.Instance);
        }
    }
}
