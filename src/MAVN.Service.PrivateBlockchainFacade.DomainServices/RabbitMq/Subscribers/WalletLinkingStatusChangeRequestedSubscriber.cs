using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.CrossChainWalletLinker.Contract.Linking;
using MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Subscribers
{
    public class WalletLinkingStatusChangeRequestedSubscriber : JsonRabbitSubscriber<WalletLinkingStatusChangeRequestedEvent>
    {
        private readonly IWalletLinkingStatusChangeRequestedHandler _handler;
        private readonly ILog _log;

        public WalletLinkingStatusChangeRequestedSubscriber(
            IWalletLinkingStatusChangeRequestedHandler handler,
            string connectionString,
            string exchangeName,
            string queueName,
            ILogFactory logFactory)
            : base(connectionString, exchangeName, queueName, logFactory)
        {
            _handler = handler;
            _log = logFactory.CreateLog(this);
        }

        protected override async Task ProcessMessageAsync(WalletLinkingStatusChangeRequestedEvent message)
        {
            if (message.Direction == LinkingDirection.Link)
            {
                await _handler.HandleWalletLinkingAsync(message.EventId, message.CustomerId,
                    message.MasterWalletAddress, message.PrivateAddress, message.PublicAddress, message.Fee);
            }
            else
            {
                await _handler.HandleWalletUnlinkingAsync(message.EventId, message.CustomerId,
                    message.MasterWalletAddress, message.PrivateAddress);
            }

            _log.Info("Processed WalletLinkingStatusChangeRequestedEvent", message);
        }
    }
}
