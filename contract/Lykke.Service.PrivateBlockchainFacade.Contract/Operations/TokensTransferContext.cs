using Falcon.Numerics;

namespace Lykke.Service.PrivateBlockchainFacade.Contract.Operations
{
    /// <summary>
    /// Tokens transfer operation context
    /// </summary>
    public class TokensTransferContext
    {
        /// <summary>
        /// The sender wallet address
        /// </summary>
        public string SenderWalletAddress { get; set; }
        
        /// <summary>
        /// The recipient wallet address
        /// </summary>
        public string RecipientWalletAddress { get; set; }
        
        /// <summary>
        /// The amount to be transferred
        /// </summary>
        public Money18 Amount { get; set; }
        
        /// <summary>
        /// The external identifier of the operation
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Additional data which would be passed to blockchain
        /// </summary>
        public string AdditionalData { get; set; }
    }
}
