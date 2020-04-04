using JetBrains.Annotations;

namespace MAVN.Service.PrivateBlockchainFacade.Client
{
    /// <summary>
    /// Transfer error
    /// </summary>
    [PublicAPI]
    public enum TransferError
    {
        /// <summary>
        /// No errors
        /// </summary>
        None,
        
        /// <summary>
        /// Sender customer id is not valid
        /// </summary>
        InvalidSenderId,
        
        /// <summary>
        /// Recipient customer id is not valid
        /// </summary>
        InvalidRecipientId,

        /// <summary>
        /// Sender customer wallet has not been assigned yet
        /// </summary>
        SenderWalletMissing,

        /// <summary>
        /// Recipient customer wallet has not been assigned yet
        /// </summary>
        RecipientWalletMissing,
        
        /// <summary>
        /// Invalid amount
        /// </summary>
        InvalidAmount,
        
        /// <summary>
        /// Sender customer wallet doesn't have enough funds to transfer
        /// </summary>
        NotEnoughFunds,
        
        /// <summary>
        /// The same transfer request has already been received 
        /// </summary>
        DuplicateRequest,

        /// <summary>
        /// Additional data property does not start with "0x"
        /// </summary>
        InvalidAdditionalDataFormat,

        /// <summary>
        /// Customer private wallet is missing
        /// </summary>
        PrivateWalletMissing,

        /// <summary>
        /// Customer's public wallet is missing
        /// </summary>
        PublicWalletMissing,

        /// <summary>
        /// Fee value is not valid
        /// </summary>
        InvalidFeeAmount
    }
}
