using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.PrivateBlockchainFacade.Settings
{
    public class RabbitSubscribers
    {
        [AmqpCheck]
        public string PrivateBlockchainRabbitConnString { get; set; }
    }
}
