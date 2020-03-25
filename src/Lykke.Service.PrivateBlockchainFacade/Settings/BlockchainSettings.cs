using JetBrains.Annotations;

namespace Lykke.Service.PrivateBlockchainFacade.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class BlockchainSettings
    {
        public string PrivateBlockchainGatewayContractAddress { get; set; }
    }
}
