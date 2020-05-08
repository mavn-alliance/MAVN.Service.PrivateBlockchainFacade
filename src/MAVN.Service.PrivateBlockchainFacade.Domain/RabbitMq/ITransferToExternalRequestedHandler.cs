using System.Threading.Tasks;
using MAVN.Numerics;

namespace MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface ITransferToExternalRequestedHandler
    {
        Task HandleAsync(string operationId, string customerId, Money18 amount, string privateBlochcainGatewayAddress);
    }
}
