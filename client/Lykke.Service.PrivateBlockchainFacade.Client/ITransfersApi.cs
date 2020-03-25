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
    public interface ITransfersApi
    {
        /// <summary>
        /// Transfer tokens between customers
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Post("/api/transfers")]
        Task<TransferResponseModel> TransferAsync([Body] TransferRequestModel request);
    }
}
