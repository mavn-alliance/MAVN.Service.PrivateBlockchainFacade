using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PrivateBlockchainFacade.Client.Models
{
    /// <summary>
    /// request model for generic operations
    /// </summary>
    public class GenericOperationRequest
    {
        /// <summary>
        /// Id of the operation
        /// </summary>
        public string OperationId { get; set; }
        /// <summary>
        /// Additional data for the operation
        /// </summary>
        [Required]
        public string Data { get; set; }
        
        /// <summary>
        /// Wallet address of the source
        /// </summary>
        [Required]
        public string SourceAddress { get; set; }

        /// <summary>
        /// Wallet address of the target
        /// </summary>
        [Required]
        public string TargetAddress { get; set; }
    }
}
