using System;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.Features.Wallets
{
    public class WalletOwnerDuplicateException : Exception
    {
        // Parameterless constructor is required for unit tests
        public WalletOwnerDuplicateException()
        {
        }
        
        public WalletOwnerDuplicateException(string ownerId, string walletId, string message = null): base(message ?? "Can't create new wallet owner because it has duplicate")
        {
            OwnerId = ownerId;
            WalletId = walletId;
        }
        
        public string OwnerId { get; }
        public string WalletId { get; }
    }
}
