using JetBrains.Annotations;
using Falcon.Numerics;

namespace MAVN.Service.PrivateBlockchainFacade.Client.Models
{
    /// <summary>
    /// Customer balance response
    /// </summary>
    [PublicAPI]
    public class CustomerBalanceResponseModel
    {
        /// <summary>
        /// The total available balance
        /// </summary>
        public Money18 Total { get; set; }

        /// <summary>
        /// Total staked balance
        /// </summary>
        public Money18 Staked { get; set; }

        /// <summary>
        /// Customer balance error
        /// </summary>
        public CustomerBalanceError Error { get; set; }
    }
}
