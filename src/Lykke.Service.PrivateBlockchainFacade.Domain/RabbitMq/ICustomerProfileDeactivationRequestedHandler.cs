using System.Threading.Tasks;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface ICustomerProfileDeactivationRequestedHandler
    {
        Task HandleAsync(string customerId);
    }
}
