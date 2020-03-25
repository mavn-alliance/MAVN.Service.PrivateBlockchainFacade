using System.Numerics;
using System.Threading.Tasks;
using Falcon.Numerics;
using Lykke.Service.PrivateBlockchainFacade.Contract.Operations;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.Features.Transfers
{
    public interface ITransferService
    {
        Task<TransferResultModel> P2PTransferAsync(
            string senderId,
            string recipientId,
            Money18 amount,
            string transferRequestId);

        Task<TransferResultModel> GenericTransferAsync(
            string senderId,
            string recipientAddress,
            Money18 amount,
            string transferRequestId,
            string additionalData);

        Task<TransferResultModel> TransferToExternalAsync(
            string senderId,
            string recipientAddress,
            Money18 amount,
            Money18 fee,
            string transferRequestId);

        Task<TransferResultModel> TransferToInternalAsync(
            string privateWalletAddress,
            string publicWalletAddress,
            Money18 amount,
            BigInteger publicTransferId);
    }
}
