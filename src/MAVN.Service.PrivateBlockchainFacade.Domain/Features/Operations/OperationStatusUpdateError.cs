namespace MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations
{
    public enum OperationStatusUpdateError
    {
        None,
        OperationNotFound,
        InvalidStatus,
        InvalidTransactionHash,
        DuplicateTransactionHash,
        OperationIdIsNull,
        UnsupportedOperationStatus
    }
}
