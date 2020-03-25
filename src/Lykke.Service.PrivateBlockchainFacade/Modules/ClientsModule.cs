using Autofac;
using JetBrains.Annotations;
using Lykke.Service.PrivateBlockchainFacade.Settings;
using Lykke.Service.QuorumOperationExecutor.Client;
using Lykke.Service.QuorumTransactionSigner.Client;
using Lykke.SettingsReader;

namespace Lykke.Service.PrivateBlockchainFacade.Modules
{
    [UsedImplicitly]
    public class ClientsModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ClientsModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterQuorumTransactionSignerClient(_appSettings.CurrentValue.QuorumTransactionSignerService, null);
            
            builder.RegisterQuorumOperationExecutorClient(_appSettings.CurrentValue.QuorumOperationExecutorService, null);
        }
    }
}
