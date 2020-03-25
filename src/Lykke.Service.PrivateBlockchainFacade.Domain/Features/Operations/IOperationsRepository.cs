using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations
{
    public interface IOperationsRepository
    {
        /// <summary>
        /// Add new operation
        /// </summary>
        /// <param name="id">The id of the operation</param>
        /// <param name="customerId">The customer id</param>
        /// <param name="nonce">The unique operation id within masterWalletAddress</param>
        /// <param name="masterWalletAddress">The master wallet address, the owner of the operation</param>
        /// <param name="type">The operation type</param>
        /// <param name="contextJson">The operation type specific details</param>
        /// <param name="createdAt">The date and time of creation</param>
        /// <param name="transactionHash">The blockchain operation transaction hash</param>
        Task<Guid> AddAsync(
            Guid id,
            string customerId,
            long nonce,
            string masterWalletAddress,
            OperationType type,
            string contextJson,
            DateTime createdAt,
            string transactionHash);

        /// <summary>
        /// Update operation status by id
        /// </summary>
        /// <param name="id">The operation id</param>
        /// <param name="status">The status value</param>
        Task SetStatusAsync(Guid id, OperationStatus status);

        /// <summary>
        /// Update operation status and transaction hash by id
        /// </summary>
        /// <param name="id">The operation id</param>
        /// <param name="status">The status value</param>
        /// <param name="hash">The operation hash</param>
        /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException">Thrown when there is already an operation with the same hash</exception>
        Task SetStatusAsync(Guid id, OperationStatus status, string hash);

        /// <summary>
        /// Get all operations by status
        /// </summary>
        /// <param name="status">The operation status</param>
        /// <param name="max">The maximum amount of records to return</param>
        Task<IReadOnlyList<IOperation>> GetByStatusAsync(OperationStatus status, int max);

        /// <summary>
        /// Get operation by id
        /// </summary>
        Task<IOperation> GetByIdAsync(Guid id);

        /// <summary>
        /// Get ids of existing operations
        /// </summary>
        Task<List<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids);

        /// <summary>
        /// Get operation by hash
        /// </summary>
        /// <param name="hash"></param>
        Task<IOperation> GetByHashAsync(string hash);

        /// <summary>
        /// Get all operations that satisfy conditional expression
        /// </summary>
        /// <param name="condition">Where clause</param>
        Task<IReadOnlyList<IOperation>> GetByConditionAsync(Expression<Func<IOperation, bool>> condition);
    }
}
