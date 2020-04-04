using System.Threading.Tasks;

namespace MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface ICustomerProfileDeactivationRequestedHandler
    {
        Task HandleAsync(string customerId);
    }
}
