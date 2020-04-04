using System;
using System.Threading.Tasks;
using Common.Log;
using Falcon.Numerics;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.PrivateBlockchainFacade.Contract.Events;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using MAVN.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers
{
    public class FeeCollectedHandler : IFeeCollectedHandler
    {
        private const string TransferToPublicFeeReason = "Fee for transfer to Ethereum network";
        private const string WalletLinkingFeeReason = "Fee for Ethereum account linking";

        private readonly IWalletOwnersRepository _walletOwnersRepository;
        private readonly IRabbitPublisher<FeeCollectedEvent> _feeCollectedPublisher;
        private readonly ILog _log;

        public FeeCollectedHandler(
            IWalletOwnersRepository walletOwnersRepository,
            IRabbitPublisher<FeeCollectedEvent> feeCollectedPublisher,
            ILogFactory logFactory)
        {
            _walletOwnersRepository = walletOwnersRepository;
            _feeCollectedPublisher = feeCollectedPublisher;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(string eventId, string walletAddress, string reason, Money18 amount, string transactionHash)
        {
            #region Validation

            if (string.IsNullOrEmpty(eventId))
            {
                _log.Error(message: "Fee collected event without eventId",
                    context: new { walletAddress, reason, amount });
                return;
            }

            if (string.IsNullOrEmpty(walletAddress))
            {
                _log.Error(message: "Fee collected event without walletAddress",
                    context: new { eventId, reason, amount });
                return;
            }

            if (string.IsNullOrEmpty(reason))
            {
                _log.Error(message: "Fee collected event without reason",
                    context: new { eventId, walletAddress, amount });
                return;
            }

            if (amount < 0)
            {
                _log.Error(message: "Fee collected event with invalid fee amount",
                    context: new { eventId, walletAddress, amount, reason });
                return;
            }

            #endregion

            var walletOwner = await _walletOwnersRepository.GetByWalletAddressAsync(walletAddress);

            if (walletOwner == null)
            {
                _log.Error(message: "Fee collected event with wallet address which does not match any of ours",
                    context: new { eventId, walletAddress, amount, reason });
                return;
            }

            await _feeCollectedPublisher.PublishAsync(new FeeCollectedEvent
            {
                CustomerId = walletOwner.OwnerId,
                EventId = eventId,
                Amount = amount,
                WalletAddress = walletAddress,
                TransactionHash = transactionHash,
                Reason = GetFeeCollectedReason(reason)
            });
        }

        private FeeCollectedReason GetFeeCollectedReason(string reason)
        {
            switch (reason)
            {
                case TransferToPublicFeeReason:
                    return FeeCollectedReason.TransferToPublic;
                case WalletLinkingFeeReason:
                    return FeeCollectedReason.WalletLinking;
                default: throw new ArgumentOutOfRangeException(nameof(reason),"Invalid fee collected reason");
            }
        }
    }
}
