using System;
using JetBrains.Annotations;
using MAVN.Numerics;

namespace MAVN.Service.PrivateBlockchainFacade.Contract.Events
{
    /// <summary>
    /// Event which is raised when a customer receives a bonus reward
    /// </summary>
    [PublicAPI]
    public class BonusRewardDetectedEvent
    {
        /// <summary>
        /// Id of the customer who received the bonus
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Amount of tokens that were received as a bonus
        /// </summary>
        public Money18 Amount { get; set; }

        /// <summary>
        /// Reason why bonus was received
        /// </summary>
        public string BonusReason { get; set; }

        /// <summary>
        /// Id of the operation
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// The campaign identifier.
        /// </summary>
        public string CampaignId { get; set; }

        /// <summary>
        /// The condition identifier.
        /// </summary>
        public string ConditionId { get; set; }

        /// <summary>
        /// Timestamp of the event
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
