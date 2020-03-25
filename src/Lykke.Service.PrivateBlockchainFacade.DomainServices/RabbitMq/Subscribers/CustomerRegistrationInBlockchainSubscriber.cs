using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.QuorumTransactionWatcher.Contract;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace Lykke.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Subscribers
{
    public class CustomerRegistrationInBlockchainSubscriber : JsonRabbitSubscriber<CustomerRegisteredInBlockchainEvent>
    {
        private readonly ILog _log;
        private readonly ICustomerRegisteredInBlockchainHandler _customerRegisteredInBlockchainHandler;
        
        public CustomerRegistrationInBlockchainSubscriber(
            string connectionString,
            string exchangeName,
            string queueName,
            ILogFactory logFactory,
            ICustomerRegisteredInBlockchainHandler customerRegisteredInBlockchainHandler) : base(connectionString, exchangeName, queueName, true, logFactory)
        {
            _log = logFactory.CreateLog(this);
            _customerRegisteredInBlockchainHandler = customerRegisteredInBlockchainHandler;
        }

        protected override async Task ProcessMessageAsync(CustomerRegisteredInBlockchainEvent evt)
        {
            await _customerRegisteredInBlockchainHandler.HandleAsync(evt.CustomerId, evt.TransactionHash);
            
            _log.Info("Processed Customer Registered In Blockchain Event", evt);
        }
    }
}
