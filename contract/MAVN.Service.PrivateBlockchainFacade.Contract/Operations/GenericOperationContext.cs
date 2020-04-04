namespace MAVN.Service.PrivateBlockchainFacade.Contract.Operations
{
    public class GenericOperationContext
    {
        /// <summary>
        /// The source wallet address
        /// </summary>
        public string SourceWalletAddress { get; set; }

        /// <summary>
        /// The target wallet address
        /// </summary>
        public string TargetWalletAddress { get; set; }

        /// <summary>
        /// Additional data which would be passed to blockchain
        /// </summary>
        public string Data { get; set; }
    }
}
