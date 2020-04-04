using System;

namespace MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations
{
    public class AcceptedOperationModel
    {
        public Guid Id { get; set; }
        
        public string TransactionHash { get; set; }
        
        public string MasterWalletAddress { get; set; }
        
        public long Nonce { get; set; }
        
        public OperationType Type { get; set; }
        
        public string PayloadJson { get; set; }
        
        public DateTime Timestamp { get; set; }
    }
}
