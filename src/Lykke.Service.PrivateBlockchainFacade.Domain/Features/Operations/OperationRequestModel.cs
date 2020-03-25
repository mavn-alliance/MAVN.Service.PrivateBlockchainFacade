using System;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations
{
    public class OperationRequestModel
    {
        public Guid Id { get; set; }
        
        public string MasterWalletAddress { get; set; }
        
        public long Nonce { get; set; }
        
        public OperationType Type { get; set; }
        
        public string PayloadJson { get; set; }
    }
}
