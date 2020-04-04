using System.Threading.Tasks;

namespace MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface ITransactionFailedInBlockchainHandler
    {
        Task HandleAsync(string hash);
    }
}
