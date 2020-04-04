using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using MAVN.Service.PrivateBlockchainFacade.Client;
using MAVN.Service.PrivateBlockchainFacade.Client.Models;
using MAVN.Service.PrivateBlockchainFacade.Contract.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddOperationError = Lykke.Service.PrivateBlockchainFacade.Client.AddOperationError;

namespace MAVN.Service.PrivateBlockchainFacade.Controllers
{
    [ApiController]
    [Route("api/operations")]
    public class OperationsController : ControllerBase, IOperationsApi
    {
        private readonly IOperationsFetcher _operationsFetcher;
        private readonly IOperationStatusUpdater _operationStatusUpdater;
        private readonly IOperationRequestsProducer _operationsRequestsProducer;
        private readonly IOperationsService _operationsService;
        private readonly IMapper _mapper;

        public OperationsController(
            IOperationsFetcher operationsFetcher, 
            IOperationStatusUpdater operationStatusUpdater,
            IOperationRequestsProducer operationsRequestsProducer,
            IOperationsService operationsService,
            IMapper mapper)
        {
            _operationsFetcher = operationsFetcher;
            _operationStatusUpdater = operationStatusUpdater;
            _operationsRequestsProducer = operationsRequestsProducer;
            _operationsService = operationsService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get a list of new operations, maximum is limited
        /// </summary>
        /// <returns></returns>
        [HttpGet("new")]
        [ProducesResponseType(typeof(IEnumerable<NewOperationResponseModel>), (int) HttpStatusCode.OK)]
        public async Task<IEnumerable<NewOperationResponseModel>> GetNewOperationsAsync()
        {
            var operations = await _operationsFetcher.GetRequestsAsync();

            return _mapper.Map<IEnumerable<NewOperationResponseModel>>(operations);
        }

        /// <summary>
        /// Get a list of accepted operations, maximum is limited
        /// </summary>
        /// <returns></returns>
        [HttpGet("accepted")]
        [ProducesResponseType(typeof(IEnumerable<AcceptedOperationResponseModel>), (int) HttpStatusCode.OK)]
        public async Task<IEnumerable<AcceptedOperationResponseModel>> GetAcceptedOperationsAsync()
        {
            var operations = await _operationsFetcher.GetAcceptedAsync();

            return _mapper.Map<IEnumerable<AcceptedOperationResponseModel>>(operations);
        }

        /// <summary>
        /// Operation accepted
        /// </summary>
        /// <param name="id">The operation id</param>
        /// <param name="hash">The operation hash</param>
        /// <returns></returns>
        [Authorize]
        [HttpPut("{id}/accepted")]
        [ProducesResponseType(typeof(OperationStatusUpdateResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        public async Task<OperationStatusUpdateResponseModel> AcceptAsync(Guid id, [Required] string hash)
        {
            var result = await _operationStatusUpdater.AcceptAsync(id, hash);

            return _mapper.Map<OperationStatusUpdateResponseModel>(result);
        }

        /// <summary>
        /// Operation accepted
        /// </summary>
        /// <param name="operationsHashesDict">The operation id to hash dicttionary</param>
        /// <returns></returns>
        [Authorize]
        [HttpPut("accepted")]
        [ProducesResponseType(typeof(OperationStatusUpdateResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<OperationStatusUpdateResponseModel> AcceptBatchAsync([FromBody] Dictionary<Guid, string> operationsHashesDict)
        {
            var result = await _operationStatusUpdater.AcceptBatchAsync(operationsHashesDict);

            return _mapper.Map<OperationStatusUpdateResponseModel>(result);
        }

        /// <summary>
        /// Operation failed
        /// </summary>
        /// <param name="hash">The operation hash</param>
        /// <returns></returns>
        [Authorize]
        [HttpPut("{hash}/failed")]
        [ProducesResponseType(typeof(OperationStatusUpdateResponseModel), (int) HttpStatusCode.OK)]
        public async Task<OperationStatusUpdateResponseModel> FailAsync(string hash)
        {
            var result = await _operationStatusUpdater.FailAsync(hash);

            return _mapper.Map<OperationStatusUpdateResponseModel>(result);
        }

        /// <summary>
        /// Operation succeeded
        /// </summary>
        /// <param name="hash">The operation hash</param>
        [Authorize]
        [HttpPut("{hash}/succeeded")]
        [ProducesResponseType(typeof(OperationStatusUpdateResponseModel), (int) HttpStatusCode.OK)]
        public async Task<OperationStatusUpdateResponseModel> SucceedAsync(string hash)
        {
            var result = await _operationStatusUpdater.SucceedAsync(hash);

            return _mapper.Map<OperationStatusUpdateResponseModel>(result);
        }

        /// <summary>
        /// Endpoint for generic operations which are rerouted to blockchain
        /// </summary>
        /// <param name="request"></param>
        [HttpPost]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<GenericOperationResponse> AddGenericOperationAsync([FromBody] GenericOperationRequest request)
        {
            var result = await _operationsService.AddGenericOperationAsync(request.OperationId, request.Data,
                request.SourceAddress, request.TargetAddress);

            return new GenericOperationResponse
            {
                OperationId = result.OperationId,
                Error = (AddOperationError) result.Error
            };
        }

    }
}
