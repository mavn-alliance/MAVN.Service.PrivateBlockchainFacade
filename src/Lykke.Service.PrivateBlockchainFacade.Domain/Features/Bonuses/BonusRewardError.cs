namespace Lykke.Service.PrivateBlockchainFacade.Domain.Features.Bonuses
{
    public enum BonusRewardError
    {
        None,

        DuplicateRequest,

        InvalidCustomerId,

        InvalidAmount,

        CustomerWalletMissing,

        MissingBonusReason,

        InvalidCampaignId
    }
}
