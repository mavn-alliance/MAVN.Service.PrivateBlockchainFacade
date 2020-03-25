namespace Lykke.Service.PrivateBlockchainFacade.Contract.Events
{
    public enum FeeCollectedReason
    {
        /// <summary>
        /// Fee was collected because the customer has linked his wallet to a public one
        /// </summary>
        WalletLinking,
        /// <summary>
        /// Fee was collected because the customer has transferred tokens to his public wallet
        /// </summary>
        TransferToPublic
    }
}
