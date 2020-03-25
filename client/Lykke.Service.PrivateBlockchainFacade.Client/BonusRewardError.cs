using JetBrains.Annotations;

namespace Lykke.Service.PrivateBlockchainFacade.Client
{
    /// <summary>
    /// Bonus reward error
    /// </summary>
    [PublicAPI]
    public enum BonusRewardError
    {
        /// <summary>
        /// No errors
        /// </summary>
        None,

        /// <summary>
        /// The same reward request has already been received 
        /// </summary>
        DuplicateRequest,

        /// <summary>
        /// The customer id value is not valid
        /// </summary>
        InvalidCustomerId,

        /// <summary>
        /// Invalid amount
        /// </summary>
        InvalidAmount,

        /// <summary>
        /// The customer wallet has not been created yet
        /// </summary>
        CustomerWalletMissing,

        /// <summary>
        /// The bonus reason missed.
        /// </summary>
        MissingBonusReason,
        
        /// <summary>
        /// The campaign identifier is not valid.
        /// </summary>
        InvalidCampaignId
    }
}
