namespace MAVN.Service.PrivateBlockchainFacade.Contract.Events
{
    public class WalletStatusChangeFailedEvent
    {
        /// <summary>
        /// Wallet address of the customer in the internal network
        /// </summary>
        public string InternalWalletAddress { get; set; }

        /// <summary>
        /// Wallet address of the customer in the public network
        /// </summary>
        public string PublicWalletAddress { get; set; }
    }
}
