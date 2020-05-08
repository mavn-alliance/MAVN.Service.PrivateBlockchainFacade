using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Job.EthereumBridge.Contract;
using Lykke.RabbitMqBroker.Subscriber;
using MAVN.Numerics;
using MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Subscribers
{
    public class SeizeToInternalDetectedSubscriber : JsonRabbitSubscriber<SeizeToInternalDetectedEvent>
    {
        private readonly ISeizeToInternalDetectedHandler _seizeToInternalDetectedHandler;
        private readonly ILog _log;

        public SeizeToInternalDetectedSubscriber(
            string connectionString,
            string exchangeName,
            string queueName,
            ILogFactory logFactory,
            ISeizeToInternalDetectedHandler seizeToInternalDetectedHandler)
            : base(connectionString, exchangeName, queueName, true, logFactory)
        {
            _seizeToInternalDetectedHandler = seizeToInternalDetectedHandler;
            _log = logFactory.CreateLog(this);
        }

        protected override async Task ProcessMessageAsync(SeizeToInternalDetectedEvent @event)
        {
            await _seizeToInternalDetectedHandler.HandleAsync(@event.OperationId, Money18.Parse(@event.Amount.ToString()), @event.Reason);

            _log.Info("Processed seize event.",
                $"operationId: {@event.OperationId}; account : {@event.Account}; amount: {@event.Amount}; reason: {@event.Reason}");
        }
    }
}
