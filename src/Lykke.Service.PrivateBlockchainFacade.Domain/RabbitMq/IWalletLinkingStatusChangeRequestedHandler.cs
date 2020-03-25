using System.Threading.Tasks;
using Falcon.Numerics;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface IWalletLinkingStatusChangeRequestedHandler
    {
        Task HandleWalletLinkingAsync(
            string eventId, string customerId, string masterWalletAddress, string internalAddress, string publicAddress,
            Money18 fee);

        Task HandleWalletUnlinkingAsync(string eventId, string customerId, string masterWalletAddress,
            string internalAddress);
    }
}
