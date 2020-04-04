using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.PrivateBlockchainFacade.Contract.Events;
using MAVN.Service.PrivateBlockchainFacade.Contract.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Balances;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers
{
    public class CustomerProfileDeactivationRequestedHandler : ICustomerProfileDeactivationRequestedHandler
    {
        private const string BalanceSeizeReason = "Profile deactivation";

        private readonly IBalanceService _balanceService;
        private readonly IOperationRequestsProducer _operationRequestsProducer;
        private readonly IWalletOwnersRepository _walletOwnersRepository;
        private readonly IRabbitPublisher<SeizeBalanceFromCustomerCompletedEvent> _seizeBalanceFromCustomerCompletedPublisher;
        private readonly ILog _log;

        public CustomerProfileDeactivationRequestedHandler(
            IBalanceService balanceService,
            IOperationRequestsProducer operationRequestsProducer,
            IWalletOwnersRepository walletOwnersRepository,
            IRabbitPublisher<SeizeBalanceFromCustomerCompletedEvent> seizeBalanceFromCustomerCompletedPublisher,
            ILogFactory logFactory)
        {
            _balanceService = balanceService;
            _operationRequestsProducer = operationRequestsProducer;
            _walletOwnersRepository = walletOwnersRepository;
            _seizeBalanceFromCustomerCompletedPublisher = seizeBalanceFromCustomerCompletedPublisher;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(string customerId)
        {
            var customerBalance = await _balanceService.GetAsync(customerId);

            if (customerBalance.Error != CustomerBalanceError.None)
            {
                _log.Error(message: "Cannot seize balance of a customer because of error", context: customerBalance.Error);
                return;
            }

            if (customerBalance.Total == 0)
            {
                await _seizeBalanceFromCustomerCompletedPublisher.PublishAsync(
                    new SeizeBalanceFromCustomerCompletedEvent {CustomerId = customerId});
                return;
            }

            var walletResult = await _walletOwnersRepository.GetByOwnerIdAsync(customerId);

            if (walletResult == null)
            {
                _log.Error(message: "Cannot seize balance of a customer because the wallet is missing", context: customerId);
                return;
            }

            await _operationRequestsProducer.AddAsync(customerId, OperationType.SeizeToInternal, new SeizeToInternalContext
            {
                Account = walletResult.WalletId,
                Amount = customerBalance.Total,
                Reason = BalanceSeizeReason,
            });
        }
    }
}
