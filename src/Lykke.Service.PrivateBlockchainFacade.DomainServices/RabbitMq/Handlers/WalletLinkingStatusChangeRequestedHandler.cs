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
    public class WalletLinkingStatusChangeRequestedHandler : IWalletLinkingStatusChangeRequestedHandler
    {
        private readonly IOperationRequestsProducer _operationRequestsProducer;
        private readonly IDeduplicationLogRepository<WalletLinkingDeduplicationLogEntity> _deduplicationLog;
        private readonly ITransactionScopeHandler _transactionScopeHandler;
        private readonly ILog _log;

        public WalletLinkingStatusChangeRequestedHandler(
            IOperationRequestsProducer operationRequestsProducer,
            IDeduplicationLogRepository<WalletLinkingDeduplicationLogEntity> deduplicationLog,
            ITransactionScopeHandler transactionScopeHandler,
            ILogFactory logFactory)
        {
            _operationRequestsProducer = operationRequestsProducer;
            _deduplicationLog = deduplicationLog;
            _transactionScopeHandler = transactionScopeHandler;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleWalletLinkingAsync(
            string eventId, string customerId,string masterWalletAddress, string internalAddress, string publicAddress, Money18 fee)
        {
            await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                if (!string.IsNullOrEmpty(eventId))
                {
                    var isDuplicate = await _deduplicationLog.IsDuplicateAsync(eventId);

                    if (isDuplicate)
                    {
                        _log.Warning("There is already wallet linking operation with the same id", context: eventId);
                        return;
                    }
                }

                await _operationRequestsProducer.AddAsync(customerId, OperationType.WalletLinking,
                    new WalletLinkingContext
                    {
                        InternalWalletAddress = internalAddress,
                        PublicWalletAddress = publicAddress,
                        LinkingFee = fee
                    }, masterWalletAddress);
            });
        }

        public async Task HandleWalletUnlinkingAsync(string eventId, string customerId, string masterWalletAddress, string internalAddress)
        {
            await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                if (!string.IsNullOrEmpty(eventId))
                {
                    var isDuplicate = await _deduplicationLog.IsDuplicateAsync(eventId);

                    if (isDuplicate)
                    {
                        _log.Warning("There is already wallet unlinking operation with the same id", context: eventId);
                        return;
                    }
                }

                await _operationRequestsProducer.AddAsync(customerId, OperationType.WalletUnlinking,
                    new WalletUnlinkingContext
                    {
                        InternalWalletAddress = internalAddress,
                    }, masterWalletAddress);
            });
        }
    }
}
