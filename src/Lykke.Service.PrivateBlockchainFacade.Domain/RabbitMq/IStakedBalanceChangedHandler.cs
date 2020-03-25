using System.Threading.Tasks;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface IStakedBalanceChangedHandler
    {
        Task HandleAsync(string walletAddress);
    }
}
