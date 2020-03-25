using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.PrivateBlockchainFacade.Contract.Events;
using Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace Lykke.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers
{
    public class CustomerRegisteredInBlockchainHandler : ICustomerRegisteredInBlockchainHandler
    {
        private readonly IRabbitPublisher<CustomerWalletCreatedEvent> _customerWalletCreationPublisher;
        private readonly ILog _log;

        public CustomerRegisteredInBlockchainHandler(
            IRabbitPublisher<CustomerWalletCreatedEvent> customerWalletCreationPublisher,
            ILogFactory logFactory)
        {
            _customerWalletCreationPublisher = customerWalletCreationPublisher;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(Guid customerId, string hash)
        {
            if (Guid.Empty == customerId)
            {
                _log.Warning("CustomerId is empty");
                return;
            }
            
            if (string.IsNullOrEmpty(hash))
            {
                _log.Warning("Hash is empty", context: new {customerId});
                return;
            }

            await _customerWalletCreationPublisher.PublishAsync(
                new CustomerWalletCreatedEvent {CustomerId = customerId, Timestamp = DateTime.UtcNow});
        }
    }
}
