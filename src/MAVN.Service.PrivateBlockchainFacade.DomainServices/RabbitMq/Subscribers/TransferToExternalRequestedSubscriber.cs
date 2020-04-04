using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.CrossChainTransfers.Contract;
using MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Subscribers
{
    public class TransferToExternalRequestedSubscriber : JsonRabbitSubscriber<TransferToExternalRequestedEvent>
    {
        private readonly ITransferToExternalRequestedHandler _handler;
        private readonly ILog _log;

        public TransferToExternalRequestedSubscriber(
            ITransferToExternalRequestedHandler handler,
            string connectionString,
            string exchangeName,
            string queueName,
            ILogFactory logFactory)
            : base(connectionString, exchangeName, queueName, logFactory)
        {
            _handler = handler;
            _log = logFactory.CreateLog(this);
        }

        protected override async Task ProcessMessageAsync(TransferToExternalRequestedEvent message)
        {
            await _handler.HandleAsync(message.OperationId, message.CustomerId, message.Amount,
                message.PrivateBlockchainGatewayContractAddress);
            _log.Info("Processed TransferToExternalRequestedEvent", message);
        }
    }
}
