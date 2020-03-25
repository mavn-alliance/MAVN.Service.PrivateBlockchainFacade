using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.Service.PrivateBlockchainFacade.Contract.Events;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace Lykke.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers
{
    public class TransactionCompletedInBlockchainHandler : ITransactionCompletedInBlockchainHandler
    {
        private readonly IOperationStatusUpdater _operationStatusUpdater;
        private readonly IOperationsFetcher _operationsFetcher;
        private readonly IRabbitPublisher<TransactionSucceededEvent> _transactionSucceededPublisher;
        private readonly ILog _log;

        public TransactionCompletedInBlockchainHandler(
            IOperationStatusUpdater operationStatusUpdater,
            IOperationsFetcher operationsFetcher,
            IRabbitPublisher<TransactionSucceededEvent> transactionSucceededPublisher,
            ILogFactory logFactory)
        {
            _operationStatusUpdater = operationStatusUpdater;
            _operationsFetcher = operationsFetcher;
            _transactionSucceededPublisher = transactionSucceededPublisher;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(string hash)
        {
            if (string.IsNullOrEmpty(hash))
            {
                _log.Warning("Hash is empty");
                return;
            }

            var statusUpdateResult = await _operationStatusUpdater.SucceedAsync(hash);

            if (statusUpdateResult.Error == OperationStatusUpdateError.OperationNotFound)
            {
                _log.Info("Operation was not found by hash in PBF. Trying to find it via call to Executor", context: hash);
                statusUpdateResult = await _operationStatusUpdater.SyncWithBlockchainAsync(hash);
            }

            if (statusUpdateResult.Error != OperationStatusUpdateError.None)
            {
                _log.Warning("Operation status was not updated to succeeded",
                    context: new { hash, error = statusUpdateResult.Error.ToString() });
                return;
            }

            var operation = await _operationsFetcher.GetByHashAsync(hash);

            if (operation == null)
            {
                _log.Info("Succeeded in BC Operation was not found by hash", context: $"hash: {hash}");
                return;
            }

            await _transactionSucceededPublisher.PublishAsync(new TransactionSucceededEvent
            {
                OperationId = operation.Id.ToString()
            });
        }
    }
}
