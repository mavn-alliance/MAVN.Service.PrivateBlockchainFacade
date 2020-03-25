using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Transfers;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PrivateBlockchainFacade.Controllers
{
    [ApiController]
    [Route("api/generic-transfers")]
    public class GenericTransfersController : Controller, IGenericTransfersApi
    {
        private readonly ITransferService _transferService;
        private readonly IMapper _mapper;

        public GenericTransfersController(ITransferService transferService, IMapper mapper)
        {
            _transferService = transferService;
            _mapper = mapper;
        }

        /// <summary>
        /// Transfer tokens from customer to specific wallet address
        /// </summary>
        /// <param name="request">The transfer details</param>
        [HttpPost]
        [ProducesResponseType(typeof(TransferResponseModel), (int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<TransferResponseModel> GenericTransferAsync(GenericTransferRequestModel request)
        {
            var result = await _transferService.GenericTransferAsync(
                request.SenderCustomerId,
                request.RecipientAddress,
                request.Amount,
                request.TransferId,
                request.AdditionalData);

            return _mapper.Map<TransferResponseModel>(result);
        }
    }
}
