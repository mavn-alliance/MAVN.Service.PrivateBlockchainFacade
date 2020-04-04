using System.Threading.Tasks;
using JetBrains.Annotations;
using MAVN.Service.PrivateBlockchainFacade.Client.Models;
using Refit;

namespace MAVN.Service.PrivateBlockchainFacade.Client
{
    /// <summary>
    /// Wallets API interface.
    /// </summary>
    [PublicAPI]
    public interface IWalletsApi
    {
        /// <summary>
        /// Create request for customer wallet registration in private blockchain
        /// </summary>
        /// <param name="request">The customer wallet creation details</param>
        /// <returns></returns>
        [Post("/api/wallets")]
        Task<CustomerWalletCreationResponseModel> CreateAsync([Body] CustomerWalletCreationRequestModel request);
    }
}
