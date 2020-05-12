using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Job.QuorumTransactionWatcher.Contract;
using Lykke.RabbitMqBroker.Subscriber;
using MAVN.Numerics;
using MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Subscribers
{
    public class SeizedFromSubscriber : JsonRabbitSubscriber<SeizedFromEvent>
    {
        private readonly ISeizedFromHandler _handler;
        private readonly ILog _log;

        public SeizedFromSubscriber(
            ISeizedFromHandler handler,
            string connectionString,
            string exchangeName,
            string queueName,
            ILogFactory logFactory)
            : base(connectionString, exchangeName, queueName, logFactory)
        {
            _handler = handler;
            _log = logFactory.CreateLog(this);
        }


        protected override async Task ProcessMessageAsync(SeizedFromEvent message)
        {
            await _handler.HandleAsync(message.Address, message.Amount);

            _log.Info("Handled SeizedFromEvent", message);
        }
    }
}
