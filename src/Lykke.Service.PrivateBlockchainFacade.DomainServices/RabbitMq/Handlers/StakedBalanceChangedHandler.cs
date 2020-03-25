using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Balances;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using Lykke.Service.PrivateBlockchainFacade.Domain.RabbitMq;

namespace Lykke.Service.PrivateBlockchainFacade.DomainServices.RabbitMq.Handlers
{
    public class StakedBalanceChangedHandler : IStakedBalanceChangedHandler
    {
        private readonly IBalanceService _balanceService;
        private readonly IWalletOwnersRepository _walletOwnersRepository;
        private readonly ILog _log;

        public StakedBalanceChangedHandler(IBalanceService balanceService, IWalletOwnersRepository walletOwnersRepository, ILogFactory logFactory)
        {
            _balanceService = balanceService;
            _walletOwnersRepository = walletOwnersRepository;
            _log = logFactory.CreateLog(this);
        }

        public async Task HandleAsync(string walletAddress)
        {
            var walletOwnerResponse = await _walletOwnersRepository.GetByWalletAddressAsync(walletAddress);

            if (walletOwnerResponse == null)
            {
                _log.Error(message: "Wallet Owner not found for staked operation", context: walletAddress);
                return;
            }

            await _balanceService.ForceBalanceUpdateAsync(walletOwnerResponse.OwnerId, OperationType.StakeOperation, Guid.Empty);
        }
    }
}
