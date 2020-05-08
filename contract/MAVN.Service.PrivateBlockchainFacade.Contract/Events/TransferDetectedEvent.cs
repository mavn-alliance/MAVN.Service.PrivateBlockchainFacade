using System;
using JetBrains.Annotations;
using MAVN.Numerics;

namespace MAVN.Service.PrivateBlockchainFacade.Contract.Events
{
    /// <summary>
    /// Event which is raised when transfer has been completed
    /// </summary>
    [PublicAPI]
    public class TransferDetectedEvent
    {
        /// <summary>
        /// The transfer transaction hash
        /// </summary>
        public string TransactionHash { get; set; }
        
        /// <summary>
        /// The sender customer id
        /// </summary>
        public string SenderCustomerId { get; set; }
        
        /// <summary>
        /// The recipient customer id
        /// </summary>
        public string ReceiverCustomerId { get; set; }
        
        /// <summary>
        /// The amount of transfer
        /// </summary>
        public Money18 Amount { get; set; }
        
        /// <summary>
        /// The time of the event
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Id of the operation
        /// </summary>
        public string RequestId { get; set; }
    }
}
