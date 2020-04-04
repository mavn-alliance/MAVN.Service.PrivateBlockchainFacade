using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.QuorumTransactionWatcher.Contract;
using Lykke.RabbitMqBroker.Subscriber;
using MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Subscribers
{
    public class StakeReleasedSubscriber : JsonRabbitSubscriber<StakeReleasedEvent>
    {
        private readonly ILog _log;
        private readonly IStakedBalanceChangedHandler _handler;

        public StakeReleasedSubscriber(
            string connectionString,
            string exchangeName,
            string queueName,
            ILogFactory logFactory,
            IStakedBalanceChangedHandler handler) : base(connectionString, exchangeName, queueName, true, logFactory)
        {
            _handler = handler;
            _log = logFactory.CreateLog(this);
        }

        protected override async Task ProcessMessageAsync(StakeReleasedEvent evt)
        {
            await _handler.HandleAsync(evt.WalletAddress);

            _log.Info("Processed StakeReleasedEvent", evt);
        }
    }
}
