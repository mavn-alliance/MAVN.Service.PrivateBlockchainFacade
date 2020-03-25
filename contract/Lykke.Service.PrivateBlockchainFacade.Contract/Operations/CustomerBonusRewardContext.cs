using System;
using Falcon.Numerics;

namespace Lykke.Service.PrivateBlockchainFacade.Contract.Operations
{
    /// <summary>
    /// Customer bonus reward operation context
    /// </summary>
    public class CustomerBonusRewardContext
    {
        /// <summary>
        /// The customer id
        /// </summary>
        public string CustomerId { get; set; }
        
        /// <summary>
        /// The amount to reward
        /// </summary>
        public Money18 Amount { get; set; }
        
        /// <summary>
        /// The customer's wallet address
        /// </summary>
        public string WalletAddress { get; set; }
        
        /// <summary>
        /// The source address for bonuses transfer
        /// </summary>
        public string MinterAddress { get; set; }

        /// <summary>
        /// Id of the operation
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Reason for receiving a bonus
        /// </summary>
        public string BonusReason { get; set; }
        
        /// <summary>
        /// The campaign identifier.
        /// </summary>
        public string CampaignId { get; set; }

        /// <summary>
        /// The condition identifier.
        /// </summary>
        public string ConditionId { get; set; }
    }
}
