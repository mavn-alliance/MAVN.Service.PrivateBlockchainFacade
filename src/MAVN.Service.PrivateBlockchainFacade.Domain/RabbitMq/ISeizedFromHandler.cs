using System.Threading.Tasks;
using MAVN.Numerics;

namespace MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface ISeizedFromHandler
    {
        Task HandleAsync(string account, Money18 amount);
    }
}
