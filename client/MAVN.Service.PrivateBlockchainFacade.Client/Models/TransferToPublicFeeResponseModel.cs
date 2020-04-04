using Falcon.Numerics;

namespace MAVN.Service.PrivateBlockchainFacade.Client.Models
{
    /// <summary>
    /// response model
    /// </summary>
    public class TransferToPublicFeeResponseModel
    {
        /// <summary>
        /// The fee amount
        /// </summary>
        public Money18 Fee { get; set; }
    }
}
