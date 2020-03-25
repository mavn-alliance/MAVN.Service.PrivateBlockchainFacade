using System;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Fees;
using Microsoft.AspNetCore.Mvc;
using FeesError = Lykke.Service.PrivateBlockchainFacade.Client.FeesError;

namespace Lykke.Service.PrivateBlockchainFacade.Controllers
{
    [ApiController]
    [Route("api/fees")]
    public class FeesController : ControllerBase, IFeesApi
    {
        private readonly IFeesService _feesService;

        public FeesController(IFeesService feesService)
        {
            _feesService = feesService;
        }

        /// <summary>
        /// Get transfers to public fee
        /// </summary>
        /// <returns></returns>
        [HttpGet("transfer-to-public")]
        [ProducesResponseType(typeof(TransferToPublicFeeResponseModel), (int)HttpStatusCode.OK)]
        public async Task<TransferToPublicFeeResponseModel> GetTransferToPublicFeeAsync()
        {
            var fee = await _feesService.GetTransfersToPublicFeeAsync();

            return new TransferToPublicFeeResponseModel{Fee = fee};
        }

        /// <summary>
        /// Set transfers to public fee
        /// </summary>
        /// <param name="request"></param>
        [HttpPost("transfer-to-public")]
        [ProducesResponseType(typeof(SetTransferToPublicFeeResponseModel), (int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<SetTransferToPublicFeeResponseModel> SetTransferToPublicFeeAsync(SetTransferToPublicFeeRequestModel request)
        {
            var result = await _feesService.SetTransfersToPublicFeeAsync(request.Fee);

            return new SetTransferToPublicFeeResponseModel{Error = (FeesError) result};
        }
    }
}
