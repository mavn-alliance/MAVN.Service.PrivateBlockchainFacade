using System;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.Features.Transfers
{
    public class TransferResultModel
    {
        public TransferError Error { get; set; }

        public Guid OperationId { get; set; }
        
        public static TransferResultModel Succeeded(Guid operationId)
        {
            return new TransferResultModel
            {
                Error = TransferError.None,
                OperationId = operationId
            };
        }

        public static TransferResultModel Failed(TransferError error)
        {
            return new TransferResultModel
            {
                Error = error,
            };       
        }
    }
}
