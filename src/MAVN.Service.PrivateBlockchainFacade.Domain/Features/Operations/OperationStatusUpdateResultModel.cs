namespace MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations
{
    public class OperationStatusUpdateResultModel
    {
        public OperationStatusUpdateError Error { get; private set; }
        
        public static OperationStatusUpdateResultModel Succeeded()
        {
            return new OperationStatusUpdateResultModel
            {
                Error = OperationStatusUpdateError.None
            };
        }

        public static OperationStatusUpdateResultModel Failed(OperationStatusUpdateError error)
        {
            return new OperationStatusUpdateResultModel
            {
                Error = error
            };       
        }
    }
}
