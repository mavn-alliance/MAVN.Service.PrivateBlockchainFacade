using JetBrains.Annotations;

namespace Lykke.Service.PrivateBlockchainFacade.Client
{
    /// <summary>
    /// Customer wallet creation error
    /// </summary>
    [PublicAPI]
    public enum CustomerWalletCreationError
    {
        /// <summary>
        /// No errors
        /// </summary>
        None,
        
        /// <summary>
        /// The customer wallet has already been created
        /// </summary>
        AlreadyCreated,
        
        /// <summary>
        /// Customer id is invalid
        /// </summary>
        InvalidCustomerId
    }
}
