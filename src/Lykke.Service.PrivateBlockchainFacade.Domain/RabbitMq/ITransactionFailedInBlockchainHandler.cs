using System.Threading.Tasks;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface ITransactionFailedInBlockchainHandler
    {
        Task HandleAsync(string hash);
    }
}
