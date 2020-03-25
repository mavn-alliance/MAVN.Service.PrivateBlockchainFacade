namespace Lykke.Service.PrivateBlockchainFacade.Domain.Features.Wallets
{
    public interface IWalletOwner
    {
        string OwnerId { get; set; }
        
        string WalletId { get; set; }
    }
}
