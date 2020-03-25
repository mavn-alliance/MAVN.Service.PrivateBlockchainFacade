using System;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations
{
    public interface IOperation
    {
        Guid Id { get; set; }
        
        string TransactionHash { get; set; }
        
        string CustomerId { get; set; }
        
        string MasterWalletAddress { get; set; }
        
        long Nonce { get; set; }
        
        DateTime Timestamp { get; set; }

        OperationType Type { get; set; }
        
        OperationStatus Status { get; set; }
        
        string ContextJson { get; set; }
        
        DateTime CreatedAt { get; set; }
    }
}
