using System.Threading.Tasks;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;
using Lykke.Service.PrivateBlockchainFacade.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PrivateBlockchainFacade.Controllers
{
    [ApiController]
    [Route("api/tokens")]
    public class TokensController : ControllerBase, ITokensApi
    {
        private readonly ITokensService _tokensService;

        public TokensController(ITokensService tokensService)
        {
            _tokensService = tokensService;
        }

        /// <summary>
        /// Method for getting total amount of tokens in private blockchain
        /// </summary>
        /// <returns>TotalTokensSupplyResponse which holds total tokens amount</returns>
        [HttpGet]
        [Route("total-supply")]
        public async Task<TotalTokensSupplyResponse> GetTotalTokensSupplyAsync()
        {
            var result = await _tokensService.GetTotalTokensSupplyAsync();

            return new TotalTokensSupplyResponse {TotalTokensAmount = result};
        }

        /// <summary>
        /// Method for getting total amount of tokens in gateway contract
        /// </summary>
        /// <returns>TotalTokensSupplyResponse which holds total tokens amount</returns>
        [HttpGet]
        [Route("mvn-gateway/total")]
        public async Task<TotalTokensSupplyResponse> GetMVNGatewayWalletBalance()
        {
            var result = await _tokensService.GetTokenGatewayBalance();

            return new TotalTokensSupplyResponse { TotalTokensAmount = result };
        }

        /// <summary>
        /// Method for getting total amount of tokens in the token gateway contract
        /// </summary>
        /// <returns>TotalTokensSupplyResponse which holds total tokens amount</returns>
        [HttpGet]
        [Route("token-gateway/total")]
        public async Task<TotalTokensSupplyResponse> GetTokenGatewayWalletBalance()
        {
            var result = await _tokensService.GetTokenGatewayBalance();

            return new TotalTokensSupplyResponse { TotalTokensAmount = result };
        }
    }
}
