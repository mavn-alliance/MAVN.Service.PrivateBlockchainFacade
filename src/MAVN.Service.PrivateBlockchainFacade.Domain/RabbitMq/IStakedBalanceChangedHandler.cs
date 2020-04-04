using System.Threading.Tasks;

namespace MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface IStakedBalanceChangedHandler
    {
        Task HandleAsync(string walletAddress);
    }
}
