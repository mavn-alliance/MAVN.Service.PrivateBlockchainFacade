using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Falcon.Numerics;
using Falcon.Numerics.Linq;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.PrivateBlockchainFacade.Contract.Events;
using MAVN.Service.PrivateBlockchainFacade.Contract.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Balances;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using Lykke.Service.QuorumOperationExecutor.Client;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.Features.Balances
{
    public class BalanceService : IBalanceService
    {
        private readonly IQuorumOperationExecutorClient _quorumOperationExecutorClient;
        private readonly IWalletsService _walletsService;
        private readonly IOperationsFetcher _operationsFetcher;
        private readonly TimeSpan _cacheExpirationPeriod;
        private readonly IRabbitPublisher<CustomerBalanceUpdatedEvent> _customerBalanceUpdatedPublisher;
        private readonly IDistributedCache _distributedCache;
        private readonly ILog _log;

        private static readonly IDictionary<OperationType, CustomerBalanceUpdateReason>
            AllowedOperationTypesToUpdateBalanceMap =
                new Dictionary<OperationType, CustomerBalanceUpdateReason>
                {
                    {OperationType.TokensTransfer, CustomerBalanceUpdateReason.Transfer},
                    {OperationType.CustomerBonusReward, CustomerBalanceUpdateReason.BonusReward},
                    {OperationType.GenericOperation, CustomerBalanceUpdateReason.Transfer},
                    {OperationType.StakeOperation, CustomerBalanceUpdateReason.Stake},
                    {OperationType.WalletLinking, CustomerBalanceUpdateReason.WalletLinking},
                    {OperationType.TransferToExternal, CustomerBalanceUpdateReason.TransferToExternal},
                    {OperationType.TransferToInternal, CustomerBalanceUpdateReason.TransferToInternal},
                    {OperationType.SeizeToInternal, CustomerBalanceUpdateReason.SeizedBalance},
                };

        public BalanceService(
            IQuorumOperationExecutorClient quorumOperationExecutorClient,
            IWalletsService walletsService,
            TimeSpan cacheExpirationPeriod,
            IOperationsFetcher operationsFetcher,
            IRabbitPublisher<CustomerBalanceUpdatedEvent> customerBalanceUpdatedPublisher,
            ILogFactory logFactory, IDistributedCache distributedCache)
        {
            _quorumOperationExecutorClient = quorumOperationExecutorClient;
            _walletsService = walletsService;
            _cacheExpirationPeriod = cacheExpirationPeriod;
            _operationsFetcher = operationsFetcher;
            _customerBalanceUpdatedPublisher = customerBalanceUpdatedPublisher;
            _distributedCache = distributedCache;
            _log = logFactory.CreateLog(this);
        }

        public async Task<CustomerBalanceResultModel> GetAsync(string customerId)
        {
            if (string.IsNullOrEmpty(customerId))
            {
                return CustomerBalanceResultModel.Failed(CustomerBalanceError.InvalidCustomerId);
            }

            return await InternalGetAsync(customerId);
        }

        public async Task<CustomerBalanceResultModel> ForceBalanceUpdateAsync(string customerId, OperationType operationCausedUpdate, Guid operationId)
        {
            if (string.IsNullOrEmpty(customerId))
            {
                return CustomerBalanceResultModel.Failed(CustomerBalanceError.InvalidCustomerId);
            }

            if (!AllowedOperationTypesToUpdateBalanceMap.ContainsKey(operationCausedUpdate))
            {
                throw new InvalidOperationException(
                    $"Attempt to update balance with wrong operation type, operationId={operationId}, operationType={operationCausedUpdate.ToString()}");
            }

            await _distributedCache.RemoveAsync(BuildCacheKey(customerId));

            var balanceResult = await InternalGetAsync(customerId);

            if (balanceResult.Error != CustomerBalanceError.None)
            {
                _log.Error(message: "Couldn't update balance",
                    context: new { customerId, error = balanceResult.Error.ToString() });
            }
            else
            {
                await _customerBalanceUpdatedPublisher.PublishAsync(new CustomerBalanceUpdatedEvent
                {
                    CustomerId = customerId,
                    Balance = balanceResult.Total,
                    Timestamp = DateTime.UtcNow,
                    Reason = AllowedOperationTypesToUpdateBalanceMap[operationCausedUpdate],
                    OperationId = operationId
                });
            }

            return balanceResult;
        }

        private async Task<CustomerBalanceResultModel> InternalGetAsync(string customerId)
        {
            var value = await GetCachedValue(customerId);

            if (value != null)
            {
                return value.DeserializeJson<CustomerBalanceResultModel>();
            }

            var walletAddressResult = await _walletsService.GetCustomerWalletAsync(customerId);
            switch (walletAddressResult.Error)
            {
                case CustomerWalletAddressError.CustomerWalletMissing:
                    return CustomerBalanceResultModel.Failed(CustomerBalanceError.CustomerWalletMissing);
                case CustomerWalletAddressError.InvalidCustomerId:
                    return CustomerBalanceResultModel.Failed(CustomerBalanceError.InvalidCustomerId);
                case CustomerWalletAddressError.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(walletAddressResult.Error));
            }

            var balanceResponse =
                await _quorumOperationExecutorClient.AddressesApi.GetBalanceForAddressAsync(walletAddressResult.WalletAddress);

            var transfersInProgress = await _operationsFetcher.GetTransfersInProgressAsync(walletAddressResult.WalletAddress);

            var transfersInProgressAmount = transfersInProgress
                .Select(x => JsonConvert.DeserializeObject<TokensTransferContext>(x.ContextJson))
                .Sum(x => x.Amount);

            var seizedAmount = (await _operationsFetcher.GetSeizeOperationsInProgressAsync(customerId))
                .Select(x => JsonConvert.DeserializeObject<SeizeToInternalContext>(x.ContextJson))
                .Sum(x => x.Amount);

            var reservedAmount = transfersInProgressAmount + seizedAmount;

            if (balanceResponse.Balance < reservedAmount)
            {
                _log.Warning(
                    $"The reserved amount ({reservedAmount}) is more than actual balance ({balanceResponse.Balance})",
                    context: new { customerId });
            }

            var availableBalance = balanceResponse.Balance - balanceResponse.StakedBalance >= reservedAmount
                ? balanceResponse.Balance - balanceResponse.StakedBalance - reservedAmount
                : 0;

            var result = CustomerBalanceResultModel.Succeeded(availableBalance, balanceResponse.StakedBalance);

            await SetCacheValueAsync(customerId, result);

            return result;
        }

        private string BuildCacheKey(string customerId)
        {
            return $"pbf:customer-balance:{customerId}";
        }

        private async Task SetCacheValueAsync(string customerId, CustomerBalanceResultModel value)
        {
            await _distributedCache.SetStringAsync(BuildCacheKey(customerId),
                value.ToJson(),
                new DistributedCacheEntryOptions { AbsoluteExpiration = DateTime.UtcNow.Add(_cacheExpirationPeriod) });
        }

        private Task<string> GetCachedValue(string customerId)
        {
            return _distributedCache.GetStringAsync(BuildCacheKey(customerId));
        }
    }
}
