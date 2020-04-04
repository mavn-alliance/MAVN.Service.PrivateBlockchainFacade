using System;
using System.ComponentModel.DataAnnotations;
using Falcon.Numerics;

namespace MAVN.Service.PrivateBlockchainFacade.Client.Models
{
    /// <summary>
    /// Generic transfer request
    /// </summary>
    public class GenericTransferRequestModel
    {
        /// <summary>
        /// Sender's customer id
        /// </summary>
        [Required]
        public string SenderCustomerId { get; set; }

        /// <summary>
        /// Recipient's blockchain wallet address
        /// </summary>
        [Required]
        public string RecipientAddress { get; set; }

        /// <summary>
        /// Transfer amount
        /// </summary>
        public Money18 Amount { get; set; }

        /// <summary>
        /// The transfer request id, has to be unique
        /// </summary>
        public string TransferId { get; set; }

        /// <summary>
        /// Additional data which would be passed to blockchain
        /// </summary>
        public string AdditionalData { get; set; }
    }
}
