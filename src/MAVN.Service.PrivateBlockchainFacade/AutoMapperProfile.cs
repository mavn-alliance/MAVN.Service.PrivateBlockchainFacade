using AutoMapper;
using MAVN.Service.PrivateBlockchainFacade.Client.Models;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Balances;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Bonuses;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Transfers;
using MAVN.Service.PrivateBlockchainFacade.Domain.Features.Wallets;

namespace MAVN.Service.PrivateBlockchainFacade
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CustomerWalletCreationResultModel, CustomerWalletCreationResponseModel>();
            CreateMap<OperationRequestModel, NewOperationResponseModel>();
            CreateMap<BonusRewardResultModel, BonusRewardResponseModel>();
            CreateMap<CustomerBalanceResultModel, CustomerBalanceResponseModel>();
            CreateMap<AcceptedOperationModel, AcceptedOperationResponseModel>();
            CreateMap<CustomerWalletAddressResultModel, CustomerWalletAddressResponseModel>();
            CreateMap<OperationStatusUpdateResultModel, OperationStatusUpdateResponseModel>();
            CreateMap<TransferResultModel, TransferResponseModel>();
            CreateMap<CustomerIdByWalletResultModel, CustomerIdByWalletAddressResponse>();
        }
    }
}
