using Falcon.Numerics;

namespace Lykke.Service.PrivateBlockchainFacade.Domain.Features.Balances
{
    public class CustomerBalanceResultModel
    {
        public Money18 Total { get; set; }

        public Money18 Staked { get; set; }

        public CustomerBalanceError Error { get; private set; }
        
        public static CustomerBalanceResultModel Succeeded(Money18 total, Money18 staked)
        {
            return new CustomerBalanceResultModel
            {
                Error = CustomerBalanceError.None,
                Total = total,
                Staked = staked
            };
        }

        public static CustomerBalanceResultModel Failed(CustomerBalanceError error)
        {
            return new CustomerBalanceResultModel
            {
                Error = error,
                Total = 0
            };       
        }
    }
}
