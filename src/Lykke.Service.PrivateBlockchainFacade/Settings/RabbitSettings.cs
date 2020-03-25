using JetBrains.Annotations;

namespace Lykke.Service.PrivateBlockchainFacade.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class RabbitSettings
    {
        public RabbitPublishers Publishers { get; set; }
        public RabbitSubscribers Subscribers { get; set; }
    }
}
