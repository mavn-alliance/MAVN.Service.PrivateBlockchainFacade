using JetBrains.Annotations;

namespace Lykke.Service.PrivateBlockchainFacade.Client.Models
{
    /// <summary>
    /// Get customer wallet response 
    /// </summary>
    [PublicAPI]
    public class CustomerWalletAddressResponseModel
    {
        /// <summary>
        /// The wallet address
        /// </summary>
        public string WalletAddress { get; set; }
        
        /// <summary>
        /// Customer wallet address error
        /// </summary>
        public CustomerWalletAddressError Error { get; set; }
    }
}
