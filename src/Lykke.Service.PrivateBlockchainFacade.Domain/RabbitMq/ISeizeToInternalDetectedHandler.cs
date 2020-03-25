using System.Threading.Tasks;
using Falcon.Numerics;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface ISeizeToInternalDetectedHandler
    {
        Task HandleAsync(string operationId, Money18 amount, string reason);
    }
}
