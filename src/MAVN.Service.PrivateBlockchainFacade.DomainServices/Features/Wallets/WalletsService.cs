using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using MAVN.Service.PrivateBlockchainFacade.Contract.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Common;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using MAVN.Service.QuorumTransactionSigner.Client;
using CustomerWalletCreationError = MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets.CustomerWalletCreationError;

namespace MAVN.Service.PrivateBlockchainFacade.DomainServices.Features.Wallets
{
    public class WalletsService : IWalletsService
    {
        private readonly ITransactionScopeHandler _transactionScopeHandler;
        private readonly IWalletOwnersRepository _walletOwnersRepository;
        private readonly IQuorumTransactionSignerClient _quorumTransactionSignerClient;
        private readonly IOperationRequestsProducer _operationRequestsProducer;
        private readonly ILog _log;

        public WalletsService(
            ILogFactory logFactory,
            ITransactionScopeHandler transactionScopeHandler,
            IWalletOwnersRepository walletOwnersRepository,
            IQuorumTransactionSignerClient quorumTransactionSignerClient, 
            IOperationRequestsProducer operationRequestsProducer)
        {
            _transactionScopeHandler = transactionScopeHandler;
            _walletOwnersRepository = walletOwnersRepository;
            _quorumTransactionSignerClient = quorumTransactionSignerClient;
            _operationRequestsProducer = operationRequestsProducer;
            _log = logFactory.CreateLog(this);
        }

        public async Task<CustomerWalletCreationResultModel> CreateCustomerWalletAsync(string customerId)
        {
            #region Validation

            if (string.IsNullOrEmpty(customerId))
            {
                return CustomerWalletCreationResultModel.Failed(CustomerWalletCreationError.InvalidCustomerId);
            }

            var walletAlreadyAssigned = await _walletOwnersRepository.GetByOwnerIdAsync(customerId);
            if (walletAlreadyAssigned != null)
            {
                _log.Info("Customer already has wallet assigned", walletAlreadyAssigned);
                return CustomerWalletCreationResultModel.Failed(CustomerWalletCreationError.AlreadyCreated);
            }

            #endregion

            var createWalletResponse = await _quorumTransactionSignerClient.WalletsApi.CreateWalletAsync();
            
            try
            {
                await _transactionScopeHandler.WithTransactionAsync(async () =>
                {
                    await _walletOwnersRepository.AddAsync(customerId, createWalletResponse.Address);

                    await _operationRequestsProducer.AddAsync(
                        customerId,
                        OperationType.CustomerWalletCreation,
                        new CustomerWalletContext
                        {
                            CustomerId = customerId, WalletAddress = createWalletResponse.Address
                        });
                });
            }
            catch (Exception e)
            {
                if (e is WalletOwnerDuplicateException)
                    return CustomerWalletCreationResultModel.Failed(CustomerWalletCreationError.AlreadyCreated);

                throw;
            }

            return CustomerWalletCreationResultModel.Succeeded();
        }

        public async Task<CustomerWalletAddressResultModel> GetCustomerWalletAsync(string customerId)
        {
            if (string.IsNullOrEmpty(customerId))
                return CustomerWalletAddressResultModel.Failed(CustomerWalletAddressError.InvalidCustomerId);

            var walletOwner = await _walletOwnersRepository.GetByOwnerIdAsync(customerId);

            if (string.IsNullOrEmpty(walletOwner?.WalletId))
                return CustomerWalletAddressResultModel.Failed(CustomerWalletAddressError.CustomerWalletMissing);

            return CustomerWalletAddressResultModel.Succeeded(walletOwner.WalletId);
        }

        public async Task<CustomerIdByWalletResultModel> GetCustomerIdByWalletAsync(string walletAddress)
        {
            if (string.IsNullOrEmpty(walletAddress))
                return CustomerIdByWalletResultModel.Failed(CustomerWalletAddressError.InvalidWalletAddress);

            var walletOwner = await _walletOwnersRepository.GetByWalletAddressAsync(walletAddress);

            if (string.IsNullOrEmpty(walletOwner?.OwnerId))
                return CustomerIdByWalletResultModel.Failed(CustomerWalletAddressError.CustomerWalletMissing);

            return CustomerIdByWalletResultModel.Succeeded(walletOwner.OwnerId);
        }
    }
}
