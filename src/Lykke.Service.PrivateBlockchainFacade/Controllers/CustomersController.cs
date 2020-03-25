using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Balances;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Wallets;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PrivateBlockchainFacade.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomersController : ControllerBase, ICustomersApi
    {
        private readonly IBalanceService _balanceService;
        private readonly IWalletsService _walletsService;
        private readonly IMapper _mapper;

        public CustomersController(
            IBalanceService balanceService,
            IWalletsService walletsService,
            IMapper mapper)
        {
            _balanceService = balanceService;
            _walletsService = walletsService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get customer wallet address balance
        /// </summary>
        /// <param name="customerId">The customer id</param>
        /// <returns></returns>
        [HttpGet("{customerId}/balance")]
        [ProducesResponseType(typeof(CustomerBalanceResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        public async Task<CustomerBalanceResponseModel> GetBalanceAsync(Guid customerId)
        {
            var result = await _balanceService.GetAsync(customerId.ToString());

            return _mapper.Map<CustomerBalanceResponseModel>(result);
        }

        /// <summary>
        /// Get customer wallet address
        /// </summary>
        /// <param name="customerId">The customer id</param>
        /// <returns></returns>
        [HttpGet("{customerId}/walletAddress")]
        [ProducesResponseType(typeof(CustomerWalletAddressResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        public async Task<CustomerWalletAddressResponseModel> GetWalletAddress(Guid customerId)
        {
            var result = await _walletsService.GetCustomerWalletAsync(customerId.ToString());

            return _mapper.Map<CustomerWalletAddressResponseModel>(result);
        }

        /// <summary>
        /// Get customer id by wallet address
        /// </summary>
        /// <param name="walletAddress">BC address of the wallet </param>
        /// <returns></returns>
        [HttpGet("{walletAddress}/customerId")]
        [ProducesResponseType(typeof(CustomerIdByWalletAddressResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<CustomerIdByWalletAddressResponse> GetCustomerIdByWalletAddress(string walletAddress)
        {
            var result = await _walletsService.GetCustomerIdByWalletAsync(walletAddress);

            return _mapper.Map<CustomerIdByWalletAddressResponse>(result);
        }
    }
}
