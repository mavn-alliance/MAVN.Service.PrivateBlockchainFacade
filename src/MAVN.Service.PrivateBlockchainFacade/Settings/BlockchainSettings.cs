using JetBrains.Annotations;

namespace MAVN.Service.PrivateBlockchainFacade.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class BlockchainSettings
    {
        public string PrivateBlockchainGatewayContractAddress { get; set; }
    }
}
