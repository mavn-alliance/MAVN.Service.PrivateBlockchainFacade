using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Falcon.Numerics;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.PrivateBlockchainFacade.Contract.Events;
using MAVN.Service.PrivateBlockchainFacade.Contract.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Balances;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq;
using Lykke.Service.QuorumOperationExecutor.Client;
using Lykke.Service.QuorumOperationExecutor.Client.Models.Responses;
using Newtonsoft.Json;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers
{
    public class TransferEventHandler : ITransferEventHandler
    {
        private readonly IRabbitPublisher<TransferDetectedEvent> _transferDetectedPublisher;
        private readonly IRabbitPublisher<P2PTransferDetectedEvent> _p2PTransferPublisher;
        private readonly IBalanceService _balanceService;
        private readonly IWalletOwnersRepository _walletOwnersRepository;
        private readonly IOperationsFetcher _operationsFetcher;
        private readonly IQuorumOperationExecutorClient _executorClient;
        private readonly ILog _log;

        public const string EmptyWalletAddress = "0x0000000000000000000000000000000000000000";

        public TransferEventHandler(
            IRabbitPublisher<TransferDetectedEvent> transferDetectedPublisher,
            IRabbitPublisher<P2PTransferDetectedEvent> p2PTransferPublisher,
            ILogFactory logFactory,
            IBalanceService balanceService, 
            IWalletOwnersRepository walletOwnersRepository, 
            IOperationsFetcher operationsFetcher, 
            IQuorumOperationExecutorClient executorClient)
        {
            _transferDetectedPublisher = transferDetectedPublisher;
            _p2PTransferPublisher = p2PTransferPublisher;
            _balanceService = balanceService;
            _walletOwnersRepository = walletOwnersRepository;
            _operationsFetcher = operationsFetcher;
            _executorClient = executorClient;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(
            string sourceAddress, 
            string targetAddress, 
            Money18 amount, 
            string transactionHash,
            DateTime observedAt)
        {
            #region Validation

            if (string.IsNullOrEmpty(sourceAddress))
            {
                _log.Warning("Source address is empty");
                return;
            }

            if (string.IsNullOrEmpty(targetAddress))
            {
                _log.Warning("Target address is empty");
                return;
            }
            
            if (sourceAddress.Equals(EmptyWalletAddress) || targetAddress.Equals(EmptyWalletAddress))
            {
                return;
            }

            if (amount <= 0)
            {
                _log.Warning("Amount is less or equal 0");
                return;
            }

            if (string.IsNullOrEmpty(transactionHash))
            {
                _log.Warning("Transaction hash is empty");
                return;
            }          
            
            #endregion

            #region Find operation
            
            var operation = await _operationsFetcher.GetByHashAsync(transactionHash);
            if (operation == null)
            {
                var transactionState = await _executorClient.TransactionsApi.GetTransactionStateAsync(transactionHash);
                
                if (transactionState.Error != GetTransactionStateError.None)
                {
                    _log.Error(message: "Already processed operation was not found by hash",
                        context: new {transactionHash, sourceAddress, targetAddress});
                    return;
                }

                if (!transactionState.OperationId.HasValue)
                {
                    _log.Warning("Operation id is empty",
                        context: new {transactionHash, sourceAddress, targetAddress});
                    return;
                }
                
                operation = await _operationsFetcher.GetByIdAsync(transactionState.OperationId.Value);
                if (operation == null)
                {
                    _log.Error(message: "Already processed operation was not found by id",
                        context: new
                        {
                            id = transactionState.OperationId.Value, transactionHash, sourceAddress, targetAddress
                        });
                    return;
                }
            }
            
            #endregion
            
            var tasksForBalanceUpdate = new List<Task>();

            var sourceWalletOwner = await _walletOwnersRepository.GetByWalletAddressAsync(sourceAddress);
            if (sourceWalletOwner == null)
            {
                _log.Info("Transfer with unknown source wallet owner", context: sourceAddress);
            }
            else
            {
                tasksForBalanceUpdate.Add(_balanceService.ForceBalanceUpdateAsync(sourceWalletOwner.OwnerId, operation.Type, operation.Id));
            }

            var targetWalletOwner = await _walletOwnersRepository.GetByWalletAddressAsync(targetAddress);
            if (targetWalletOwner == null)
            {
                _log.Info("Transfer with unknown target wallet owner", context: targetAddress);
            }
            else
            {
                tasksForBalanceUpdate.Add(_balanceService.ForceBalanceUpdateAsync(targetWalletOwner.OwnerId, operation.Type, operation.Id));
            }

            await Task.WhenAll(tasksForBalanceUpdate);

            var context = JsonConvert.DeserializeObject<TokensTransferContext>(operation.ContextJson);

            //If there is not customer behind one of the wallets then this is not P2P transfer
            if (sourceWalletOwner != null && targetWalletOwner != null)
            {
                await _p2PTransferPublisher.PublishAsync(new P2PTransferDetectedEvent
                {
                    TransactionHash = transactionHash,
                    SenderCustomerId = sourceWalletOwner.OwnerId,
                    ReceiverCustomerId = targetWalletOwner.OwnerId,
                    Amount = amount,
                    Timestamp = observedAt,
                    RequestId = context.RequestId
                });
            }

            await _transferDetectedPublisher.PublishAsync(new TransferDetectedEvent
            {
                TransactionHash = transactionHash,
                SenderCustomerId = sourceWalletOwner?.OwnerId,
                ReceiverCustomerId = targetWalletOwner?.OwnerId,
                Amount = amount,
                Timestamp = observedAt,
                RequestId = context.RequestId
            });
        }
    }
}
