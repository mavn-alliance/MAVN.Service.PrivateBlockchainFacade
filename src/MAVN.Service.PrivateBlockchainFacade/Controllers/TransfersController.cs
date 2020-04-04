using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using MAVN.Service.PrivateBlockchainFacade.Client;
using MAVN.Service.PrivateBlockchainFacade.Client.Models;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Transfers;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MAVN.Service.PrivateBlockchainFacade.Controllers
{
    [ApiController]
    [Route("api/transfers")]
    public class TransfersController : ControllerBase, ITransfersApi
    {
        private readonly ITransferService _transferService;
        private readonly IMapper _mapper;

        public TransfersController(ITransferService transferService, IMapper mapper)
        {
            _transferService = transferService;
            _mapper = mapper;
        }

        /// <summary>
        /// Transfer tokens between customers
        /// </summary>
        /// <param name="request">The transfer details</param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("Transfer")]
        [ProducesResponseType(typeof(TransferResponseModel), (int) HttpStatusCode.Accepted)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        public async Task<TransferResponseModel> TransferAsync(TransferRequestModel request)
        {
            var result = await _transferService.P2PTransferAsync(
                request.SenderCustomerId.ToString(), 
                request.RecipientCustomerId.ToString(), 
                request.Amount,
                request.TransferId);

            return _mapper.Map<TransferResponseModel>(result);
        }
    }
}
