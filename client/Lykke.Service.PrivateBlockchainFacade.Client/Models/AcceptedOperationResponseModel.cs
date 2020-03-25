using System;
using JetBrains.Annotations;

namespace Lykke.Service.PrivateBlockchainFacade.Client.Models
{
    /// <summary>
    /// Accepted operation response model
    /// </summary>
    [PublicAPI]
    public class AcceptedOperationResponseModel
    {
        /// <summary>
        /// The operation id
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// The transaction hash
        /// </summary>
        public string TransactionHash { get; set; }
        
        /// <summary>
        /// Master wallet address which holds the operation
        /// </summary>
        public string MasterWalletAddress { get; set; }
        
        /// <summary>
        /// Unique nonce number
        /// </summary>
        public long Nonce { get; set; }

        /// <summary>
        /// The operation type
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// The payload serialized into JSON
        /// </summary>
        public string PayloadJson { get; set; }
        
        /// <summary>
        /// The date and time of the latest operation update
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
