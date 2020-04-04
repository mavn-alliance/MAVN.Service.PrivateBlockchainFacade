using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Service.PrivateBlockchainFacade.Contract.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Common;
using MAVN.Service.PrivateBlockchainFacade.Domain.Deduplication;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities.Deduplication;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.Features.Operations
{
    public class OperationsService : IOperationsService
    {
        private readonly IOperationRequestsProducer _operationRequestsProducer;
        private readonly IDeduplicationLogRepository<OperationDeduplicationLogEntity> _deduplicationLog;
        private readonly ITransactionScopeHandler _transactionScopeHandler;
        private readonly ILog _log;

        public OperationsService(
            IOperationRequestsProducer operationRequestsProducer,
            IDeduplicationLogRepository<OperationDeduplicationLogEntity> deduplicationLog,
            ITransactionScopeHandler transactionScopeHandler,
            ILogFactory logFactory)
        {
            _operationRequestsProducer = operationRequestsProducer;
            _deduplicationLog = deduplicationLog;
            _transactionScopeHandler = transactionScopeHandler;
            _log = logFactory.CreateLog(this);
        }

        public async Task<GenericOperationResultModel> AddGenericOperationAsync(string operationId, string encodedData, string sourceAddress, string targetAddress)
        {
            if(string.IsNullOrEmpty(encodedData))
                throw new ArgumentNullException(nameof(encodedData));

            if (string.IsNullOrEmpty(sourceAddress))
                throw new ArgumentNullException(nameof(sourceAddress));

            if (string.IsNullOrEmpty(targetAddress))
                throw new ArgumentNullException(nameof(targetAddress));

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                if (!string.IsNullOrEmpty(operationId))
                {
                    var isDuplicate = await _deduplicationLog.IsDuplicateAsync(operationId);

                    if (isDuplicate)
                    {
                        _log.Warning("There is already generic operation with the same id", context: operationId);
                        return GenericOperationResultModel.Failed(AddOperationError.DuplicateRequest);
                    }
                }

                var result = await _operationRequestsProducer.AddAsync(
                    OperationType.GenericOperation,
                    new GenericOperationContext
                    {
                        Data = encodedData,
                        SourceWalletAddress = sourceAddress,
                        TargetWalletAddress = targetAddress
                    }, sourceAddress);

                return GenericOperationResultModel.Succeeded(result);
            });
        }
    }
}
