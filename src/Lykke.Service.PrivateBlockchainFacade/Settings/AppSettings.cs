using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.QuorumOperationExecutor.Client;
using Lykke.Service.QuorumTransactionSigner.Client;

namespace Lykke.Service.PrivateBlockchainFacade.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public PrivateBlockchainFacadeSettings PrivateBlockchainFacadeService { get; set; }
        
        public QuorumTransactionSignerServiceClientSettings QuorumTransactionSignerService { get; set; }

        public QuorumOperationExecutorServiceClientSettings QuorumOperationExecutorService { get; set; }
        
        
    }
}
