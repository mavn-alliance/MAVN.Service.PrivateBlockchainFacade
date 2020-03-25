using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities.Deduplication
{
    [Table("bonus_reward_deduplication_log")]
    public class BonusRewardDeduplicationLogEntity : BaseDeduplicationLogEntity
    {
        internal static BonusRewardDeduplicationLogEntity Create(string deduplicationKey, DateTime retentionStartsAt)
        {
            return new BonusRewardDeduplicationLogEntity
            {
                DeduplicationKey = deduplicationKey,
                RetentionStartsAt = retentionStartsAt
            };            
        }
    }
}
