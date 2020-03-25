using System.Threading.Tasks;
using Falcon.Numerics;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface ITransferToExternalRequestedHandler
    {
        Task HandleAsync(string operationId, string customerId, Money18 amount, string privateBlochcainGatewayAddress);
    }
}
