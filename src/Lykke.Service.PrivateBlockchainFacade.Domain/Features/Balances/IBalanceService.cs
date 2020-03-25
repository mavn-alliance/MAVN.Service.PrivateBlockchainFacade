using System;
using System.Threading.Tasks;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.Features.Balances
{
    public interface IBalanceService
    {
        Task<CustomerBalanceResultModel> GetAsync(string customerId);
        
        Task<CustomerBalanceResultModel> ForceBalanceUpdateAsync(string customerId, OperationType operationCausedUpdate, Guid operationId);
    }
}
