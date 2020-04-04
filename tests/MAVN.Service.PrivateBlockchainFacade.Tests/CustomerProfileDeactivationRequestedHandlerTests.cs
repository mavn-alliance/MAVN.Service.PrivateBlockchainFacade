using System.Threading.Tasks;
using Lykke.Logs;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.PrivateBlockchainFacade.Contract.Events;
using MAVN.Service.PrivateBlockchainFacade.Contract.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Balances;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities;
using Moq;
using Xunit;

namespace MAVN.Service.PrivateBlockchainFacade.Tests
{
    public class CustomerProfileDeactivationRequestedHandlerTests
    {
        private const string FakeCustomerId = "108e049b-81de-42b7-88e5-a5ac276fae88";
        private const string FakeWalletId = "203e049b-81de-42b7-88e5-a5ac276fae88";
        private const long FakeBalance = 100;

        private readonly Mock<IBalanceService> _balanceServiceMock = new Mock<IBalanceService>();
        private readonly Mock<IOperationRequestsProducer> _operationRequestProducerMock = new Mock<IOperationRequestsProducer>();
        private readonly Mock<IWalletOwnersRepository> _walletOwnersRepoMock = new Mock<IWalletOwnersRepository>();
        private readonly Mock<IRabbitPublisher<SeizeBalanceFromCustomerCompletedEvent>>
            _seizeBalanceFromCustomerCompletedPublisher =
                new Mock<IRabbitPublisher<SeizeBalanceFromCustomerCompletedEvent>>();
         

        [Fact]
        public async Task HandleAsync_ErrorWhenGettingTheBalance_WalletOwnersRepoNotCalled()
        {
            _balanceServiceMock.Setup(x => x.GetAsync(FakeCustomerId))
                .ReturnsAsync(CustomerBalanceResultModel.Failed(CustomerBalanceError.CustomerWalletMissing));

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeCustomerId);

            _walletOwnersRepoMock.Verify(x => x.GetByOwnerIdAsync(FakeCustomerId), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_CustomerHas0Balance_PublisherCalledAndWalletOwnersRepoNotCalled()
        {
            _balanceServiceMock.Setup(x => x.GetAsync(FakeCustomerId))
                .ReturnsAsync(CustomerBalanceResultModel.Succeeded(0,0));

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeCustomerId);

            _seizeBalanceFromCustomerCompletedPublisher.Verify(x =>
                x.PublishAsync(It.Is<SeizeBalanceFromCustomerCompletedEvent>(e => e.CustomerId == FakeCustomerId)));
            _walletOwnersRepoMock.Verify(x => x.GetByOwnerIdAsync(FakeCustomerId), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WalletOwnerDoesNotExist_OperationProducerNotCalled()
        {
            _balanceServiceMock.Setup(x => x.GetAsync(FakeCustomerId))
                .ReturnsAsync(CustomerBalanceResultModel.Succeeded(FakeBalance, 0));

            _walletOwnersRepoMock.Setup(x => x.GetByOwnerIdAsync(FakeCustomerId))
                .ReturnsAsync((IWalletOwner) null);

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeCustomerId);

            _operationRequestProducerMock.Verify(
                x => x.AddAsync(FakeCustomerId, OperationType.SeizeToInternal, It.IsAny<SeizeToInternalContext>(),
                    null), Times.Never);
        }

        [Fact]
        public async Task HandleAsync__OperationProducerCalled()
        {
            _balanceServiceMock.Setup(x => x.GetAsync(FakeCustomerId))
                .ReturnsAsync(CustomerBalanceResultModel.Succeeded(FakeBalance, 0));

            _walletOwnersRepoMock.Setup(x => x.GetByOwnerIdAsync(FakeCustomerId))
                .ReturnsAsync(new WalletOwnerEntity
                {
                    WalletId = FakeWalletId
                });

            var sut = CreateSutInstance();

            await sut.HandleAsync(FakeCustomerId);

            _operationRequestProducerMock.Verify(
                x => x.AddAsync(FakeCustomerId, OperationType.SeizeToInternal,
                    It.Is<SeizeToInternalContext>(c => c.Account == FakeWalletId && c.Amount == FakeBalance),
                    null), Times.Once);
        }

        private CustomerProfileDeactivationRequestedHandler CreateSutInstance()
        {
            return new CustomerProfileDeactivationRequestedHandler(
                _balanceServiceMock.Object,
                _operationRequestProducerMock.Object,
                _walletOwnersRepoMock.Object,
                _seizeBalanceFromCustomerCompletedPublisher.Object,
                EmptyLogFactory.Instance);
        }
    }
}
