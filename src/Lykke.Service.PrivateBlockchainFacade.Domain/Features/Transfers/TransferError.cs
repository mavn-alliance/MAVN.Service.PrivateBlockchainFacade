namespace Lykke.Service.PrivateBlockchainFacade.Domain.Features.Transfers
{
    public enum TransferError
    {
        None,
        InvalidSenderId,
        InvalidRecipientId,
        SenderWalletMissing,
        RecipientWalletMissing,
        InvalidAmount,
        NotEnoughFunds,
        DuplicateRequest,
        InvalidAdditionalDataFormat,
        PrivateWalletMissing,
        PublicWalletMissing,
        InvalidFeeAmount
    }
}
