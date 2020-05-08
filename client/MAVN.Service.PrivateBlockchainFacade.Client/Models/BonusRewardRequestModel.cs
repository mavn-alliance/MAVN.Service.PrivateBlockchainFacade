using JetBrains.Annotations;
using MAVN.Numerics;
using System;
using System.ComponentModel.DataAnnotations;

namespace MAVN.Service.PrivateBlockchainFacade.Client.Models
{
    /// <summary>
    /// Bonus reward request details
    /// </summary>
    [PublicAPI]
    public class BonusRewardRequestModel
    {
        /// <summary>
        /// The customer id to reward bonuses to
        /// </summary>
        [Required]
        public Guid CustomerId { get; set; }

        /// <summary>
        /// The amount of bonuses to reward
        /// </summary>
        public Money18 Amount { get; set; }

        /// <summary>
        /// The reward request id, has to be unique
        /// </summary>
        [Required]
        public string RewardRequestId { get; set; }

        /// <summary>
        /// Reason for receiving the bonus
        /// </summary>
        [Required]
        public string BonusReason { get; set; }

        /// <summary>
        /// The campaign identifier.
        /// </summary>
        [Required]
        public Guid CampaignId { get; set; }

        /// <summary>
        /// The condition identifier.
        /// </summary>
        [Required]
        public Guid ConditionId { get; set; }
    }
}
