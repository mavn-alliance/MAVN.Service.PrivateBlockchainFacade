using AutoMapper;
using Lykke.Service.PrivateBlockchainFacade.Client.Models;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Balances;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Bonuses;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Operations;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Transfers;
using Lykke.Service.PrivateBlockchainFacade.Domain.Features.Wallets;

namespace Lykke.Service.PrivateBlockchainFacade
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
