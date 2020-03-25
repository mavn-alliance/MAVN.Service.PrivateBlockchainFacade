using System;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations
{
    public class GenericOperationResultModel
    {
        public AddOperationError Error { get; set; }

        public Guid OperationId { get; set; }

        public static GenericOperationResultModel Succeeded(Guid operationId)
        {
            return new GenericOperationResultModel
            {
                Error = AddOperationError.None,
                OperationId = operationId
            };
        }

        public static GenericOperationResultModel Failed(AddOperationError error)
        {
            return new GenericOperationResultModel
            {
                Error = error,
            };
        }
    }
}
