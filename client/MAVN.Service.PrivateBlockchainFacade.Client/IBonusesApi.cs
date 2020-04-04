using System.Threading.Tasks;
using JetBrains.Annotations;
using MAVN.Service.PrivateBlockchainFacade.Client.Models;
using Refit;

namespace MAVN.Service.PrivateBlockchainFacade.Client
{
    /// <summary>
    /// Bonuses API interface
    /// </summary>
    [PublicAPI]
    public interface IBonusesApi
    {
        /// <summary>
        /// Reward customer's account with bonuses
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Post("/api/bonuses")]
        Task<BonusRewardResponseModel> RewardAsync([Body] BonusRewardRequestModel request);
    }
}
