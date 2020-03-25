using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.PrivateBlockchainFacade.Contract.Events;
using Lykke.Service.PrivateBlockchainFacade.Contract.Operations;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq;
using Newtonsoft.Json;

namespace Lykke.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers
{
    public class TransactionFailedInBlockchainHandler : ITransactionFailedInBlockchainHandler
    {
        private readonly IOperationStatusUpdater _operationStatusUpdater;
        private readonly IOperationsFetcher _operationsFetcher;
        private readonly IWalletOwnersRepository _walletOwnersRepository;
        private readonly IRabbitPublisher<P2PTransferFailedEvent> _p2PFailedPublisher;
        private readonly IRabbitPublisher<TransactionFailedEvent> _transactionFailedPublisher;
        private readonly IRabbitPublisher<WalletStatusChangeFailedEvent> _walletStatusChangeFailedPublisher;
        private readonly IRabbitPublisher<TransferToExternalFailedEvent> _transferToExternalFailedPublisher;
        private readonly ILog _log;

        public TransactionFailedInBlockchainHandler(
            IOperationStatusUpdater operationStatusUpdater,
            IOperationsFetcher operationsFetcher,
            IWalletOwnersRepository walletOwnersRepository,
            IRabbitPublisher<P2PTransferFailedEvent> p2PFailedPublisher,
            IRabbitPublisher<TransactionFailedEvent> transactionFailedPublisher,
            IRabbitPublisher<WalletStatusChangeFailedEvent> walletStatusChangeFailedPublisher,
            IRabbitPublisher<TransferToExternalFailedEvent> transferToExternalFailedPublisher,
            ILogFactory logFactory)
        {
            _operationStatusUpdater = operationStatusUpdater;
            _operationsFetcher = operationsFetcher;
            _walletOwnersRepository = walletOwnersRepository;
            _p2PFailedPublisher = p2PFailedPublisher;
            _transactionFailedPublisher = transactionFailedPublisher;
            _walletStatusChangeFailedPublisher = walletStatusChangeFailedPublisher;
            _transferToExternalFailedPublisher = transferToExternalFailedPublisher;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(string hash)
        {
            if (string.IsNullOrEmpty(hash))
            {
                _log.Warning("Hash is empty");
                return;
            }

            var statusUpdateResult = await _operationStatusUpdater.FailAsync(hash);
            if (statusUpdateResult.Error == OperationStatusUpdateError.OperationNotFound)
            {
                _log.Info("Operation was not found by hash in PBF. Trying to find it via call to Executor", context:hash);
                statusUpdateResult = await _operationStatusUpdater.SyncWithBlockchainAsync(hash);
            }

            if (statusUpdateResult.Error != OperationStatusUpdateError.None)
            {
                _log.Warning("Operation status was not updated to failed",
                    context: new {hash, error = statusUpdateResult.Error.ToString()});
                return;
            }

            var operation = await _operationsFetcher.GetByHashAsync(hash);

            if (operation == null)
            {
                _log.Info("Failed in BC Operation was not found by hash", context: $"hash: {hash}");
                return;
            }

            await _transactionFailedPublisher.PublishAsync(new TransactionFailedEvent
            {
                OperationId = operation.Id.ToString()
            });

            switch (operation.Type)
            {
                case OperationType.TokensTransfer:
                    await ProcessTransferOperationFailure(operation);
                    break;
                case OperationType.WalletLinking:
                    await ProcessWalletLinkingOperationFailure(operation);
                    break;
                case OperationType.WalletUnlinking:
                    await ProcessWalletUnlinkingOperationFailure(operation);
                    break;
                case OperationType.TransferToExternal:
                    await ProcessTransferToExternalFailure(operation);
                    break;
                case OperationType.CustomerWalletCreation:
                case OperationType.CustomerBonusReward:
                case OperationType.GenericOperation:
                case OperationType.StakeOperation:
                case OperationType.TransferToInternal:
                case OperationType.SetTransferToPublicFee:
                case OperationType.SeizeToInternal:
                    LogErrorForFailure(operation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation.Type));
            }
        }

        private async Task ProcessTransferOperationFailure(IOperation operation)
        {
            var operationContext = JsonConvert.DeserializeObject<TokensTransferContext>(operation.ContextJson);

            var senderWalletOwner =
                await _walletOwnersRepository.GetByWalletAddressAsync(operationContext.SenderWalletAddress);

            //If sender is not a customer this is not a P2P transaction and we shouldn't raise an event
            if (senderWalletOwner == null)
                return;

            var receiverWalletOwner =
                await _walletOwnersRepository.GetByWalletAddressAsync(operationContext.RecipientWalletAddress);

            //If receiver is not a customer this is not a P2P transaction and we shouldn't raise an event
            if (receiverWalletOwner == null)
                return;

            await _p2PFailedPublisher.PublishAsync(new P2PTransferFailedEvent
            {
                TransactionHash = operation.TransactionHash,
                RequestId = operationContext.RequestId,
                Timestamp = operation.Timestamp,
                Amount = operationContext.Amount,
                SenderCustomerId = senderWalletOwner.OwnerId,
                ReceiverCustomerId = receiverWalletOwner.OwnerId
            });
        }

        private Task ProcessWalletLinkingOperationFailure(IOperation operation)
        {
            var operationContext = JsonConvert.DeserializeObject<WalletLinkingContext>(operation.ContextJson);

            return _walletStatusChangeFailedPublisher.PublishAsync(new WalletStatusChangeFailedEvent
            {
                PublicWalletAddress = operationContext.PublicWalletAddress,
                InternalWalletAddress = operationContext.InternalWalletAddress,
            });
        }

        private Task ProcessWalletUnlinkingOperationFailure(IOperation operation)
        {
            var operationContext = JsonConvert.DeserializeObject<WalletUnlinkingContext>(operation.ContextJson);

            return _walletStatusChangeFailedPublisher.PublishAsync(new WalletStatusChangeFailedEvent
            {
                InternalWalletAddress = operationContext.InternalWalletAddress,
            });
        }

        private Task ProcessTransferToExternalFailure(IOperation operation)
        {
            var operationContext = JsonConvert.DeserializeObject<TransferToExternalContext>(operation.ContextJson);

            return _transferToExternalFailedPublisher.PublishAsync(new TransferToExternalFailedEvent
            {
                CustomerId = operation.CustomerId,
                Amount = operationContext.Amount,
            });
        }

        private void LogErrorForFailure(IOperation operation)
        {
            _log.Error(message: "Operation failed in PBF", context: operation);
        }
    }
}
