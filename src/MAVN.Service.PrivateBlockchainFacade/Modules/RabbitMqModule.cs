using Autofac;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.Job.QuorumTransactionWatcher.Contract;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.PrivateBlockchainFacade.Contract.Events;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Subscribers;
using MAVN.Service.PrivateBlockchainFacade.Settings;
using Lykke.SettingsReader;
using FeeCollectedEvent = Lykke.Service.PrivateBlockchainFacade.Contract.Events.FeeCollectedEvent;

namespace MAVN.Service.PrivateBlockchainFacade.Modules
{
    [UsedImplicitly]
    public class RabbitMqModule : Module
    {
        private const string TransferToExternalRequestedEventExchangeName = "lykke.wallet.transfertoexternalrequested";
        private const string TransferToInternalDetectedExchangeName = "lykke.wallet.transfertointernaldetected";
        private const string WalletLinkingStatusChangeRequestedExchangeName = "lykke.wallet.walletlinkingstatuschangerequested";
        private const string SeizeToInternalDetectedExchangeName = "lykke.wallet.seizetointernaldetected";
        private const string TransferToExternalFailedExchange = "lykke.wallet.transfertoexternalfailed";

        private const string DefaultQueueName = "privateblockchainfacade";
        private const string CustomerProfileDeactivationRequestedExchangeName = "lykke.customer.profiledeactivationrequested";
        private const string CustomerWalletCreatedExchangeName = "lykke.customer.walletcreated";
        private const string BonusRewardDetectedExchangeName = "lykke.wallet.bonusrewarddetected";
        private const string TransferDetectedExchange = "lykke.wallet.transferdetected";
        private const string CustomerBalanceUpdateExchange = "lykke.wallet.customerbalanceupdated";
        private const string P2PTransferFailedExchange = "lykke.wallet.p2ptransferfailed";
        private const string P2PTransferDetectedExchange = "lykke.wallet.p2ptransferdetected";
        private const string TxFailedExchange = "lykke.wallet.transactionfailed";
        private const string TxSucceededExchange = "lykke.wallet.transactionsucceeded";
        private const string WalletStatusChangedExchange = "lykke.wallet.walletstatuschangefailed";
        private const string FeeCollectedExchange = "lykke.wallet.feecollected";
        private const string SeizedFromCustomerExchange = "lykke.wallet.seizedfromcustomer";
        private const string SeizeFromCustomerCompletedExchange = "lykke.wallet.seizebalancefromcustomercompleted";

        private readonly RabbitSettings _rabbitSettings;
        private readonly bool _isPublicBlockChainFeatureDisabled;

        public RabbitMqModule(IReloadingManager<AppSettings> reloadingManager)
        {
            var appSettings = reloadingManager.CurrentValue;
            _rabbitSettings = appSettings.PrivateBlockchainFacadeService.Rabbit;

            _isPublicBlockChainFeatureDisabled = appSettings.PrivateBlockchainFacadeService.IsPublicBlockchainFeatureDisabled
                ?? false;
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterSubscribers(builder);

            RegisterPublishers(builder);
        }

        private void RegisterSubscribers(ContainerBuilder builder)
        {
            builder.RegisterType<CustomerRegistrationInBlockchainSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _rabbitSettings.Subscribers.PrivateBlockchainRabbitConnString)
                .WithParameter("exchangeName", Context.GetEndpointName<CustomerRegisteredInBlockchainEvent>())
                .WithParameter("queueName", DefaultQueueName);

            builder.RegisterType<TransactionFailedInBlockchainSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _rabbitSettings.Subscribers.PrivateBlockchainRabbitConnString)
                .WithParameter("exchangeName", Context.GetEndpointName<TransactionFailedInBlockchainEvent>())
                .WithParameter("queueName", DefaultQueueName);

            builder.RegisterType<MintSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _rabbitSettings.Subscribers.PrivateBlockchainRabbitConnString)
                .WithParameter("exchangeName", Context.GetEndpointName<MintEvent>())
                .WithParameter("queueName", DefaultQueueName);

            builder.RegisterType<TransferSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _rabbitSettings.Subscribers.PrivateBlockchainRabbitConnString)
                .WithParameter("exchangeName", Context.GetEndpointName<TransferEvent>())
                .WithParameter("queueName", DefaultQueueName);

            builder.RegisterType<TransactionCompletedInBlockchainSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _rabbitSettings.Subscribers.PrivateBlockchainRabbitConnString)
                .WithParameter("exchangeName", Context.GetEndpointName<TransactionCompletedInBlockchainEvent>())
                .WithParameter("queueName", DefaultQueueName);

            builder.RegisterType<StakeIncreasedSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _rabbitSettings.Subscribers.PrivateBlockchainRabbitConnString)
                .WithParameter("exchangeName", Context.GetEndpointName<StakeIncreasedEvent>())
                .WithParameter("queueName", DefaultQueueName);

            builder.RegisterType<StakeReleasedSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _rabbitSettings.Subscribers.PrivateBlockchainRabbitConnString)
                .WithParameter("exchangeName", Context.GetEndpointName<StakeReleasedEvent>())
                .WithParameter("queueName", DefaultQueueName);

            builder.RegisterType<FeeCollectedSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _rabbitSettings.Subscribers.PrivateBlockchainRabbitConnString)
                .WithParameter("exchangeName", Context.GetEndpointName<Job.QuorumTransactionWatcher.Contract.FeeCollectedEvent>())
                .WithParameter("queueName", DefaultQueueName);

            builder.RegisterType<CustomerProfileDeactivationRequestedSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _rabbitSettings.Subscribers.PrivateBlockchainRabbitConnString)
                .WithParameter("exchangeName", CustomerProfileDeactivationRequestedExchangeName)
                .WithParameter("queueName", DefaultQueueName);

            builder.RegisterType<SeizedFromSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _rabbitSettings.Subscribers.PrivateBlockchainRabbitConnString)
                .WithParameter("exchangeName", Context.GetEndpointName<SeizedFromEvent>())
                .WithParameter("queueName", DefaultQueueName);

            if (!_isPublicBlockChainFeatureDisabled)
                RegisterSubscribersForPublicBlockchainFeature(builder);
        }

        private void RegisterSubscribersForPublicBlockchainFeature(ContainerBuilder builder)
        {
            builder.RegisterType<TransferToExternalRequestedSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _rabbitSettings.Subscribers.PrivateBlockchainRabbitConnString)
                .WithParameter("exchangeName", TransferToExternalRequestedEventExchangeName)
                .WithParameter("queueName", DefaultQueueName);

            builder.RegisterType<TransferToInternalDetectedSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _rabbitSettings.Subscribers.PrivateBlockchainRabbitConnString)
                .WithParameter("exchangeName", TransferToInternalDetectedExchangeName)
                .WithParameter("queueName", DefaultQueueName);

            builder.RegisterType<WalletLinkingStatusChangeRequestedSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _rabbitSettings.Subscribers.PrivateBlockchainRabbitConnString)
                .WithParameter("exchangeName", WalletLinkingStatusChangeRequestedExchangeName)
                .WithParameter("queueName", DefaultQueueName);

            builder.RegisterType<SeizeToInternalDetectedSubscriber>()
                .As<IStartStop>()
                .SingleInstance()
                .WithParameter("connectionString", _rabbitSettings.Subscribers.PrivateBlockchainRabbitConnString)
                .WithParameter("exchangeName", SeizeToInternalDetectedExchangeName)
                .WithParameter("queueName", DefaultQueueName);
        }

        private void RegisterPublishers(ContainerBuilder builder)
        {
            builder.RegisterJsonRabbitPublisher<CustomerWalletCreatedEvent>(
                _rabbitSettings.Publishers.CustomerRabbitConnectionString,
                CustomerWalletCreatedExchangeName);

            builder.RegisterJsonRabbitPublisher<BonusRewardDetectedEvent>(
                _rabbitSettings.Publishers.WalletRabbitConnectionString,
                BonusRewardDetectedExchangeName);

            builder.RegisterJsonRabbitPublisher<TransferDetectedEvent>(
                _rabbitSettings.Publishers.WalletRabbitConnectionString,
                TransferDetectedExchange);

            builder.RegisterJsonRabbitPublisher<CustomerBalanceUpdatedEvent>(
                _rabbitSettings.Publishers.WalletRabbitConnectionString,
                CustomerBalanceUpdateExchange);

            builder.RegisterJsonRabbitPublisher<P2PTransferFailedEvent>(
                _rabbitSettings.Publishers.WalletRabbitConnectionString,
                P2PTransferFailedExchange);

            builder.RegisterJsonRabbitPublisher<P2PTransferDetectedEvent>(
                _rabbitSettings.Publishers.WalletRabbitConnectionString,
                P2PTransferDetectedExchange);

            builder.RegisterJsonRabbitPublisher<TransactionFailedEvent>(
                _rabbitSettings.Publishers.WalletRabbitConnectionString,
                TxFailedExchange);

            builder.RegisterJsonRabbitPublisher<TransactionSucceededEvent>(
                _rabbitSettings.Publishers.WalletRabbitConnectionString,
                TxSucceededExchange);

            builder.RegisterJsonRabbitPublisher<WalletStatusChangeFailedEvent>(
                _rabbitSettings.Publishers.WalletRabbitConnectionString,
                WalletStatusChangedExchange);

            builder.RegisterJsonRabbitPublisher<FeeCollectedEvent>(
                _rabbitSettings.Publishers.WalletRabbitConnectionString,
                FeeCollectedExchange);

            builder.RegisterJsonRabbitPublisher<SeizedFromCustomerEvent>(
                _rabbitSettings.Publishers.WalletRabbitConnectionString,
                SeizedFromCustomerExchange);

            builder.RegisterJsonRabbitPublisher<SeizeBalanceFromCustomerCompletedEvent>(
                _rabbitSettings.Publishers.WalletRabbitConnectionString,
                SeizeFromCustomerCompletedExchange);

            builder.RegisterJsonRabbitPublisher<TransferToExternalFailedEvent>(
                _rabbitSettings.Publishers.WalletRabbitConnectionString,
                TransferToExternalFailedExchange);
        }
    }
}
