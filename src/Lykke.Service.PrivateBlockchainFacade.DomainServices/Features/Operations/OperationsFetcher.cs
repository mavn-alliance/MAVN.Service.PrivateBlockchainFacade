using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations;

namespace Lykke.Service.PrivateBlockchainFacade.DomainServices.Features.Operations
{
    public class OperationsFetcher : IOperationsFetcher
    {
        private readonly int _maxNewOperationsAmount;
        private readonly int _maxAcceptedOperationsAmount;
        private readonly IOperationsRepository _operationsRepository;
        private readonly IOperationRequestsRepository _operationRequestsRepository;
        
        private const int MaxNewOperationsDefault = 100;
        private const int MaxAcceptedOperationsDefault = 100;

        public OperationsFetcher(
            int maxNewOperationsAmount, 
            int maxAcceptedOperationsAmount, 
            IOperationsRepository operationsRepository, 
            IOperationRequestsRepository operationRequestsRepository)
        {
            _operationsRepository = operationsRepository;
            _operationRequestsRepository = operationRequestsRepository;

            _maxNewOperationsAmount = maxNewOperationsAmount > 0 
                ? maxNewOperationsAmount 
                : MaxNewOperationsDefault;
            
            _maxAcceptedOperationsAmount = maxAcceptedOperationsAmount > 0
                ? maxAcceptedOperationsAmount
                : MaxAcceptedOperationsDefault;
        }

        public async Task<IReadOnlyList<OperationRequestModel>> GetRequestsAsync()
        {
            var operations =
                await _operationRequestsRepository.GetAsync(_maxNewOperationsAmount);

            return operations.Select(x => new OperationRequestModel
            {
                Id = x.Id,
                MasterWalletAddress = x.MasterWalletAddress,
                Nonce = x.Nonce,
                Type = x.Type,
                PayloadJson = x.ContextJson
            }).ToList(); 
        }

        public async Task<IReadOnlyList<AcceptedOperationModel>> GetAcceptedAsync()
        {
            var operations =
                await _operationsRepository.GetByStatusAsync(OperationStatus.Accepted, _maxAcceptedOperationsAmount);

            return operations.Select(x => new AcceptedOperationModel
            {
                Id = x.Id,
                TransactionHash = x.TransactionHash,
                MasterWalletAddress = x.MasterWalletAddress,
                Nonce = x.Nonce,
                Type = x.Type,
                PayloadJson = x.ContextJson,
                Timestamp = x.Timestamp
            }).ToList();
        }

        public async Task<IReadOnlyList<IOperation>> GetTransfersInProgressAsync(string walletAddress)
        {
            if (string.IsNullOrEmpty(walletAddress))
            {
                throw new ArgumentNullException(nameof(walletAddress));
            }
            
            return await _operationsRepository.GetByConditionAsync(x =>
                x.MasterWalletAddress == walletAddress &&
                x.Type == OperationType.TokensTransfer &&
                x.Status == OperationStatus.Accepted);
        }

        public Task<IOperation> GetByHashAsync(string hash)
        {
            if (string.IsNullOrEmpty(hash))
            {
                throw new ArgumentNullException(nameof(hash));
            }

            return _operationsRepository.GetByHashAsync(hash);
        }

        public async Task<IOperation> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await _operationRequestsRepository.GetByIdAsync(id) ?? 
                   await _operationsRepository.GetByIdAsync(id);
        }

        public async Task<IReadOnlyList<IOperation>> GetSeizeOperationsInProgressAsync(string customerId)
        {
            if (string.IsNullOrEmpty(customerId))
            {
                throw new ArgumentNullException(nameof(customerId));
            }

            return await _operationsRepository.GetByConditionAsync(x =>
                x.CustomerId == customerId &&
                x.Type == OperationType.SeizeToInternal &&
                x.Status == OperationStatus.Accepted);
        }
    }
}
