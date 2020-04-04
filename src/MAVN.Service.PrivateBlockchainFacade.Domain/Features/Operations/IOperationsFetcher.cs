using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations
{
    public interface IOperationsFetcher
    {
        /// <summary>
        /// Get a batch of operations in status "Created", maximum is limited 
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<OperationRequestModel>> GetRequestsAsync();

        /// <summary>
        /// Get a batch of operations in status "Accepted", maximum is limited
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<AcceptedOperationModel>> GetAcceptedAsync();

        /// <summary>
        /// Get transfer operations in progress
        /// </summary>
        /// <param name="walletAddress">The payee wallet address</param>
        /// <returns></returns>
        Task<IReadOnlyList<IOperation>> GetTransfersInProgressAsync(string walletAddress);

        /// <summary>
        /// Get operation by transaction hash value
        /// </summary>
        /// <param name="hash">The transaction hash</param>
        /// <returns></returns>
        Task<IOperation> GetByHashAsync(string hash);

        /// <summary>
        /// Get operations by id, checks among operation requests as well
        /// </summary>
        /// <param name="id">The operation id</param>
        /// <returns></returns>
        Task<IOperation> GetByIdAsync(Guid id);

        /// <summary>
        /// Get seize operations for customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        Task<IReadOnlyList<IOperation>> GetSeizeOperationsInProgressAsync(string customerId);
    }
}
