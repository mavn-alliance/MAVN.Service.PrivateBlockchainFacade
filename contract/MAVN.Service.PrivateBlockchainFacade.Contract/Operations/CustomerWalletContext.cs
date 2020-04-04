namespace MAVN.Service.PrivateBlockchainFacade.Contract.Operations
{
    /// <summary>
    /// Create wallet operation context
    /// </summary>
    public class CustomerWalletContext
    {
        /// <summary>
        /// The customer id
        /// </summary>
        public string CustomerId { get; set; }
        
        /// <summary>
        /// The customer wallet address
        /// </summary>
        public string WalletAddress { get; set; }
    }
}
