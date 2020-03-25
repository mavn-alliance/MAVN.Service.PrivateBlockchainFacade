using System;

namespace Lykke.Service.PrivateBlockchainFacade.Client.Models
{
    /// <summary>
    /// Response model for generic Operation
    /// </summary>
    public class GenericOperationResponse
    {
        /// <summary>
        /// Id of the operation
        /// </summary>
        public Guid OperationId { get; set; }

        /// <summary>
        /// Error code
        /// </summary>
        public AddOperationError Error { get; set; }
    }
}
