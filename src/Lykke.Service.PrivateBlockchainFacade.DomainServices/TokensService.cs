using System.Threading.Tasks;
using Falcon.Numerics;
using Lykke.Service.PrivateBlockchainFacade.Domain.Services;
using Lykke.Service.QuorumOperationExecutor.Client;

namespace Lykke.Service.PrivateBlockchainFacade.DomainServices
{
    public class TokensService : ITokensService
    {
        private readonly IQuorumOperationExecutorClient _executorClient;
        private readonly string _tokenGatewayAddress;

        public TokensService(IQuorumOperationExecutorClient executorClient, string tokenGatewayAddress)
        {
            _executorClient = executorClient;
            _tokenGatewayAddress = tokenGatewayAddress;
        }

        public async Task<Money18> GetTotalTokensSupplyAsync()
        {
            var result = await _executorClient.TokensApi.GetTokensTotalSupplyAsync();

            return result.TotalTokensAmount;
        }

        public async Task<Money18> GetTokenGatewayBalance()
        {
            var result = await _executorClient.AddressesApi.GetBalanceForAddressAsync(_tokenGatewayAddress);

            return result.Balance;
        }
    }
}
