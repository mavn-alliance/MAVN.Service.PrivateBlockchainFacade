using System;
using JetBrains.Annotations;
using Falcon.Numerics;

namespace Lykke.Service.PrivateBlockchainFacade.Contract.Events
{
    /// <summary>
    /// Raised upon customer's balance update
    /// </summary>
    [PublicAPI]
    public class CustomerBalanceUpdatedEvent
    {
        /// <summary>
        /// Id of the customer whose balance has been updated
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// The new value of the customer's balance
        /// </summary>
        public Money18 Balance { get; set; }

        /// <summary>
        /// Timestamp of the event
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// The reason for balance update
        /// </summary>
        public CustomerBalanceUpdateReason Reason { get; set; }
        
        /// <summary>
        /// The internal operation id which caused the balance update
        /// </summary>
        public Guid OperationId { get; set; }
    }
}
