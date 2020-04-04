using System;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace MAVN.Service.PrivateBlockchainFacade.Client.Models
{
    /// <summary>
    /// Customer wallet creation details
    /// </summary>
    [PublicAPI]
    public class CustomerWalletCreationRequestModel
    {
        /// <summary>
        /// The customer id
        /// </summary>
        [Required]
        public Guid CustomerId { get; set; }
    }
}
