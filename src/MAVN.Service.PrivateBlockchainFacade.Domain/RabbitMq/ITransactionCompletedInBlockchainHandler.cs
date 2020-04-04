using System;
using System.Threading.Tasks;

namespace MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq
{
    public interface ITransactionCompletedInBlockchainHandler
    {
        Task HandleAsync(string hash);
    }
}
