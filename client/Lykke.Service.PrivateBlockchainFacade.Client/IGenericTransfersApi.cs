using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;
using Refit;

namespace Lykke.Service.PrivateBlockchainFacade.Client
{
    /// <summary>
    /// Transfers API interface
    /// </summary>
    [PublicAPI]
    public interface IGenericTransfersApi
    {
        /// <summary>
        /// Transfer tokens from customer to specific wallet address
        /// </summary>
        /// <param name="request">The transfer details</param>
        /// <returns></returns>
        [Post("/api/generic-transfers")]
        Task<TransferResponseModel> GenericTransferAsync([Body] GenericTransferRequestModel request);
    }
}
