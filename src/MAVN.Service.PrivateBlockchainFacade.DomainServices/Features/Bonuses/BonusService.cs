using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Numerics;
using MAVN.Service.PrivateBlockchainFacade.Contract.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Common;
using MAVN.Service.PrivateBlockchainFacade.Domain.Deduplication;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Bonuses;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities.Deduplication;
using BonusRewardError = MAVN.Service.PrivateBlockchainFacade.Domain.Features.Bonuses.BonusRewardError;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.Features.Bonuses
{
    public class BonusService : IBonusService
    {
        private readonly ITransactionScopeHandler _transactionScopeHandler;
        private readonly IDeduplicationLogRepository<BonusRewardDeduplicationLogEntity> _deduplicationLog;
        private readonly IOperationRequestsProducer _operationRequestsProducer;
        private readonly IWalletsService _walletsService;
        private readonly string _walletCreationMasterWalletAddress;
        private readonly ILog _log;

        public BonusService(
            IDeduplicationLogRepository<BonusRewardDeduplicationLogEntity> deduplicationLog,
            ITransactionScopeHandler transactionScopeHandler,
            ILogFactory logFactory,
            IOperationRequestsProducer operationRequestsProducer, 
            IWalletsService walletsService, 
            string walletCreationMasterWalletAddress)
        {
            _deduplicationLog = deduplicationLog;
            _transactionScopeHandler = transactionScopeHandler;
            _operationRequestsProducer = operationRequestsProducer;
            _walletsService = walletsService;
            _walletCreationMasterWalletAddress = walletCreationMasterWalletAddress;
            _log = logFactory.CreateLog(this);
        }

        public async Task<BonusRewardResultModel> RewardAsync(string customerId, Money18 amount, string rewardId,
            string bonusReason, string campaignId, string conditionId)
        {
            if (string.IsNullOrEmpty(customerId))
            {
                return BonusRewardResultModel.Failed(BonusRewardError.InvalidCustomerId);
            }

            if (string.IsNullOrEmpty(bonusReason))
            {
                return BonusRewardResultModel.Failed(BonusRewardError.MissingBonusReason);
            }

            if (string.IsNullOrEmpty(campaignId))
            {
                return BonusRewardResultModel.Failed(BonusRewardError.InvalidCampaignId);
            }

            if (amount <= 0)
            {
                return BonusRewardResultModel.Failed(BonusRewardError.InvalidAmount);
            }

            var customerWalletAddressResult = await _walletsService.GetCustomerWalletAsync(customerId);
            switch(customerWalletAddressResult.Error)
            {
                case CustomerWalletAddressError.None:
                    break;
                case CustomerWalletAddressError.CustomerWalletMissing:
                    return BonusRewardResultModel.Failed(BonusRewardError.CustomerWalletMissing);
                case CustomerWalletAddressError.InvalidCustomerId:
                    return BonusRewardResultModel.Failed(BonusRewardError.InvalidCustomerId);
                default:
                    throw new ArgumentOutOfRangeException(nameof(customerWalletAddressResult.Error));
            }
            
            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                var isDuplicate = await _deduplicationLog.IsDuplicateAsync(rewardId);

                if (isDuplicate)
                {
                    _log.Warning("There is already bonus reward operation with the same id", context: rewardId);
                    return BonusRewardResultModel.Failed(BonusRewardError.DuplicateRequest);
                }

                await _operationRequestsProducer.AddAsync(
                    customerId,
                    OperationType.CustomerBonusReward,
                    new CustomerBonusRewardContext
                    {
                        CustomerId = customerId, 
                        Amount = amount, 
                        // todo: temporary solution
                        MinterAddress = _walletCreationMasterWalletAddress, 
                        WalletAddress = customerWalletAddressResult.WalletAddress,
                        RequestId = rewardId,
                        BonusReason = bonusReason,
                        CampaignId = campaignId,
                        ConditionId = conditionId
                    });

                return BonusRewardResultModel.Succeeded();
            });
        }
    }
}
