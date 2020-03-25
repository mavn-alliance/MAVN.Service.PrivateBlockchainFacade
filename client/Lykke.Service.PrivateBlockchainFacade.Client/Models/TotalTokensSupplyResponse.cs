using JetBrains.Annotations;
using Falcon.Numerics;

namespace Lykke.Service.PrivateBlockchainFacade.Client.Models
{
    /// <summary>
    /// Response model which holds total tokens amount
    /// </summary>
    [PublicAPI]
    public class TotalTokensSupplyResponse
    {
        /// <summary>
        /// Total tokens amount
        /// </summary>
        public Money18 TotalTokensAmount { get; set; }
    }
}
