using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using MAVN.Service.QuorumOperationExecutor.Client;
using MAVN.Service.QuorumTransactionSigner.Client;

namespace MAVN.Service.PrivateBlockchainFacade.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public PrivateBlockchainFacadeSettings PrivateBlockchainFacadeService { get; set; }
        
        public QuorumTransactionSignerServiceClientSettings QuorumTransactionSignerService { get; set; }

        public QuorumOperationExecutorServiceClientSettings QuorumOperationExecutorService { get; set; }
        
        
    }
}
