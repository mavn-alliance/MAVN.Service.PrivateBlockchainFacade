using JetBrains.Annotations;

namespace MAVN.Service.PrivateBlockchainFacade.Client.Models
{
    /// <summary>
    /// Bonus reward response
    /// </summary>
    [PublicAPI]
    public class BonusRewardResponseModel
    {
        /// <summary>
        /// Bonuses reward error
        /// </summary>
        public BonusRewardError Error { get; set; }
    }
}
