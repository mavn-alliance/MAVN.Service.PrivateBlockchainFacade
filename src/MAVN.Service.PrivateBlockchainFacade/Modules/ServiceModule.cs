using Autofac;
using JetBrains.Annotations;
using Lykke.Sdk;
using MAVN.Service.PrivateBlockchainFacade.Auth;
using MAVN.Service.PrivateBlockchainFacade.Domain.Common;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Balances;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Bonuses;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Fees;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Transfers;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq;
using MAVN.Service.PrivateBlockchainFacade.Domain.Services;
using MAVN.Service.PrivateBlockchainFacade.DomainServices;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.Common;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.Features.Balances;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.Features.Bonuses;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.Features.Fees;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.Features.Transfers;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.Features.Wallets;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers;
using MAVN.Service.PrivateBlockchainFacade.Managers;
using MAVN.Service.PrivateBlockchainFacade.Settings;
using Lykke.SettingsReader;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;

namespace MAVN.Service.PrivateBlockchainFacade.Modules
{
    [UsedImplicitly]
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<WalletsService>()
                .As<IWalletsService>();
            
            builder.RegisterType<BonusService>()
                .WithParameter(TypedParameter.From(_appSettings.CurrentValue.PrivateBlockchainFacadeService.MasterWalletAddress))
                .As<IBonusService>();

            builder.RegisterType<OperationRequestsProducer>()
                .WithParameter(TypedParameter.From(_appSettings.CurrentValue.PrivateBlockchainFacadeService.MasterWalletAddress))
                .As<IOperationRequestsProducer>();

            builder.RegisterType<OperationsFetcher>()
                .WithParameter("maxNewOperationsAmount", _appSettings.CurrentValue.PrivateBlockchainFacadeService.MaxNewOperationsAmountPerRequest)
                .WithParameter("maxAcceptedOperationsAmount", _appSettings.CurrentValue.PrivateBlockchainFacadeService.MaxAcceptedOperationsAmountPerRequest)
                .As<IOperationsFetcher>();

            builder.RegisterType<OperationStatusUpdater>()
                .As<IOperationStatusUpdater>();

            builder.RegisterType<TransactionScopeHandler>()
                .As<ITransactionScopeHandler>()
                .InstancePerLifetimeScope();
            
            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();
            
            builder.RegisterType<CustomerRegisteredInBlockchainHandler>()
                .As<ICustomerRegisteredInBlockchainHandler>()
                .SingleInstance();

            builder.RegisterType<TransactionFailedInBlockchainHandler>()
                .As<ITransactionFailedInBlockchainHandler>()
                .SingleInstance();

            builder.RegisterType<TokensService>()
                .As<ITokensService>()
                .WithParameter(TypedParameter.From(_appSettings.CurrentValue.PrivateBlockchainFacadeService.Blockchain.PrivateBlockchainGatewayContractAddress))
                .SingleInstance();
            
            builder.RegisterType<BalanceService>()
                .WithParameter(TypedParameter.From(_appSettings.CurrentValue.PrivateBlockchainFacadeService.BalancesCacheExpirationPeriod))
                .As<IBalanceService>()
                .SingleInstance();

            builder.RegisterType<MintEventHandler>()
                .As<IMintEventHandler>()
                .SingleInstance();

            builder.RegisterType<SeizeToInternalDetectedHandler>()
                .As<ISeizeToInternalDetectedHandler>()
                .WithParameter("privateBlockchainGatewayContractAddress",
                    _appSettings.CurrentValue.PrivateBlockchainFacadeService.Blockchain
                        .PrivateBlockchainGatewayContractAddress)
                .SingleInstance();

            builder.RegisterType<TransferEventHandler>()
                .As<ITransferEventHandler>()
                .SingleInstance();

            builder.RegisterType<StakedBalanceChangedHandler>()
                .As<IStakedBalanceChangedHandler>()
                .SingleInstance();

            builder.RegisterType<TransferService>()
                .As<ITransferService>();

            builder.RegisterType<TransactionCompletedInBlockchainHandler>()
                .As<ITransactionCompletedInBlockchainHandler>()
                .SingleInstance();
            
            RegisterDistributedCache(builder);

            builder.RegisterType<ApiKeyService>()
                .WithParameter(TypedParameter.From(_appSettings.CurrentValue.PrivateBlockchainFacadeService.ApiKey))
                .As<IApiKeyService>();

            builder.RegisterType<TransferToExternalRequestedHandler>()
                .As<ITransferToExternalRequestedHandler>()
                .SingleInstance();

            builder.RegisterType<TransferToInternalDetectedHandler>()
                .As<ITransferToInternalDetectedHandler>()
                .SingleInstance();

            builder.RegisterType<OperationsService>()
                .As<IOperationsService>()
                .SingleInstance();

            builder.RegisterType<WalletLinkingStatusChangeRequestedHandler>()
                .As<IWalletLinkingStatusChangeRequestedHandler>()
                .SingleInstance();

            builder.RegisterType<FeesService>()
                .As<IFeesService>()
                .SingleInstance();

            builder.RegisterType<FeeCollectedHandler>()
                .As<IFeeCollectedHandler>()
                .SingleInstance();

            builder.RegisterType<CustomerProfileDeactivationRequestedHandler>()
                .As<ICustomerProfileDeactivationRequestedHandler>()
                .SingleInstance();

            builder.RegisterType<SeizedFromHandler>()
                .As<ISeizedFromHandler>()
                .SingleInstance();
        }

        private void RegisterDistributedCache(ContainerBuilder builder)
        {
            var redis = new RedisCache(new RedisCacheOptions
            {
                Configuration = _appSettings.CurrentValue.PrivateBlockchainFacadeService.CacheSettings.RedisConfiguration,
                InstanceName = _appSettings.CurrentValue.PrivateBlockchainFacadeService.CacheSettings.DataCacheInstance
            });
            
            builder.RegisterInstance(redis).As<IDistributedCache>().SingleInstance();
        }
    }
}
