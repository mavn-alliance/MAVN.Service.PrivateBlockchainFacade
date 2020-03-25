using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations;

namespace Lykke.Service.PrivateBlockchainFacade.DomainServices.Features.Operations
{
    public class OperationRequestsProducer : IOperationRequestsProducer
    {
        private readonly string _walletCreationMasterWalletAddress;
        private readonly IOperationRequestsRepository _operationRequestsRepository;
        private readonly ILog _log;

        public OperationRequestsProducer(
            string walletCreationMasterWalletAddress,
            IOperationRequestsRepository operationRequestsRepository,
            ILogFactory logFactory)
        {
            _walletCreationMasterWalletAddress = walletCreationMasterWalletAddress;
            _operationRequestsRepository = operationRequestsRepository;
            _log = logFactory.CreateLog(this);
        }

        public Task<Guid> AddAsync<T>(
            string customerId,
            OperationType operationType,
            T context,
            string ownerWalletAddress)
        {
            if (string.IsNullOrEmpty(customerId))
            {
                throw new ArgumentNullException(nameof(customerId));
            }

            return AddAsyncImplementation(customerId, operationType, context, ownerWalletAddress);
        }

        public Task<Guid> AddAsync<T>(
            OperationType operationType,
            T context,
            string ownerWalletAddress)
        {
            return AddAsyncImplementation(customerId: null, operationType, context, ownerWalletAddress);
        }

        private async Task<Guid> AddAsyncImplementation<T>(
            string customerId,
            OperationType operationType,
            T context,
            string ownerWalletAddress)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var walletAddress = ownerWalletAddress ?? _walletCreationMasterWalletAddress;

            var nonceCounter = await _operationRequestsRepository.GenerateNextCounterAsync(walletAddress);

            var opId = await _operationRequestsRepository.AddAsync(
                customerId,
                nonceCounter.CounterValue,
                nonceCounter.MasterWalletAddress,
                operationType,
                context.ToJson());

            _log.Info($"Added {operationType} operation", context);

            return opId;
        }
    }
}
