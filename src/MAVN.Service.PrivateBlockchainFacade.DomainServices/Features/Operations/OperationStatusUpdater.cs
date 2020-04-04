using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Service.PrivateBlockchainFacade.Domain.Common;
using MAVN.Service.PrivateBlockchainFacade.Domain.Deduplication;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.DomainServices.Common;
using Lykke.Service.QuorumOperationExecutor.Client;
using Lykke.Service.QuorumOperationExecutor.Client.Models.Responses;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.Features.Operations
{
    public class OperationStatusUpdater : IOperationStatusUpdater
    {
        private readonly ITransactionScopeHandler _transactionScopeHandler;
        private readonly IOperationsRepository _operationsRepository;
        private readonly IOperationRequestsRepository _operationRequestsRepository;
        private readonly IQuorumOperationExecutorClient _executorClient;
        private readonly ILog _log;

        public OperationStatusUpdater(ILogFactory logFactory,
            ITransactionScopeHandler transactionScopeHandler,
            IOperationsRepository operationsRepository,
            IQuorumOperationExecutorClient executorClient, 
            IOperationRequestsRepository operationRequestsRepository)
        {
            _transactionScopeHandler = transactionScopeHandler;
            _operationsRepository = operationsRepository;
            _executorClient = executorClient;
            _operationRequestsRepository = operationRequestsRepository;
            _log = logFactory.CreateLog(this);
        }

        public async Task<OperationStatusUpdateResultModel> AcceptAsync(Guid id, string hash)
        {
            if (Guid.Empty == id)
            {
                _log.Warning("Operation id is empty");
                return OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.OperationNotFound);
            }

            if (string.IsNullOrEmpty(hash))
            {
                _log.Warning("Transaction hash is empty");
                return OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.InvalidTransactionHash);
            }

            if (!hash.IsValidTransactionHash())
            {
                _log.Warning("Transaction hash has invalid format");
                return OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.InvalidTransactionHash);
            }

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                var operationRequest = await _operationRequestsRepository.GetByIdAsync(id);
                if (operationRequest == null)
                {
                    var operation = await _operationsRepository.GetByIdAsync(id);
                    if (operation == null)
                    {
                        _log.Warning("Operation request not found by id", context: id);
                        return OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.OperationNotFound);
                    }

                    if (operation.Status == OperationStatus.Accepted)
                        _log.Warning("Operation has already been accepted", context: id);

                    return OperationStatusUpdateResultModel.Succeeded();
                }

                if (operationRequest.Status != OperationStatus.Created)
                {
                    _log.Error(message: "Current operation request status is not expected",
                        context: new {id = operationRequest.Id, currentStatus = operationRequest.Status.ToString()});
                    return OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.InvalidStatus);
                }

                await _operationRequestsRepository.AcceptAsync(
                    operationRequest.Id,
                    operationRequest.CustomerId,
                    operationRequest.Nonce,
                    operationRequest.MasterWalletAddress,
                    operationRequest.Type,
                    operationRequest.ContextJson,
                    operationRequest.CreatedAt,
                    hash);

                return OperationStatusUpdateResultModel.Succeeded();
            });
        }

        public async Task<OperationStatusUpdateResultModel> AcceptBatchAsync(Dictionary<Guid, string> operationsHashesDict)
        {
            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                var operationRequestsDict = await _operationRequestsRepository.GetByIdsAsync(operationsHashesDict.Keys);
                var notInRequests = operationsHashesDict.Keys.Where(k => !operationRequestsDict.ContainsKey(k)).ToHashSet();

                await _operationRequestsRepository.AcceptBatchAsync(operationRequestsDict.Values, operationsHashesDict);

                if (notInRequests.Any())
                {
                    var operationsIds = await _operationsRepository.GetExistingIdsAsync(notInRequests);
                    foreach (var operationId in operationsIds)
                    {
                        notInRequests.Remove(operationId);
                    }
                    if (notInRequests.Any())
                    {
                        _log.Warning("Operation request not found by id", context: new { missingIds = notInRequests });
                        return OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.OperationNotFound);
                    }
                }

                _log.Info($"Accepted {operationRequestsDict.Count} operationss", new { operationIds = operationRequestsDict .Keys});

                return OperationStatusUpdateResultModel.Succeeded();
            });
        }

        public async Task<OperationStatusUpdateResultModel> FailAsync(string hash)
        {
            if (string.IsNullOrEmpty(hash))
            {
                _log.Warning("Operation hash is empty");
                return OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.OperationNotFound);
            }

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                var operation = await _operationsRepository.GetByHashAsync(hash);
                if (operation == null)
                {
                    _log.Warning("Operation not found by hash", context: hash);
                    return OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.OperationNotFound);
                }

                if (operation.Status == OperationStatus.Failed)
                {
                    _log.Warning("Operation is already failed", context: hash);
                    return OperationStatusUpdateResultModel.Succeeded();
                }

                switch (operation.Status)
                {
                    case OperationStatus.Accepted:
                        await _operationsRepository.SetStatusAsync(operation.Id, OperationStatus.Failed);
                        _log.Info("Operation has been failed", new {operationId = operation.Id});
                        break;
                    case OperationStatus.Created:
                    case OperationStatus.Failed:
                    case OperationStatus.Succeeded:
                        _log.Warning("Operation status can't be updated", context:
                            new
                            {
                                hash,
                                currentStatus = operation.Status.ToString(),
                                desiredStatus = OperationStatus.Failed.ToString()
                            });
                        return OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.InvalidStatus);
                    default:
                        _log.Error(message: "Current operation status is not expected",
                            context: new {hash, currentStatus = operation.Status.ToString()});
                        return OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.InvalidStatus);
                }

                return OperationStatusUpdateResultModel.Succeeded();
            });
        }

        // todo: merge with SyncWithBlockchain into single method like EnsureSucceededAsync, no need to have both
        public async Task<OperationStatusUpdateResultModel> SucceedAsync(string hash)
        {
            if (string.IsNullOrEmpty(hash))
            {
                _log.Warning("Operation hash is empty");
                return OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.OperationNotFound);
            }

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                var operation = await _operationsRepository.GetByHashAsync(hash);
                if (operation == null)
                {
                    _log.Warning("Operation not found by hash", context: hash);
                    return OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.OperationNotFound);
                }

                if (operation.Status == OperationStatus.Succeeded)
                {
                    _log.Warning("Operation is already succeeded", context: hash);
                    return OperationStatusUpdateResultModel.Succeeded();
                }

                switch (operation.Status)
                {
                    case OperationStatus.Accepted:
                        await _operationsRepository.SetStatusAsync(operation.Id, OperationStatus.Succeeded);
                        _log.Info("Operation succeeded", new {operationId = operation.Id});
                        break;
                    case OperationStatus.Created:
                    case OperationStatus.Failed:
                    case OperationStatus.Succeeded:
                        _log.Warning("Operation status can't be updated", context:
                            new
                            {
                                hash,
                                currentStatus = operation.Status.ToString(),
                                desiredStatus = OperationStatus.Succeeded.ToString()
                            });
                        return OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.InvalidStatus);
                    default:
                        _log.Error(message: "Current operation status is not expected",
                            context: new {hash, currentStatus = operation.Status.ToString()});
                        return OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.InvalidStatus);
                }

                return OperationStatusUpdateResultModel.Succeeded();
            });
        }

        public async Task<OperationStatusUpdateResultModel> SyncWithBlockchainAsync(string hash)
        {
            var result = await _executorClient.TransactionsApi.GetTransactionStateAsync(hash);

            if (result.Error == GetTransactionStateError.TransactionNotFound)
                return OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.OperationNotFound);

            if(!result.OperationId.HasValue)
                return OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.OperationIdIsNull);

            var enumValueExists = Enum.TryParse(typeof(OperationStatus),
                result.TransactionState.ToString(), true, out var operationStatus);

            if(!enumValueExists)
                return OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError.UnsupportedOperationStatus);

            try
            {
                // Accepting operation if we haven't accepted it yet
                var acceptResult = await AcceptAsync(result.OperationId.Value, result.TransactionHash);

                if (acceptResult.Error != OperationStatusUpdateError.None)
                    return acceptResult;
                
                await _operationsRepository.SetStatusAsync
                    (result.OperationId.Value, (OperationStatus)operationStatus, result.TransactionHash);
                _log.Info("Operation status and hash have been synced with blockchain",
                    new {operationId = result.OperationId.Value.ToString()});
            }
            catch (DuplicateException e)
            {
                _log.Warning("There is already an operation with the same hash", e, e.Key);
                return OperationStatusUpdateResultModel.Failed(OperationStatusUpdateError
                    .DuplicateTransactionHash);
            }

            return OperationStatusUpdateResultModel.Succeeded();
        }
    }
}
