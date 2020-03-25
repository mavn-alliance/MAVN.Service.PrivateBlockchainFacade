using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations
{
    public interface IOperationRequestsRepository
    {
        /// <summary>
        /// Add new operation request
        /// </summary>
        /// <param name="customerId">The customer id</param>
        /// <param name="nonce">The unique operation id within masterWalletAddress</param>
        /// <param name="masterWalletAddress">The master wallet address, the owner of the operation</param>
        /// <param name="type">The operation type</param>
        /// <param name="contextJson">The operation type specific details</param>
        Task<Guid> AddAsync(
            string customerId,
            long nonce,
            string masterWalletAddress,
            OperationType type,
            string contextJson);

        /// <summary>
        /// Accept new operation
        /// </summary>
        /// <param name="id">The id of the operation</param>
        /// <param name="customerId">The customer id</param>
        /// <param name="nonce">The unique operation id within masterWalletAddress</param>
        /// <param name="masterWalletAddress">The master wallet address, the owner of the operation</param>
        /// <param name="type">The operation type</param>
        /// <param name="contextJson">The operation type specific details</param>
        /// <param name="createdAt">The date and time of creation</param>
        /// <param name="transactionHash">The blockchain transaction hash</param>
        Task AcceptAsync(
            Guid id,
            string customerId,
            long nonce,
            string masterWalletAddress,
            OperationType type,
            string contextJson,
            DateTime createdAt,
            string transactionHash);

        Task AcceptBatchAsync(IEnumerable<IOperationRequest> operationRequests, Dictionary<Guid, string> operationsHashesDict);

        /// <summary>
        /// Generate new nonce value
        /// </summary>
        Task<INonceCounter> GenerateNextCounterAsync(string masterWalletAddress);

        /// <summary>
        /// Get all operation requests
        /// </summary>
        /// <param name="max">The maximum amount of records to return</param>
        Task<IReadOnlyList<IOperationRequest>> GetAsync(int max);

        /// <summary>
        /// Get operation request by hash
        /// </summary>
        /// <param name="hash">The operation request hash</param>
        Task<IOperationRequest> GetByHashAsync(string hash);

        /// <summary>
        /// Get operation request by id
        /// </summary>
        /// <param name="id">The operation request id</param>
        Task<IOperationRequest> GetByIdAsync(Guid id);

        /// <summary>
        /// Get operation requests by ids
        /// </summary>
        /// <param name="ids">The operation requests ids</param>
        Task<Dictionary<Guid, IOperationRequest>> GetByIdsAsync(IEnumerable<Guid> ids);
    }
}
