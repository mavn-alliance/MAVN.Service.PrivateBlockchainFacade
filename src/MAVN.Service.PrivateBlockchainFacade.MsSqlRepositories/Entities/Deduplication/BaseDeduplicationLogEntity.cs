using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MAVN.Service.PrivateBlockchainFacade.Domain.Common;
using MAVN.Service.PrivateBlockchainFacade.Domain.Deduplication;

namespace MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities.Deduplication
{
    /// <summary>
    /// Base class for all entities to be used as deduplication log entries
    /// </summary>
    public abstract class BaseDeduplicationLogEntity : IDeduplicatable, IRetentionable
    {
        [Key] 
        [Column("deduplication_key")]
        [Required]
        public string DeduplicationKey { get; set; }
        
        [Column("retention_starts_at")]
        [Required]
        public DateTime RetentionStartsAt { get; set; }
    }
}
