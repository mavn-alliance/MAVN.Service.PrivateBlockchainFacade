using JetBrains.Annotations;

namespace Lykke.Service.PrivateBlockchainFacade.Client
{
    /// <summary>
    ///  Errors while getting customer wallet address
    /// </summary>
    [PublicAPI]
    public enum CustomerWalletAddressError
    {
        /// <summary>
        /// No errors
        /// </summary>
        None,
        
        /// <summary>
        /// The customer doesn't have wallet address assigned
        /// </summary>
        CustomerWalletMissing,
        
        /// <summary>
        /// The customer id is not valid
        /// </summary>
        InvalidCustomerId,
        /// <summary>
        /// The wallet address is nto valid
        /// </summary>
        InvalidWalletAddress
    }
}
