using Lykke.SettingsReader.Attributes;

namespace MAVN.Service.PrivateBlockchainFacade.Settings
{
    public class RabbitSubscribers
    {
        [AmqpCheck]
        public string PrivateBlockchainRabbitConnString { get; set; }
    }
}
