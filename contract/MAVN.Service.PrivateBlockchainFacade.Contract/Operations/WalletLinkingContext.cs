using Falcon.Numerics;

namespace MAVN.Service.PrivateBlockchainFacade.Contract.Operations
{
    public class WalletLinkingContext
    {
        /// <summary>
        /// The internal wallet address of the customer
        /// </summary>
        public string InternalWalletAddress { get; set; }

        /// <summary>
        /// The public wallet address of the customer
        /// </summary>
        public string PublicWalletAddress { get; set; }

        /// <summary>
        /// Fee for the link operation
        /// </summary>
        public Money18 LinkingFee { get; set; }
    }
}
