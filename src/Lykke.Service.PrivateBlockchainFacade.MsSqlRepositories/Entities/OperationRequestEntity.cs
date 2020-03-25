using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations;

namespace Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities
{
    [Table("operation_requests")]
    public class OperationRequestEntity : IOperationRequest
    {
        [Key] 
        [Column("id")] 
        public Guid Id { get; set; }
        
        [Column("transaction_hash")] 
        public string TransactionHash { get; set; }

        [Column("customer_id")] 
        public string CustomerId { get; set; }

        [Required]
        [Column("master_wallet_address")] 
        public string MasterWalletAddress { get; set; }

        [Required]
        [Column("nonce")]
        public long Nonce { get; set; }

        [Required]
        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        [Required]
        [Column("type")]
        public OperationType Type { get; set; }

        [Required]
        [Column("status")] 
        public OperationStatus Status { get; set; }

        [Required]
        [Column("context_json")] 
        public string ContextJson { get; set; }
        
        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        internal static OperationRequestEntity Create(
            string customerId,
            string masterWalletAddress,
            long nonce,
            OperationType type,
            string contextJson)
        {
            var now = DateTime.UtcNow;

            return new OperationRequestEntity
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                MasterWalletAddress = masterWalletAddress,
                Nonce = nonce,
                Timestamp = now,
                CreatedAt = now,
                Type = type,
                Status = OperationStatus.Created,
                ContextJson = contextJson
            };
        }
    }
}
