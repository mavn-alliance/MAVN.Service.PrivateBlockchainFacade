using JetBrains.Annotations;

namespace Lykke.Service.PrivateBlockchainFacade.Client
{
    /// <summary>
    /// Operation status update error enum
    /// </summary>
    [PublicAPI]
    public enum OperationStatusUpdateError
    {
        /// <summary>
        /// Operation has  been updated without errors
        /// </summary>
        None,
        
        /// <summary>
        /// Operation was not found
        /// </summary>
        OperationNotFound,
        
        /// <summary>
        /// Operation current status doesn't allow update to the desired status
        /// </summary>
        InvalidStatus,
        
        /// <summary>
        /// Operation transaction hash has invalid value
        /// </summary>
        InvalidTransactionHash,
        
        /// <summary>
        /// Transaction hash already used by another operation
        /// </summary>
        DuplicateTransactionHash,
        
        /// <summary>
        /// The transaction in blockchain has an empty related operation id
        /// </summary>
        OperationIdIsNull,
        
        /// <summary>
        /// The operation type in blockchain can't be mapped to operation type in domain
        /// </summary>
        UnsupportedOperationStatus
    }
}
