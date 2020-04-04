using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MAVN.Service.PrivateBlockchainFacade.Client.Models;
using Refit;

namespace MAVN.Service.PrivateBlockchainFacade.Client
{
    /// <summary>
    /// Operations API interface
    /// </summary>
    [PublicAPI]
    public interface IOperationsApi
    {
        /// <summary>
        /// Get list of new operations, maximum 100 per request
        /// </summary>
        /// <returns></returns>
        [Get("/api/operations/new")]
        Task<IEnumerable<NewOperationResponseModel>> GetNewOperationsAsync();

        /// <summary>
        /// Get list of accepted operations, maximum 100 per request
        /// </summary>
        /// <returns></returns>
        [Get("/api/operations/accepted")]
        Task<IEnumerable<AcceptedOperationResponseModel>> GetAcceptedOperationsAsync();

        /// <summary>
        /// Operation accepted
        /// </summary>
        /// <param name="id">The operation id</param>
        /// <param name="hash">The operation hash</param>
        /// <returns></returns>
        [Put("/api/operations/{id}/accepted")]
        Task<OperationStatusUpdateResponseModel> AcceptAsync(Guid id, string hash);

        /// <summary>
        /// Operation accepted
        /// </summary>
        /// <param name="operationsHashesDict">The operation id to hash dicttionary</param>
        /// <returns></returns>
        [Put("/api/operations/accepted")]
        Task<OperationStatusUpdateResponseModel> AcceptBatchAsync(Dictionary<Guid, string> operationsHashesDict);

        /// <summary>
        /// Operation failed 
        /// </summary>
        /// <param name="hash">The operation hash</param>
        /// <returns></returns>
        [Put("/api/operations/{hash}/failed")]
        Task<OperationStatusUpdateResponseModel> FailAsync(string hash);

        /// <summary>
        /// Operation succeeded
        /// </summary>
        /// <param name="hash">The operation hash</param>
        /// <returns></returns>
        [Put("/api/operations/{hash}/succeeded")]
        Task<OperationStatusUpdateResponseModel> SucceedAsync(string hash);

        /// <summary>
        /// Endpoint for generic operations which are rerouted to blockchain
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Post("/api/operations")]
        Task<GenericOperationResponse> AddGenericOperationAsync(GenericOperationRequest request);
    }
}
