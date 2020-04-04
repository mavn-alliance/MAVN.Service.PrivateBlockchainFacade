using JetBrains.Annotations;

namespace MAVN.Service.PrivateBlockchainFacade.Contract.Events
{
    /// <summary>
    /// The reason for updating customer's balance
    /// </summary>
    [PublicAPI]
    public enum CustomerBalanceUpdateReason
    {
        /// <summary>
        /// The bonus reward has been added 
        /// </summary>
        BonusReward,
        
        /// <summary>
        /// The funds transfer has been completed
        /// </summary>
        Transfer,
        /// <summary>
        /// Staked tokens has been changed
        /// </summary>
        Stake,
        /// <summary>
        /// Paid fee for wallet linking
        /// </summary>
        WalletLinking,
        /// <summary>
        /// Transferred tokens to the public network
        /// </summary>
        TransferToExternal,
        /// <summary>
        /// Transferred tokens from the public network
        /// </summary>
        TransferToInternal,
        /// <summary>
        /// The balance was seized
        /// </summary>
        SeizedBalance
    }
}
