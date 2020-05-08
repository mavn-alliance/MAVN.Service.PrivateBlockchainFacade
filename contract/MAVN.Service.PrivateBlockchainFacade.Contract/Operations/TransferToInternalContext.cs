using System.Numerics;
using System.Reflection;
using MAVN.Numerics;

namespace MAVN.Service.PrivateBlockchainFacade.Contract.Operations
{
    public class TransferToInternalContext
    {
        public string PublicWalletAddress { get; set; }

        public string PrivateWalletAddress { get; set; }

        public Money18 Amount { get; set; }

        public BigInteger PublicTransferId { get; set; }

    }
}
