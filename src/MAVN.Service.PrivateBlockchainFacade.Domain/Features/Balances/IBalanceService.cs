using System;
using System.Threading.Tasks;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;

namespace MAVN.Service.PrivateBlockchainFacade.Domain.Features.Balances
{
    public interface IBalanceService
    {
        Task<CustomerBalanceResultModel> GetAsync(string customerId);
        
        Task<CustomerBalanceResultModel> ForceBalanceUpdateAsync(string customerId, OperationType operationCausedUpdate, Guid operationId);
    }
}
