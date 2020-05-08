using System.Numerics;
using System.Threading.Tasks;
using MAVN.Numerics;

namespace MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface ITransferToInternalDetectedHandler
    {
        Task HandleAsync(BigInteger publicTransferId, string privateWalletAddress, string publicWalletAddress, Money18 amount);
    }
}
