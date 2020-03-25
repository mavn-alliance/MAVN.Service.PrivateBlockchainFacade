using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.QuorumTransactionWatcher.Contract;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace Lykke.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Subscribers
{
    public class FeeCollectedSubscriber : JsonRabbitSubscriber<FeeCollectedEvent>
    {
        private readonly IFeeCollectedHandler _handler;
        private readonly ILog _log;

        public FeeCollectedSubscriber(
            string connectionString,
            string exchangeName,
            string queueName,
            ILogFactory logFactory,
            IFeeCollectedHandler handler) : base(connectionString, exchangeName, queueName, true, logFactory)
        {
            _handler = handler;
            _log = logFactory.CreateLog(this);
        }

        protected override async Task ProcessMessageAsync(FeeCollectedEvent evt)
        {
            await _handler.HandleAsync(evt.EventId, evt.WalletAddress, evt.Reason, evt.Amount, evt.TransactionHash);

            _log.Info("Processed FeeCollectedEvent", evt);
        }
    }
}
