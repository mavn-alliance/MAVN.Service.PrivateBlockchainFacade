using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Job.QuorumTransactionWatcher.Contract;
using Lykke.RabbitMqBroker.Subscriber;
using MAVN.Numerics;
using MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Subscribers
{
    public class TransferSubscriber : JsonRabbitSubscriber<TransferEvent>
    {
        private readonly ILog _log;
        private readonly ITransferEventHandler _transferEventHandler;
        
        public TransferSubscriber(
            string connectionString,
            string exchangeName,
            string queueName,
            ILogFactory logFactory,
            ITransferEventHandler transferEventHandler) : base(connectionString, exchangeName, queueName, true, logFactory)
        {
            _transferEventHandler = transferEventHandler;
            _log = logFactory.CreateLog(this);
        }

        protected override async Task ProcessMessageAsync(TransferEvent evt)
        {
            await _transferEventHandler.HandleAsync(
                evt.SourceAddress, 
                evt.TargetAddress,
                Money18.Parse(evt.Amount.ToString()),
                evt.TransactionHash, 
                evt.ObservedAt);
            
            _log.Info("Processed Transfer Event", evt);
        }
    }
}
