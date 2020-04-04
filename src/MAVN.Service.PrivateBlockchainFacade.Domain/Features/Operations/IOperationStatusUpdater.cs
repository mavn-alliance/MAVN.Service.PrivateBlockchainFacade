using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations
{
    public interface IOperationStatusUpdater
    {
        /// <summary>
        /// Set operation status to Accepted
        /// </summary>
        /// <param name="id">The operation id</param>
        /// <param name="hash">The operation hash which is required when accepting operation</param>
        Task<OperationStatusUpdateResultModel> AcceptAsync(Guid id, string hash);

        /// <summary>
        /// Set operation status to Accepted
        /// </summary>
        /// <param name="operationsHashesDict">Operation id to hash dictionary</param>
        Task<OperationStatusUpdateResultModel> AcceptBatchAsync(Dictionary<Guid, string> operationsHashesDict);

        /// <summary>
        /// Set operation status to Failed
        /// </summary>
        /// <param name="hash">The operation hash</param>
        Task<OperationStatusUpdateResultModel> FailAsync(string hash);

        /// <summary>
        /// Set operation status to succeeded
        /// </summary>
        /// <param name="hash">The operation hash</param>
        Task<OperationStatusUpdateResultModel> SucceedAsync(string hash);

        /// <summary>
        /// Set operation status to succeeded or failed depending on the operation status in TransactionExecutor
        /// </summary>
        /// <param name="hash">The operation hash</param>
        Task<OperationStatusUpdateResultModel> SyncWithBlockchainAsync(string hash);
    }
}
