using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MAVN.Service.PrivateBlockchainFacade.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class RabbitPublishers
    {
        [AmqpCheck]
        public string CustomerRabbitConnectionString { get; set; }

        [AmqpCheck]
        public string WalletRabbitConnectionString { get; set; }
    }
}
