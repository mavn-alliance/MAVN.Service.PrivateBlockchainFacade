namespace MAVN.Service.PrivateBlockchainFacade.Client.Models
{
    /// <summary>
    /// Get customer id by wallet address response
    /// </summary>
    public class CustomerIdByWalletAddressResponse
    {
        /// <summary>
        /// The wallet address
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Customer wallet address error
        /// </summary>
        public CustomerWalletAddressError Error { get; set; }
    }
}
