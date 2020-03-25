using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities.Deduplication
{
    [Table("operation_deduplication_log")]
    public class OperationDeduplicationLogEntity : BaseDeduplicationLogEntity
    {
        internal static OperationDeduplicationLogEntity Create(string deduplicationKey, DateTime retentionStartsAt)
        {
            return new OperationDeduplicationLogEntity
            {
                DeduplicationKey = deduplicationKey,
                RetentionStartsAt = retentionStartsAt
            };
        }
    }
}
