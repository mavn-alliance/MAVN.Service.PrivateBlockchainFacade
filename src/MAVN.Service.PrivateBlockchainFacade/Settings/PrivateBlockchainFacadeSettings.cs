using System;
using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MAVN.Service.PrivateBlockchainFacade.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class PrivateBlockchainFacadeSettings
    {
        public DbSettings Db { get; set; }

        public BlockchainSettings Blockchain { get; set; }

        public string MasterWalletAddress { get; set; }

        public int MaxNewOperationsAmountPerRequest { get; set; }

        public int MaxAcceptedOperationsAmountPerRequest { get; set; }

        public TimeSpan BalancesCacheExpirationPeriod { get; set; }

        public RabbitSettings Rabbit { set; get; }

        public DistributedCacheSettings CacheSettings { get; set; }

        [Optional]
        public bool? IsPublicBlockchainFeatureDisabled { get; set; }

        public string ApiKey { get; set; }
    }
}
