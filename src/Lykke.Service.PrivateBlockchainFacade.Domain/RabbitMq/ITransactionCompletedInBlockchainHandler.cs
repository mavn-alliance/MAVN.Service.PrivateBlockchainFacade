using System;
using System.Threading.Tasks;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface ITransactionCompletedInBlockchainHandler
    {
        Task HandleAsync(string hash);
    }
}
