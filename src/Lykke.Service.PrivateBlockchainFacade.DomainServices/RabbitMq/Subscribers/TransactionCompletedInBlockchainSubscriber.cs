using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.QuorumTransactionWatcher.Contract;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace Lykke.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Subscribers
{
    public class TransactionCompletedInBlockchainSubscriber : JsonRabbitSubscriber<TransactionCompletedInBlockchainEvent>
    {
        private readonly ITransactionCompletedInBlockchainHandler _handler;
        private readonly ILog _log;
        public TransactionCompletedInBlockchainSubscriber(
            ITransactionCompletedInBlockchainHandler handler,
            string connectionString,
            string exchangeName,
            string queueName,
            ILogFactory logFactory) : base(connectionString, exchangeName, queueName, logFactory)
        {
            _handler = handler;
            _log = logFactory.CreateLog(this);
        }

        protected override async Task ProcessMessageAsync(TransactionCompletedInBlockchainEvent message)
        {
            await _handler.HandleAsync(message.TransactionHash);
            _log.Info("Processed Transaction Completed In Blockchain Event", message);
        }
    }
}
