using System;
using System.Threading.Tasks;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations
{
    public interface IOperationRequestsProducer
    {
        /// <summary>
        /// Adds new operation into the operations pool. 
        /// </summary>
        /// <param name="customerId">The customer id</param>
        /// <param name="operationType">The operation type</param>
        /// <param name="context">The json-serialized context</param>
        /// <param name="ownerWalletAddress">The master wallet address, owner of the operation. If not set, default master wallet address will be used.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<Guid> AddAsync<T>(
            string customerId, 
            OperationType operationType, 
            T context,
            string ownerWalletAddress = null);

        /// <summary>
        /// Adds new operation into the operations pool. 
        /// </summary>
        /// <param name="operationType">The operation type</param>
        /// <param name="context">The json-serialized context</param>
        /// <param name="ownerWalletAddress">The master wallet address, owner of the operation. If not set, default master wallet address will be used.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<Guid> AddAsync<T>(
            OperationType operationType,
            T context,
            string ownerWalletAddress = null);
    }
}
