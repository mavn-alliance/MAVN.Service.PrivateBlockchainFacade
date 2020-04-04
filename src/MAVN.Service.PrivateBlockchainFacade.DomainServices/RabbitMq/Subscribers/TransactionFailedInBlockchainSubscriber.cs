using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.QuorumTransactionWatcher.Contract;
using Lykke.RabbitMqBroker.Subscriber;
using MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Subscribers
{
    public class TransactionFailedInBlockchainSubscriber : JsonRabbitSubscriber<TransactionFailedInBlockchainEvent>
    {
        private readonly ILog _log;
        private readonly ITransactionFailedInBlockchainHandler _transactionFailedInBlockchainHandler;

        public TransactionFailedInBlockchainSubscriber(
            string connectionString,
            string exchangeName,
            string queueName,
            ILogFactory logFactory,
            ITransactionFailedInBlockchainHandler transactionFailedInBlockchainHandler) : base(connectionString,
            exchangeName, queueName, true, logFactory)
        {
            _log = logFactory.CreateLog(this);
            _transactionFailedInBlockchainHandler = transactionFailedInBlockchainHandler;
        }

        protected override async Task ProcessMessageAsync(TransactionFailedInBlockchainEvent evt)
        {
            await _transactionFailedInBlockchainHandler.HandleAsync(evt.TransactionHash);
            _log.Info("Processed Transaction Failed In Blockchain Event", evt);
        }
    }
}
