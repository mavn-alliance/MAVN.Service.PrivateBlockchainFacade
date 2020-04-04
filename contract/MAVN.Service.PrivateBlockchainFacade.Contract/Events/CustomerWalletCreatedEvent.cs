using System;
using JetBrains.Annotations;

namespace MAVN.Service.PrivateBlockchainFacade.Contract.Events
{
    /// <summary>
    /// Customer wallet registered event, being fired upon successful wallet registration
    /// </summary>
    [PublicAPI]
    public class CustomerWalletCreatedEvent
    {
        /// <summary>
        /// The customer id
        /// </summary>
        public Guid CustomerId { get; set; }
        
        /// <summary>
        /// The event timestamp
        /// </summary>
        public DateTime Timestamp { get; set; } 
    }
}
