using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;

namespace MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities
{
    [Table("operations")]
    public class OperationEntity : IOperation
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

        internal static OperationEntity Create(
            Guid id,
            string customerId, 
            string masterWalletAddress, 
            long nonce, 
            OperationType type,
            string contextJson,
            DateTime createdAt,
            string transactionHash)
        {
            return new OperationEntity
            {
                Id = id,
                CustomerId = customerId,
                MasterWalletAddress = masterWalletAddress,
                Nonce = nonce,
                Timestamp = DateTime.UtcNow,
                CreatedAt = createdAt,
                Type = type,
                // Operations can be created only in Accepted state
                Status = OperationStatus.Accepted,
                // Since operation is in Accepted state the transaction hash is required
                TransactionHash = transactionHash,
                ContextJson = contextJson
            };
        }

        internal static OperationEntity Create(IOperationRequest request, string txHash)
        {
            return new OperationEntity
            {
                Id = request.Id,
                CustomerId = request.CustomerId,
                MasterWalletAddress = request.MasterWalletAddress,
                Nonce = request.Nonce,
                Timestamp = DateTime.UtcNow,
                CreatedAt = request.CreatedAt,
                Type = request.Type,
                // Operations can be created only in Accepted state
                Status = OperationStatus.Accepted,
                // Since operation is in Accepted state the transaction hash is required
                TransactionHash = txHash,
                ContextJson = request.ContextJson
            };
        }
    }
}
