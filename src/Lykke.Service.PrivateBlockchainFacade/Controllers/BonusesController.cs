using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.PrivateBlockchainFacade.Client;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Bonuses;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.PrivateBlockchainFacade.Controllers
{
    [ApiController]
    [Route("api/bonuses")]
    public class BonusesController : ControllerBase, IBonusesApi
    {
        private readonly IBonusService _bonusService;
        private IMapper _mapper;

        public BonusesController(IBonusService bonusService, IMapper mapper)
        {
            _bonusService = bonusService;
            _mapper = mapper;
        }

        /// <summary>
        /// Reward customer's account with bonuses 
        /// </summary>
        /// <param name="request">Reward request details</param>
        [HttpPost]
        [ProducesResponseType(typeof(BonusRewardResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        public async Task<BonusRewardResponseModel> RewardAsync(BonusRewardRequestModel request)
        {
            var result = await _bonusService.RewardAsync(
                request.CustomerId.ToString(),
                request.Amount,
                request.RewardRequestId,
                request.BonusReason,
                request.CampaignId.ToString(),
                request.ConditionId != Guid.Empty
                    ? request.ConditionId.ToString()
                    : null);

            return _mapper.Map<BonusRewardResponseModel>(result);
        }
    }
}
