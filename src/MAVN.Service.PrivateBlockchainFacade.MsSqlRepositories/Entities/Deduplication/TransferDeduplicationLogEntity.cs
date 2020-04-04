using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities.Deduplication
{
    [Table("transfer_deduplication_log")]
    public class TransferDeduplicationLogEntity : BaseDeduplicationLogEntity
    {
        internal static TransferDeduplicationLogEntity Create(string deduplicationKey, DateTime retentionStartsAt)
        {
            return new TransferDeduplicationLogEntity
            {
                DeduplicationKey = deduplicationKey,
                RetentionStartsAt = retentionStartsAt
            };            
        }
    }
}
