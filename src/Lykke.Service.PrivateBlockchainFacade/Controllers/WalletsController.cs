using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Lykke.Service.PrivateBlockchainFacade.Controllers
{
    [ApiController]
    [Route("api/wallets")]
    public class WalletsController : ControllerBase, IWalletsApi
    {
        private readonly IWalletsService _walletsService;
        private readonly IMapper _mapper;

        public WalletsController(IWalletsService walletsService, IMapper mapper)
        {
            _walletsService = walletsService;
            _mapper = mapper;
        }

        /// <summary>
        /// Make request to create customer wallet in private blockchain
        /// </summary>
        /// <param name="request">Customer wallet creation details</param>
        /// <returns></returns> 
        [HttpPost]
        [SwaggerOperation("CreateCustomerWallet")]
        [ProducesResponseType(typeof(CustomerWalletCreationResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        public async Task<CustomerWalletCreationResponseModel> CreateAsync([FromBody] CustomerWalletCreationRequestModel request)
        {
            var result = await _walletsService.CreateCustomerWalletAsync(request.CustomerId.ToString());

            return _mapper.Map<CustomerWalletCreationResponseModel>(result);
        }
    }
}
