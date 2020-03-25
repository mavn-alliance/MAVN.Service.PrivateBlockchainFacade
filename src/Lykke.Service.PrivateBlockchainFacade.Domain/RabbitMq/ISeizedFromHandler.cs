using System.Threading.Tasks;
using Falcon.Numerics;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface ISeizedFromHandler
    {
        Task HandleAsync(string account, Money18 amount);
    }
}
