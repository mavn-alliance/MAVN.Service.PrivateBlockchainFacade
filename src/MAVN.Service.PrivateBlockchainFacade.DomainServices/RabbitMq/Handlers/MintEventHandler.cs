using System;
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
    public class MintEventHandler : IMintEventHandler
    {
        private readonly IRabbitPublisher<BonusRewardDetectedEvent> _bonusRewardDetectedPublisher;
        private readonly IWalletOwnersRepository _walletOwnersRepository;
        private readonly IBalanceService _balanceService;
        private readonly IQuorumOperationExecutorClient _executorClient;
        private readonly IOperationsFetcher _operationsFetcher;
        private readonly ILog _log;

        public MintEventHandler(
            IRabbitPublisher<BonusRewardDetectedEvent> bonusRewardDetectedPublisher,
            IWalletOwnersRepository walletOwnersRepository,
            IBalanceService balanceService,
            ILogFactory logFactory, 
            IQuorumOperationExecutorClient executorClient, 
            IOperationsFetcher operationsFetcher)
        {
            _bonusRewardDetectedPublisher = bonusRewardDetectedPublisher;
            _walletOwnersRepository = walletOwnersRepository;
            _balanceService = balanceService;
            _executorClient = executorClient;
            _operationsFetcher = operationsFetcher;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(string transactionHash, Money18 amount, string walletAddress, DateTime observedAt)
        {
            if (string.IsNullOrEmpty(transactionHash))
            {
                _log.Warning("Mint event with empty hash received");
                return;
            }

            if (amount <= 0)
            {
                _log.Warning("Invalid amount for handling MintEvent",
                    context:new {amount, hash = transactionHash});
                return;
            }

            var walletOwner = await _walletOwnersRepository.GetByWalletAddressAsync(walletAddress);
            if (walletOwner == null)
            {
                _log.Error(message: "Mint event for wallet address which does not exist", context: walletAddress);
                return;
            }
            
            var operation = await _operationsFetcher.GetByHashAsync(transactionHash);
            if (operation == null)
            {
                var transactionState = await _executorClient.TransactionsApi.GetTransactionStateAsync(transactionHash);

                if (transactionState.Error != GetTransactionStateError.None)
                {
                    _log.Error(message: "Already processed operation was not found by hash",
                        context: new { hash = transactionHash, walletOwner, walletAddress });
                    return;
                }

                if (!transactionState.OperationId.HasValue)
                {
                    _log.Warning("Operation id is empty", context: new {hash = transactionHash, amount, walletAddress});
                    return;
                }

                operation = await _operationsFetcher.GetByIdAsync(transactionState.OperationId.Value);
                if (operation == null)
                {
                    _log.Error(message: "Already processed operation was not found by id",
                        context: new { id = transactionState.OperationId.Value, transactionHash, walletOwner, walletAddress });
                    return;
                }
            }
            
            await _balanceService.ForceBalanceUpdateAsync(walletOwner.OwnerId, operation.Type, operation.Id);

            var bonusRewardContext = JsonConvert.DeserializeObject<CustomerBonusRewardContext>(operation.ContextJson);

            await _bonusRewardDetectedPublisher.PublishAsync(new BonusRewardDetectedEvent
            {
                Amount = amount,
                Timestamp = observedAt,
                CustomerId = walletOwner.OwnerId,
                RequestId = bonusRewardContext.RequestId,
                BonusReason = bonusRewardContext.BonusReason,
                CampaignId = bonusRewardContext.CampaignId,
                ConditionId = bonusRewardContext.ConditionId
            });
        }
    }
}
