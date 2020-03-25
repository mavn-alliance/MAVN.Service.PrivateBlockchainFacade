using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.QuorumTransactionWatcher.Contract;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace Lykke.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Subscribers
{
    public class StakeIncreasedSubscriber : JsonRabbitSubscriber<StakeIncreasedEvent>
    {
        private readonly ILog _log;
        private readonly IStakedBalanceChangedHandler _handler;

        public StakeIncreasedSubscriber(
            string connectionString,
            string exchangeName,
            string queueName,
            ILogFactory logFactory,
            IStakedBalanceChangedHandler handler) : base(connectionString, exchangeName, queueName, true, logFactory)
        {
            _handler = handler;
            _log = logFactory.CreateLog(this);
        }

        protected override async Task ProcessMessageAsync(StakeIncreasedEvent evt)
        {
            await _handler.HandleAsync(evt.WalletAddress);

            _log.Info("Processed StakeIncreasedEvent", evt);
        }
    }
}
