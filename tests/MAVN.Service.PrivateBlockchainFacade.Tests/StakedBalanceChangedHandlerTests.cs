using System;
using System.Threading.Tasks;
using Lykke.Logs;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Balances;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities;
using Moq;
using Xunit;

namespace MAVN.Service.PrivateBlockchainFacade.Tests
{
    public class StakedBalanceChangedHandlerTests
    {
        private const string FakeCustomerId = "id";
        private const string FakeWalletAddress = "address";

        private readonly Mock<IBalanceService> _balanceServiceMock = new Mock<IBalanceService>();
        private readonly Mock<IWalletOwnersRepository> _walletOwnersRepoMock = new Mock<IWalletOwnersRepository>();

        [Fact]
        public async Task HandleAsync_NoCustomerWithThisWalletAddress_BalanceServiceNotCalled()
        {
            _walletOwnersRepoMock.Setup(x => x.GetByWalletAddressAsync(It.IsAny<string>()))
                .ReturnsAsync((IWalletOwner) null);

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeWalletAddress);

            _balanceServiceMock.Verify(x =>
                x.ForceBalanceUpdateAsync(FakeCustomerId, OperationType.StakeOperation, Guid.NewGuid()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_BalanceServiceCalledToForceUpdate()
        {
            _walletOwnersRepoMock.Setup(x => x.GetByWalletAddressAsync(It.IsAny<string>()))
                .ReturnsAsync(new WalletOwnerEntity{OwnerId = FakeCustomerId});

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeWalletAddress);

            _balanceServiceMock.Verify(x =>
                x.ForceBalanceUpdateAsync(FakeCustomerId, OperationType.StakeOperation, It.IsAny<Guid>()), Times.Once);
        }

        private StakedBalanceChangedHandler CreateSutInstance()
        {
            return new StakedBalanceChangedHandler(_balanceServiceMock.Object, _walletOwnersRepoMock.Object,
                EmptyLogFactory.Instance);
        }
    }
}
