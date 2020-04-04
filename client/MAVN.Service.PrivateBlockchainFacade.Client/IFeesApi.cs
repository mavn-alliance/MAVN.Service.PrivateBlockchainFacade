using System.Threading.Tasks;
using MAVN.Service.PrivateBlockchainFacade.Client.Models;
using Refit;

namespace MAVN.Service.PrivateBlockchainFacade.Client
{
    /// <summary>
    /// FeesAPI
    /// </summary>
    public interface IFeesApi
    {
        /// <summary>
        /// Get transfers to public fee
        /// </summary>
        /// <returns></returns>
        [Get("/api/fees/transfer-to-public")]
        Task<TransferToPublicFeeResponseModel> GetTransferToPublicFeeAsync();

        /// <summary>
        /// Set transfers to public fee
        /// </summary>
        /// <param name="request"></param>
        [Post("/api/fees/transfer-to-public")]
        Task<SetTransferToPublicFeeResponseModel> SetTransferToPublicFeeAsync(SetTransferToPublicFeeRequestModel request);
    }
}
