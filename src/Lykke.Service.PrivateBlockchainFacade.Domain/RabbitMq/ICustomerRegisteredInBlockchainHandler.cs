using System;
using System.Threading.Tasks;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface ICustomerRegisteredInBlockchainHandler
    {
        Task HandleAsync(Guid customerId, string hash);
    }
}
