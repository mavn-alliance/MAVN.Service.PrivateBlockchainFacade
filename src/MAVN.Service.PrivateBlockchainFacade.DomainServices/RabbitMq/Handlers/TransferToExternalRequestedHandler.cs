using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Numerics;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.PrivateBlockchainFacade.Contract.Events;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Fees;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Transfers;
using MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers
{
    public class TransferToExternalRequestedHandler : ITransferToExternalRequestedHandler
    {
        private readonly ITransferService _transferService;
        private readonly IFeesService _feesService;
        private readonly IRabbitPublisher<TransferToExternalFailedEvent> _transferToExternalFailedPublisher;
        private readonly ILog _log;

        public TransferToExternalRequestedHandler(
            ITransferService transferService,
            IFeesService feesService,
            IRabbitPublisher<TransferToExternalFailedEvent> transferToExternalFailedPublisher,
            ILogFactory logFactory)
        {
            _transferService = transferService;
            _feesService = feesService;
            _transferToExternalFailedPublisher = transferToExternalFailedPublisher;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(string operationId, string customerId, Money18 amount, string privateBlochcainGatewayAddress)
        {
            var fee = await _feesService.GetTransfersToPublicFeeAsync();

            var result = await _transferService.TransferToExternalAsync(customerId, privateBlochcainGatewayAddress,
                amount, fee, operationId);

            if (result.Error != TransferError.None)
            {
                _log.Warning(
                    "Unable to create transfer to external when handling TransferToExternalRequested because of error",
                    context: new {customerId, amount, result.Error});

                await _transferToExternalFailedPublisher.PublishAsync(new TransferToExternalFailedEvent
                {
                    CustomerId = customerId,
                    Amount = amount
                });
            }
        }
    }
}
