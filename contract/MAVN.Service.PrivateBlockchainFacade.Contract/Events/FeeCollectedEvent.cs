using Falcon.Numerics;

namespace MAVN.Service.PrivateBlockchainFacade.Contract.Events
{
    public class FeeCollectedEvent
    {
        /// <summary>
        /// Unique identifier of the event
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// Id of the customer
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Wallet address of the customer
        /// </summary>
        public string WalletAddress { get; set; }

        /// <summary>
        /// Fee amount
        /// </summary>
        public Money18 Amount { get; set; }

        /// <summary>
        /// Reason for fee collection
        /// </summary>
        public FeeCollectedReason Reason { get; set; }

        /// <summary>
        /// Hash of the transaction
        /// </summary>
        public string TransactionHash { get; set; }
    }
}
