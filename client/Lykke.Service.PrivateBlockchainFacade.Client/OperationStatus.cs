using JetBrains.Annotations;

namespace Lykke.Service.PrivateBlockchainFacade.Client
{
    /// <summary>
    /// The operation status enum
    /// </summary>
    [PublicAPI]
    public enum OperationStatus
    {
        /// <summary>
        /// Operation has been created (registered)
        /// </summary>
        Created,
        
        /// <summary>
        /// Operation has been taken for processing
        /// </summary>
        Accepted,
        
        /// <summary>
        /// Operation processed successfully
        /// </summary>
        Succeeded,
        
        /// <summary>
        /// Operation failed
        /// </summary>
        Failed
    }
}
