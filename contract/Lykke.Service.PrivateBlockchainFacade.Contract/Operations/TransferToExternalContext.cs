using Falcon.Numerics;

namespace Lykke.Service.PrivateBlockchainFacade.Contract.Operations
{
    public class TransferToExternalContext
    {
        public string PrivateWalletAddress { get; set; }

        public string RecipientContractAddress { get; set; }

        public Money18 Amount { get; set; }
    }
}
