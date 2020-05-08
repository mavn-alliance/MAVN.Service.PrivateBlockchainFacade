using System;
using System.Numerics;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Numerics;
using MAVN.Service.PrivateBlockchainFacade.Contract.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Common;
using MAVN.Service.PrivateBlockchainFacade.Domain.Deduplication;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Balances;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Transfers;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using MAVN.Service.PrivateBlockchainFacade.MsSqlRepositories.Entities.Deduplication;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.Features.Transfers
{
    public class TransferService : ITransferService
    {
        private readonly IWalletsService _walletsService;
        private readonly IBalanceService _balanceService;
        private readonly ITransactionScopeHandler _transactionScopeHandler;
        private readonly IOperationRequestsProducer _operationRequestsProducer;
        private readonly IDeduplicationLogRepository<TransferDeduplicationLogEntity> _deduplicationLog;
        private readonly ILog _log;

        public TransferService(
            IWalletsService walletsService,
            ITransactionScopeHandler transactionScopeHandler,
            IBalanceService balanceService,
            IOperationRequestsProducer operationRequestsProducer,
            IDeduplicationLogRepository<TransferDeduplicationLogEntity> deduplicationLog,
            ILogFactory logFactory)
        {
            _walletsService = walletsService;
            _transactionScopeHandler = transactionScopeHandler;
            _balanceService = balanceService;
            _operationRequestsProducer = operationRequestsProducer;
            _deduplicationLog = deduplicationLog;
            _log = logFactory.CreateLog(this);
        }

        public async Task<TransferResultModel> P2PTransferAsync(
            string senderId,
            string recipientId,
            Money18 amount,
            string transferRequestId)
        {
            #region Validation
            if (string.IsNullOrEmpty(recipientId))
                return TransferResultModel.Failed(TransferError.InvalidRecipientId);

            if (string.IsNullOrEmpty(senderId))
                return TransferResultModel.Failed(TransferError.InvalidSenderId);

            if (amount <= 0)
                return TransferResultModel.Failed(TransferError.InvalidAmount);

            var senderAddressResult = await _walletsService.GetCustomerWalletAsync(senderId);
            var senderAddressValidationResultError = ValidateSenderAddress(senderAddressResult.Error);

            if (senderAddressValidationResultError != TransferError.None)
                return TransferResultModel.Failed(senderAddressValidationResultError);

            var recipientAddressResult = await _walletsService.GetCustomerWalletAsync(recipientId);
            var recipientAddressValidationResultError = ValidateRecipientAddress(recipientAddressResult.Error);

            if (recipientAddressValidationResultError != TransferError.None)
                return TransferResultModel.Failed(recipientAddressValidationResultError);

            var senderBalanceValidationError = await ValidateSenderBalance(senderId, amount);

            if (senderBalanceValidationError != TransferError.None)
                return TransferResultModel.Failed(senderBalanceValidationError);

            #endregion

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                if (!string.IsNullOrEmpty(transferRequestId))
                {
                    var isDuplicate = await _deduplicationLog.IsDuplicateAsync(transferRequestId);

                    if (isDuplicate)
                    {
                        _log.Warning("There is already transfer operation with the same request id", context: transferRequestId);
                        return TransferResultModel.Failed(TransferError.DuplicateRequest);
                    }
                }

                var result = await _operationRequestsProducer.AddAsync(
                    senderId,
                    OperationType.TokensTransfer,
                    new TokensTransferContext
                    {
                        SenderWalletAddress = senderAddressResult.WalletAddress,
                        RecipientWalletAddress = recipientAddressResult.WalletAddress,
                        Amount = amount,
                        RequestId = transferRequestId,
                    },
                    senderAddressResult.WalletAddress);

                return TransferResultModel.Succeeded(result);
            });
        }

        public async Task<TransferResultModel> GenericTransferAsync(
            string senderId,
            string recipientAddress,
            Money18 amount,
            string transferRequestId,
            string additionalData)
        {
            #region Validation
            if (string.IsNullOrEmpty(recipientAddress))
                return TransferResultModel.Failed(TransferError.RecipientWalletMissing);

            if (string.IsNullOrEmpty(senderId))
                return TransferResultModel.Failed(TransferError.InvalidSenderId);

            if (amount <= 0)
                return TransferResultModel.Failed(TransferError.InvalidAmount);

            if (!string.IsNullOrEmpty(additionalData) && !additionalData.StartsWith("0x"))
            {
                return TransferResultModel.Failed(TransferError.InvalidAdditionalDataFormat);
            }

            var senderAddressResult = await _walletsService.GetCustomerWalletAsync(senderId);
            var senderAddressValidationResultError = ValidateSenderAddress(senderAddressResult.Error);

            if (senderAddressValidationResultError != TransferError.None)
                return TransferResultModel.Failed(senderAddressValidationResultError);

            var senderBalanceValidationError = await ValidateSenderBalance(senderId, amount);

            if (senderBalanceValidationError != TransferError.None)
                return TransferResultModel.Failed(senderBalanceValidationError);

            #endregion

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                if (!string.IsNullOrEmpty(transferRequestId))
                {
                    var isDuplicate = await _deduplicationLog.IsDuplicateAsync(transferRequestId);

                    if (isDuplicate)
                    {
                        _log.Warning("There is already transfer operation with the same request id", context: transferRequestId);
                        return TransferResultModel.Failed(TransferError.DuplicateRequest);
                    }
                }

                var result = await _operationRequestsProducer.AddAsync(
                    senderId,
                    OperationType.TokensTransfer,
                    new TokensTransferContext
                    {
                        SenderWalletAddress = senderAddressResult.WalletAddress,
                        RecipientWalletAddress = recipientAddress,
                        Amount = amount,
                        RequestId = transferRequestId,
                        AdditionalData = additionalData
                    },
                    senderAddressResult.WalletAddress);

                return TransferResultModel.Succeeded(result);
            });
        }

        public async Task<TransferResultModel> TransferToExternalAsync(
            string senderId,
            string recipientAddress,
            Money18 amount,
            Money18 fee,
            string transferRequestId)
        {
            #region Validation
            if (string.IsNullOrEmpty(recipientAddress))
                return TransferResultModel.Failed(TransferError.RecipientWalletMissing);

            if (string.IsNullOrEmpty(senderId))
                return TransferResultModel.Failed(TransferError.InvalidSenderId);

            if(fee < 0)
                return TransferResultModel.Failed(TransferError.InvalidFeeAmount);

            if (amount <= 0)
                return TransferResultModel.Failed(TransferError.InvalidAmount);

            var senderAddressResult = await _walletsService.GetCustomerWalletAsync(senderId);
            var senderAddressValidationResultError = ValidateSenderAddress(senderAddressResult.Error);

            if (senderAddressValidationResultError != TransferError.None)
                return TransferResultModel.Failed(senderAddressValidationResultError);

            var totalAmountAndFee = amount + fee;

            var senderBalanceValidationError = await ValidateSenderBalance(senderId, totalAmountAndFee);

            if (senderBalanceValidationError != TransferError.None)
                return TransferResultModel.Failed(senderBalanceValidationError);

            #endregion

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                if (!string.IsNullOrEmpty(transferRequestId))
                {
                    var isDuplicate = await _deduplicationLog.IsDuplicateAsync(transferRequestId);

                    if (isDuplicate)
                    {
                        _log.Warning("There is already transfer to external operation with the same request id", context: transferRequestId);
                        return TransferResultModel.Failed(TransferError.DuplicateRequest);
                    }
                }

                var result = await _operationRequestsProducer.AddAsync(
                    senderId,
                    OperationType.TransferToExternal,
                    new TransferToExternalContext
                    {
                        PrivateWalletAddress = senderAddressResult.WalletAddress,
                        Amount = amount,
                        RecipientContractAddress = recipientAddress,
                    },
                    senderAddressResult.WalletAddress);

                return TransferResultModel.Succeeded(result);
            });
        }

        public async Task<TransferResultModel> TransferToInternalAsync(
            string privateWalletAddress,
            string publicWalletAddress,
            Money18 amount,
            BigInteger publicTransferId)
        {
            #region Validation

            if (string.IsNullOrEmpty(privateWalletAddress))
                return TransferResultModel.Failed(TransferError.PrivateWalletMissing);

            if (string.IsNullOrEmpty(publicWalletAddress))
                return TransferResultModel.Failed(TransferError.PublicWalletMissing);

            if (amount <= 0)
                return TransferResultModel.Failed(TransferError.InvalidAmount);

            #endregion

            return await _transactionScopeHandler.WithTransactionAsync(async () =>
            {
                if (!string.IsNullOrEmpty(publicTransferId.ToString()))
                {
                    var isDuplicate = await _deduplicationLog.IsDuplicateAsync(publicTransferId.ToString());

                    if (isDuplicate)
                    {
                        _log.Warning("There is already transfer to internal operation with the same request id", context: publicTransferId);
                        return TransferResultModel.Failed(TransferError.DuplicateRequest);
                    }
                }

                var result = await _operationRequestsProducer.AddAsync(
                    OperationType.TransferToInternal,
                    new TransferToInternalContext
                    {
                        PrivateWalletAddress = privateWalletAddress,
                        Amount = amount,
                        PublicTransferId = publicTransferId,
                        PublicWalletAddress = publicWalletAddress,
                    });

                return TransferResultModel.Succeeded(result);
            });
        }

        private async Task<TransferError> ValidateSenderBalance(string senderId, Money18 amount)
        {
            var senderBalance = await _balanceService.GetAsync(senderId);
            switch (senderBalance.Error)
            {
                case CustomerBalanceError.InvalidCustomerId:
                    return TransferError.InvalidSenderId;
                case CustomerBalanceError.CustomerWalletMissing:
                    return TransferError.SenderWalletMissing;
                case CustomerBalanceError.None when senderBalance.Total < amount:
                    return TransferError.NotEnoughFunds;
            }
            return TransferError.None;
        }

        private TransferError ValidateSenderAddress(CustomerWalletAddressError walletError)
        {
            switch (walletError)
            {
                case CustomerWalletAddressError.None:
                    return TransferError.None;
                case CustomerWalletAddressError.CustomerWalletMissing:
                    return TransferError.SenderWalletMissing;
                case CustomerWalletAddressError.InvalidCustomerId:
                    return TransferError.InvalidSenderId;
                default:
                    throw new ArgumentOutOfRangeException(nameof(walletError));
            }
        }

        private TransferError ValidateRecipientAddress(CustomerWalletAddressError walletError)
        {
            switch (walletError)
            {
                case CustomerWalletAddressError.None:
                    return TransferError.None;
                case CustomerWalletAddressError.CustomerWalletMissing:
                    return TransferError.RecipientWalletMissing;
                case CustomerWalletAddressError.InvalidCustomerId:
                    return TransferError.InvalidRecipientId;
                default:
                    throw new ArgumentOutOfRangeException(nameof(walletError));
            }
        }
    }
}
