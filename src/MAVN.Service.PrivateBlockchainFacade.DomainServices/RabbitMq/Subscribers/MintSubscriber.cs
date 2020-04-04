using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.QuorumTransactionWatcher.Contract;
using Lykke.RabbitMqBroker.Subscriber;
using MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Subscribers
{
    public class MintSubscriber : JsonRabbitSubscriber<MintEvent>
    {
        private readonly ILog _log;
        private readonly IMintEventHandler _mintEventHandler;

        public MintSubscriber(
            string connectionString,
            string exchangeName,
            string queueName,
            ILogFactory logFactory,
            IMintEventHandler mintEventHandler) : base(connectionString, exchangeName, queueName, true, logFactory)
        {
            _log = logFactory.CreateLog(this);
            _mintEventHandler = mintEventHandler;
        }

        protected override async Task ProcessMessageAsync(MintEvent evt)
        {
            await _mintEventHandler.HandleAsync(evt.TransactionHash, evt.Amount, evt.TargetAddress, evt.ObservedAt);
            _log.Info("Processed Mint Event", evt);
        }
    }
}
