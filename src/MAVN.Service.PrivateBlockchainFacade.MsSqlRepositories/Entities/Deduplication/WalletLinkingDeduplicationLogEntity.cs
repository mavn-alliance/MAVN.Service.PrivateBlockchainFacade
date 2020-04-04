using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities.Deduplication
{
    [Table("wallet_linking_deduplication_log")]
    public class WalletLinkingDeduplicationLogEntity : BaseDeduplicationLogEntity
    {
        internal static WalletLinkingDeduplicationLogEntity Create(string deduplicationKey, DateTime retentionStartsAt)
        {
            return new WalletLinkingDeduplicationLogEntity
            {
                DeduplicationKey = deduplicationKey,
                RetentionStartsAt = retentionStartsAt
            };
        }
    }
}
