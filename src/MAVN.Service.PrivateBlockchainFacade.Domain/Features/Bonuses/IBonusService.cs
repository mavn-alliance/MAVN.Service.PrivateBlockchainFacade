using System.Threading.Tasks;
using Falcon.Numerics;

namespace MAVN.Service.PrivateBlockchainFacade.Domain.Features.Bonuses
{
    public interface IBonusService
    {
        Task<BonusRewardResultModel> RewardAsync(string customerId, Money18 amount, string rewardId, string bonusReason,
            string campaignId, string conditionId);
    }
}
