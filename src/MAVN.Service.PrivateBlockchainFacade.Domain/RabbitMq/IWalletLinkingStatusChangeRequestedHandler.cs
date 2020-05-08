using System.Threading.Tasks;
using MAVN.Numerics;

namespace MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq
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
