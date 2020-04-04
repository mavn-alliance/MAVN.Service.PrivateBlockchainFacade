using System;
using System.Threading.Tasks;

namespace MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface ICustomerRegisteredInBlockchainHandler
    {
        Task HandleAsync(Guid customerId, string hash);
    }
}
