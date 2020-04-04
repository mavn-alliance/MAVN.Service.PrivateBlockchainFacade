using System.ComponentModel.DataAnnotations;
using Falcon.Numerics;

namespace MAVN.Service.PrivateBlockchainFacade.Client.Models
{
    /// <summary>
    /// Request model
    /// </summary>
    public class SetTransferToPublicFeeRequestModel
    {
        /// <summary>
        /// The fee amount
        /// </summary>
        [Required]
        public Money18 Fee { get; set; }
    }
}
