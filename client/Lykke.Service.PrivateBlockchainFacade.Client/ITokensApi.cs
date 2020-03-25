using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;
using Refit;

namespace Lykke.Service.PrivateBlockchainFacade.Client
{
    /// <summary>
    /// Tokens Api interface
    /// </summary>
    [PublicAPI]
    public interface ITokensApi
    {
        /// <summary>
        /// Method for getting total amount of tokens in private blockchain
        /// </summary>
        /// <returns>TotalTokensSupplyResponse which holds total tokens amount</returns>
        [Get("/api/tokens/total-supply")]
        Task<TotalTokensSupplyResponse> GetTotalTokensSupplyAsync();

        /// <summary>
        /// Method for getting total amount of tokens in gateway contract
        /// </summary>
        /// <returns>TotalTokensSupplyResponse which holds total tokens amount</returns>
        [Obsolete("Use GetTokenGatewayWalletBalance")]
        [Get("/api/tokens/mvn-gateway/total")]
        Task<TotalTokensSupplyResponse> GetMVNGatewayWalletBalance();

        /// <summary>
        /// Method for getting total amount of tokens in the token gateway contract
        /// </summary>
        /// <returns>TotalTokensSupplyResponse which holds total tokens amount</returns>
        [Get("/api/tokens/token-gateway/total")]
        Task<TotalTokensSupplyResponse> GetTokenGatewayWalletBalance();
    }
}
