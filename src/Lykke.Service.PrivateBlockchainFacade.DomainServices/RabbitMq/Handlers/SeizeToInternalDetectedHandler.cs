using System.Threading.Tasks;
using Common.Log;
using Falcon.Numerics;
using Lykke.Common.Log;
using Lykke.Service.PrivateBlockchainFacade.Contract.Operations;
using Lykke.Service.PrivateBlockchainFacade.Domain.Common;
using Lykke.Service.PrivateBlockchainFacade.Domain.Deduplication;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq;
using Lykke.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities.Deduplication;

namespace Lykke.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers
{
    public class SeizeToInternalDetectedHandler : ISeizeToInternalDetectedHandler
    {
        private readonly IOperationRequestsProducer _operationRequestsProducer;
        private readonly IDeduplicationLogRepository<OperationDeduplicationLogEntity> _deduplicationLogRepository;
        private readonly ITransactionScopeHandler _transactionScopeHandler;
        private readonly string _privateBlockchainGatewayContractAddress;
        private readonly ILog _log;

        public SeizeToInternalDetectedHandler(
            IOperationRequestsProducer operationRequestsProducer,
            IDeduplicationLogRepository<OperationDeduplicationLogEntity> deduplicationLogRepository,
            ITransactionScopeHandler transactionScopeHandler,
            string privateBlockchainGatewayContractAddress,
            ILogFactory logFactory)
        {
            _operationRequestsProducer = operationRequestsProducer;
            _deduplicationLogRepository = deduplicationLogRepository;
            _transactionScopeHandler = transactionScopeHandler;
            _privateBlockchainGatewayContractAddress = privateBlockchainGatewayContractAddress;
            _log = logFactory.CreateLog(this);
        }

        public Task HandleAsync(string operationId, Money18 amount, string reason)
        {
            return _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                var isDuplicate = await _deduplicationLogRepository.IsDuplicateAsync(operationId);

                if (isDuplicate)
                {
                    _log.Warning("There is already seize operation with the same operation id",
                        context: $"{nameof(operationId)}: {operationId}");
                    return;
                }

                await _operationRequestsProducer.AddAsync(OperationType.SeizeToInternal,
                    new SeizeToInternalContext(_privateBlockchainGatewayContractAddress, amount, reason));
            });
        }
    }
}
