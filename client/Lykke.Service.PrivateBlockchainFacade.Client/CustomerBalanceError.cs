namespace Lykke.Service.PrivateBlockchainFacade.Client
{
    /// <summary>
    /// Errors while getting customer wallet balance
    /// </summary>
    public enum CustomerBalanceError
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
        InvalidCustomerId
    }
}
