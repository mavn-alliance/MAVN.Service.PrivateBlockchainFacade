using Autofac;
using JetBrains.Annotations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Common;
using MAVN.Service.PrivateBlockchainFacade.Domain.Deduplication;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Contexts;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities.Deduplication;
using MAVN.Service.PrivateBlockchainFacade.Settings;
using Lykke.SettingsReader;
using MAVN.Persistence.PostgreSQL.Legacy;

namespace MAVN.Service.PrivateBlockchainFacade.Modules
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
            builder.RegisterPostgreSQL(
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
