using Autofac;
using JetBrains.Annotations;
using Lykke.Common.MsSql;
using Lykke.Service.PrivateBlockchainFacade.Domain.Common;
using Lykke.Service.PrivateBlockchainFacade.Domain.Deduplication;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories;
using Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Contexts;
using Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities.Deduplication;
using Lykke.Service.PrivateBlockchainFacade.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.PrivateBlockchainFacade.Modules
{
    [UsedImplicitly]
    public class DataModule : Module
    {
        private readonly DbSettings _dbSettings;

        private const int DefaultCommandTimeoutSeconds = 30;

        public DataModule(IReloadingManager<AppSettings> appSettings)
        {
            _dbSettings = appSettings.CurrentValue.PrivateBlockchainFacadeService.Db;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMsSql(
                _dbSettings.DataConnString,
                connString => new PbfContext(connString, false, _dbSettings.CommandTimeoutSeconds ?? DefaultCommandTimeoutSeconds),
                dbConn => new PbfContext(dbConn));

            builder.RegisterType<SqlRepositoryHelper>()
                .As<ISqlRepositoryHelper>()
                .SingleInstance();

            builder.RegisterType<OperationsRepository>()
                .As<IOperationsRepository>()
                .SingleInstance();

            builder.RegisterType<OperationRequestsRepository>()
                .As<IOperationRequestsRepository>()
                .SingleInstance();

            builder.RegisterType<WalletOwnersRepository>()
                .As<IWalletOwnersRepository>()
                .SingleInstance();
            
            builder.RegisterType<BonusRewardDeduplicationLogRepository>()
                .As<IDeduplicationLogRepository<BonusRewardDeduplicationLogEntity>>()
                .SingleInstance();
            
            builder.RegisterType<TransferDeduplicationLogRepository>()
                .As<IDeduplicationLogRepository<TransferDeduplicationLogEntity>>()
                .SingleInstance();

            builder.RegisterType<OperationDeduplicationLogRepository>()
                .As<IDeduplicationLogRepository<OperationDeduplicationLogEntity>>()
                .SingleInstance();

            builder.RegisterType<WalletLinkingDeduplicationLogRepository>()
                .As<IDeduplicationLogRepository<WalletLinkingDeduplicationLogEntity>>()
                .SingleInstance();
        }
    }
}
