using System.Numerics;
using System.Threading.Tasks;
using Common.Log;
using Falcon.Numerics;
using Lykke.Common.Log;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Transfers;
using Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace Lykke.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers
{
    public class TransferToInternalDetectedHandler : ITransferToInternalDetectedHandler
    {
        private readonly ITransferService _transfersService;
        private readonly ILog _log;

        public TransferToInternalDetectedHandler(ITransferService transfersService, ILogFactory logFactory)
        {
            _transfersService = transfersService;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(BigInteger publicTransferId, string privateWalletAddress, string publicWalletAddress, Money18 amount)
        {
            var result = await _transfersService.TransferToInternalAsync(privateWalletAddress, publicWalletAddress,
                amount, publicTransferId);

            if (result.Error != TransferError.None)
            {
                _log.Error(
                    message:"Unable to create transfer to internal because of error",
                    context: new { privateWalletAddress, publicTransferId, amount, publicWalletAddress, result.Error });
            }
        }
    }
}
