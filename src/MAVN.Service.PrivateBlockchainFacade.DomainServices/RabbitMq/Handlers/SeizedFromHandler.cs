using System.Threading.Tasks;
using MAVN.Numerics;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.PrivateBlockchainFacade.Contract.Events;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers
{
    public class SeizedFromHandler : ISeizedFromHandler
    {
        private readonly IWalletOwnersRepository _walletOwnersRepository;
        private readonly IRabbitPublisher<SeizedFromCustomerEvent> _seizedFromCustomerPublisher;
        private readonly IRabbitPublisher<SeizeBalanceFromCustomerCompletedEvent> _seizeBalanceFromCustomerCompletedPublisher;

        public SeizedFromHandler(
            IWalletOwnersRepository walletOwnersRepository,
            IRabbitPublisher<SeizedFromCustomerEvent> seizedFromCustomerPublisher,
            IRabbitPublisher<SeizeBalanceFromCustomerCompletedEvent> seizeBalanceFromCustomerCompletedPublisher)
        {
            _walletOwnersRepository = walletOwnersRepository;
            _seizedFromCustomerPublisher = seizedFromCustomerPublisher;
            _seizeBalanceFromCustomerCompletedPublisher = seizeBalanceFromCustomerCompletedPublisher;
        }

        public async Task HandleAsync(string account, Money18 amount)
        {
            var walletOwner = await _walletOwnersRepository.GetByWalletAddressAsync(account);

            //Seize is not for customer 
            if (walletOwner == null)
                return;

            var seizedFromTask = _seizedFromCustomerPublisher.PublishAsync(new SeizedFromCustomerEvent
            {
                CustomerId = walletOwner.OwnerId, Amount = amount,
            });

            var seizeBalanceFromCustomerCompletedTask =
                _seizeBalanceFromCustomerCompletedPublisher.PublishAsync(
                    new SeizeBalanceFromCustomerCompletedEvent {CustomerId = walletOwner.OwnerId});

            await Task.WhenAll(seizedFromTask, seizeBalanceFromCustomerCompletedTask);
        }
    }
}
