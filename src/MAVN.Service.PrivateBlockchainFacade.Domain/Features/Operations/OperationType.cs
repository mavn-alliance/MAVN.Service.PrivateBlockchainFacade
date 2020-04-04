namespace MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations
{
    public enum OperationType
    {
        CustomerWalletCreation,
        CustomerBonusReward,
        TokensTransfer,
        GenericOperation,
        StakeOperation,
        WalletLinking,
        WalletUnlinking,
        TransferToExternal,
        TransferToInternal,
        SetTransferToPublicFee,
        SeizeToInternal
    }
}
